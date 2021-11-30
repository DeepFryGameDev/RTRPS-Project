using System;
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
    [SerializeField] [Range(1f, 100f)] float scrollFactor;
    [SerializeField] [Range(1f, 50f)] float dragSpeed;
    [SerializeField] [Range(1f, 10f)] float minScroll;
    [SerializeField] [Range(10f, 25f)] float maxScroll;    

    public Terrain terrainMap;
    [SerializeField] Transform camTransform;

    // for drag movement
    bool rightClickDrag;
    Vector3 dragOrigin, newDragPos, panOrigin;

    // for keeping camera above terrain
    float scrollDist, minScrollClamp, maxScrollClamp;

    // for cursor management
    enum cursorModes
    {
        IDLE,
        PANUP,
        PANDOWN,
        PANLEFT,
        PANRIGHT,
        PANDIAGUL,
        PANDIAGUR,
        PANDIAGDL,
        PANDIAGDR,
        DRAG
    }
    cursorModes cursorMode;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        cursorMode = cursorModes.IDLE;
        SetCursor();

        scrollDist = camTransform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        ZoomCamera();

        if (!rightClickDrag)
        {
            PanCamera();
        }    

        DragCamera();

        KeepCameraAtTerrainHeight();

        SetCursor();
    }

    private void KeepCameraAtTerrainHeight()
    {
        float setHeight = Terrain.activeTerrain.SampleHeight(camTransform.position);

        if (GetComponent<NavInterface>().terrainHeight > setHeight)
        {
            setHeight = GetComponent<NavInterface>().terrainHeight;
        }

        float newHeight = (setHeight + scrollDist) - GetComponent<NavInterface>().terrainHeight;

        camTransform.position = new Vector3(camTransform.position.x, newHeight, camTransform.position.z);
        minScrollClamp = setHeight + minScroll;
        maxScrollClamp = setHeight + maxScroll;
    }

    void PanCamera()
    {
        Vector3 pos = camTransform.position;
        bool movingZU = false, movingZD = false, movingXL = false, movingXR = false;

        if (cursorMode != cursorModes.IDLE)
        {
            cursorMode = cursorModes.IDLE;
        }

        //up
        if (Input.GetAxis("Vertical") > 0)
        {
            pos.z += panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Vertical")) * Time.deltaTime;
        } else if (Input.mousePosition.y >= Screen.height - panBorderSize && Input.mousePosition.y <= Screen.height)
        {
            movingZU = true;
            cursorMode = cursorModes.PANUP;

            float position = (Input.mousePosition.y - (Screen.height - panBorderSize));
            float panSpeed = panFactor * (position / panBorderSize);

            pos.z += panSpeed * Time.deltaTime;
        }

        //down
        if (Input.GetAxis("Vertical") < 0)
        {
            pos.z -= panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Vertical")) * Time.deltaTime;
        }
        else if (Input.mousePosition.y <= panBorderSize && (Input.mousePosition.y - panBorderSize) >= -panBorderSize)
        {
            movingZD = true;
            cursorMode = cursorModes.PANDOWN;

            float position = (Input.mousePosition.y - panBorderSize);
            float panSpeed = panFactor * Mathf.Abs(-position / -panBorderSize);

            pos.z -= panSpeed * Time.deltaTime;
        }

        //right
        if (Input.GetAxis("Horizontal") > 0)
        {
            pos.x += panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Horizontal")) * Time.deltaTime;
        }
        else if (Input.mousePosition.x >= Screen.width - panBorderSize && Input.mousePosition.x <= Screen.width)
        {
            movingXR = true;
            cursorMode = cursorModes.PANRIGHT;

            float position = (Input.mousePosition.x - (Screen.width - panBorderSize));
            float panSpeed = panFactor * (position / panBorderSize);

            pos.x += panSpeed * Time.deltaTime;
        }

        //left
        if (Input.GetAxis("Horizontal") < 0)
        {
            pos.x -= panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Horizontal")) * Time.deltaTime;
        }
        else if (Input.mousePosition.x <= panBorderSize && (Input.mousePosition.x - panBorderSize) >= -panBorderSize)
        {
            movingXL = true;
            cursorMode = cursorModes.PANLEFT;

            float position = (Input.mousePosition.x - panBorderSize);
            float panSpeed = panFactor * Mathf.Abs(-position / -panBorderSize);

            pos.x -= panSpeed * Time.deltaTime;
        }

        if (movingZU && movingXR)
        {
            cursorMode = cursorModes.PANDIAGUR;
        } else if (movingZU && movingXL)
        {
            cursorMode = cursorModes.PANDIAGUL;
        } else if (movingZD && movingXR)
        {
            cursorMode = cursorModes.PANDIAGDR;
        } else if (movingZD && movingXL)
        {
            cursorMode = cursorModes.PANDIAGDL;
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

            float temp = pos.y;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            pos.y -= scroll * (scrollSpeed * scrollFactor) * Time.deltaTime;

            pos.y = Mathf.Clamp(pos.y, minScrollClamp, maxScrollClamp);

            scrollDist += (pos.y - temp);

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
            camTransform.position = new Vector3(
                dragOrigin.x + -pos.x * dragSpeed, 
                GetComponent<NavInterface>().cameraDistance, 
                dragOrigin.z + -pos.y * dragSpeed);
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
            case cursorModes.PANDIAGUL:
                cursorIcon = GetComponent<NavCursorIcons>().panDiagUL;
                break;
            case cursorModes.PANDIAGUR:
                cursorIcon = GetComponent<NavCursorIcons>().panDiagUR;
                break;
            case cursorModes.PANDIAGDL:
                cursorIcon = GetComponent<NavCursorIcons>().panDiagDL;
                break;
            case cursorModes.PANDIAGDR:
                cursorIcon = GetComponent<NavCursorIcons>().panDiagDR;
                break;
            case cursorModes.DRAG:
                cursorIcon = GetComponent<NavCursorIcons>().panDrag;
                break;
        }

        Cursor.SetCursor(cursorIcon, Vector2.zero, CursorMode.Auto);
    }
}
