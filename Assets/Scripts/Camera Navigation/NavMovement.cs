using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMovement : MonoBehaviour
{
    [SerializeField] bool debugging;

    [SerializeField] [Range(1f, 150f)] float panBorderSize;
    [SerializeField] [Range(1f, 50f)] float panFactor;
    [SerializeField] [Range(1f, 100f)] float panSpeedOnKeyPress;
    [SerializeField] [Range(1f, 100f)] float scrollSpeed;
    [SerializeField] [Range(1f, 50f)] float dragSpeed;
    [SerializeField] [Range(1f, 10f)] float minScroll;
    [SerializeField] [Range(10f, 25f)] float maxScroll;    

    [SerializeField] Terrain terrainMap;
    [SerializeField] Transform camTransform;

    // for drag movement
    bool rightClickDrag;
    Vector3 dragOrigin, newDragPos, panOrigin;

    // for cursor management
    enum cursorModes
    {
        IDLE,
        PANUP,
        PANDOWN,
        PANLEFT,
        PANRIGHT,
        PANDIAG,
        DRAG
    }
    cursorModes cursorMode;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        cursorMode = cursorModes.IDLE;
        SetCursor();
    }

    // Update is called once per frame
    void Update()
    {
        if (!rightClickDrag)
        {
            PanCamera();
        }
        
        ZoomCamera();

        DragCamera();

        SetCursor();
    }

    void PanCamera()
    {
        Vector3 pos = camTransform.position;
        bool movingZ = false, movingX = false;

        if (cursorMode != cursorModes.IDLE)
        {
            cursorMode = cursorModes.IDLE;
        }

        //up
        if (Input.GetAxis("Vertical") > 0)
        {
            movingZ = true;
            pos.z += panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Vertical")) * Time.deltaTime;
        } else if (Input.mousePosition.y >= Screen.height - panBorderSize && Input.mousePosition.y <= Screen.height)
        {
            movingZ = true;
            cursorMode = cursorModes.PANUP;

            float position = (Input.mousePosition.y - (Screen.height - panBorderSize));
            float panSpeed = panFactor * (position / panBorderSize);

            pos.z += panSpeed * Time.deltaTime;
        }

        //down
        if (Input.GetAxis("Vertical") < 0)
        {
            movingZ = true;
            pos.z -= panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Vertical")) * Time.deltaTime;
        }
        else if (Input.mousePosition.y <= panBorderSize && (Input.mousePosition.y - panBorderSize) >= -panBorderSize)
        {
            movingZ = true;
            cursorMode = cursorModes.PANDOWN;

            float position = (Input.mousePosition.y - panBorderSize);
            float panSpeed = panFactor * Mathf.Abs(-position / -panBorderSize);

            pos.z -= panSpeed * Time.deltaTime;
        }

        //right
        if (Input.GetAxis("Horizontal") > 0)
        {
            movingX = true;
            pos.x += panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Horizontal")) * Time.deltaTime;
        }
        else if (Input.mousePosition.x >= Screen.width - panBorderSize && Input.mousePosition.x <= Screen.width)
        {
            movingX = true;
            cursorMode = cursorModes.PANRIGHT;

            float position = (Input.mousePosition.x - (Screen.width - panBorderSize));
            float panSpeed = panFactor * (position / panBorderSize);

            pos.x += panSpeed * Time.deltaTime;
        }

        //left
        if (Input.GetAxis("Horizontal") < 0)
        {
            movingX = true;
            pos.x -= panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Horizontal")) * Time.deltaTime;
        }
        else if (Input.mousePosition.x <= panBorderSize && (Input.mousePosition.x - panBorderSize) >= -panBorderSize)
        {
            movingX = true;
            cursorMode = cursorModes.PANLEFT;

            float position = (Input.mousePosition.x - panBorderSize);
            float panSpeed = panFactor * Mathf.Abs(-position / -panBorderSize);

            pos.x -= panSpeed * Time.deltaTime;
        }

        if (movingZ && movingX)
        {
            cursorMode = cursorModes.PANDIAG;
        }

        pos.x = Mathf.Clamp(pos.x, -(terrainMap.terrainData.size.x/2), (terrainMap.terrainData.size.x/2));
        pos.z = Mathf.Clamp(pos.z, -(terrainMap.terrainData.size.z/2), (terrainMap.terrainData.size.z/2));

        camTransform.position = pos;
    }

    void ZoomCamera()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Vector3 pos = camTransform.position;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            pos.y -= scroll * (scrollSpeed * 10) * Time.deltaTime;

            pos.y = Mathf.Clamp(pos.y, minScroll, maxScroll);

            camTransform.position = pos;
        }        
    }

    void DragCamera() // Credit to Grimshad - https://answers.unity.com/questions/827834/click-and-drag-camera.html
    {
        if (Input.GetMouseButtonDown(1))
        {
            rightClickDrag = true;
            dragOrigin = camTransform.position;
            panOrigin = Camera.main.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camTransform.gameObject.GetComponent<Camera>().nearClipPlane));
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camTransform.gameObject.GetComponent<Camera>().nearClipPlane)) - panOrigin;
            camTransform.position = new Vector3(dragOrigin.x + -pos.x * dragSpeed, camTransform.position.y, dragOrigin.z + -pos.y * dragSpeed);

            newDragPos = camTransform.position;

            if (dragOrigin != newDragPos)
            {
                cursorMode = cursorModes.DRAG;
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            rightClickDrag = false;
        }
    }

    void SetCursor()
    {
        Texture2D cursorIcon = null;

        switch (cursorMode)
        {
            case cursorModes.IDLE:
                cursorIcon = GetComponent<NavCursorIcons>().idle;
                break;
            case cursorModes.PANUP:
                cursorIcon = GetComponent<NavCursorIcons>().panUp;
                break;
            case cursorModes.PANDOWN:
                cursorIcon = GetComponent<NavCursorIcons>().panDown;
                break;
            case cursorModes.PANLEFT:
                cursorIcon = GetComponent<NavCursorIcons>().panLeft;
                break;
            case cursorModes.PANRIGHT:
                cursorIcon = GetComponent<NavCursorIcons>().panRight;
                break;
            case cursorModes.PANDIAG:
                cursorIcon = GetComponent<NavCursorIcons>().panDiagonal;
                break;
            case cursorModes.DRAG:
                cursorIcon = GetComponent<NavCursorIcons>().panDrag;
                break;
        }

        Cursor.SetCursor(cursorIcon, Vector2.zero, CursorMode.Auto);
    }
}
