using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavMovement : MonoBehaviour
{
    [Tooltip("How close the cursor is to the edge of the screen to pan the camera.")]
    [SerializeField] [Range(1f, 150f)] float panBorderSize;
    [Tooltip("The speed of camera panning when cursor is on the edge of the screen.")]
    [SerializeField] [Range(1f, 10f)] float panSpeed;
    [Tooltip("A higher value results in a bigger difference in pan speed depending on where in the pan border the cursor is in.")]
    [SerializeField] [Range(1f, 50f)] float panFactor;
    [Tooltip("How fast the camera pans when using movement keys (WASD/Arrow keys).")]
    [SerializeField] [Range(1f, 100f)] float panSpeedOnKeyPress;
    [Tooltip("How fast the camera pans when using movement keys and holding sprint button (Left Shift).")]
    [SerializeField] [Range(1f, 100f)] float panSpeedOnSprintPress;
    [Tooltip("Multiplied by the scrollFactor to determine how quickly to zoom using middle mouse button.")]
    [SerializeField] [Range(1f, 100f)] float scrollSpeed;
    [Tooltip("Multiplied by the scrollSpeed to determine how quickly to zoom using middle mouse button.")]
    [SerializeField] [Range(1f, 100f)] float scrollFactor;
    [Tooltip("A larger value represents a larger jump to world position from where the right mouse button was originally clicked.")]
    [SerializeField] [Range(0.1f, 1.0f)] float dragFactor;
    [Tooltip("How fast to move the camera when dragging after holding right mouse button.")]
    [SerializeField] [Range(1f, 50f)] float dragSpeed;
    [Tooltip("Closest y position the camera is able to zoom into.")]
    [SerializeField] [Range(1f, 10f)] float minScroll;
    [Tooltip("Farthest y position the camera is able to zoom out to.")]
    [SerializeField] [Range(10f, 50f)] float maxScroll;
    [Tooltip("How quickly the camera rotates when holding down the scroll wheel.")]
    [SerializeField] [Range(1f, 20f)] float rotateSpeed;

    Terrain terrainMap;
    Transform camTransform;

    // for drag movement
    bool rightClickDrag, overUI;
    UIProcessing uip;
    float lastMouseX, lastMouseY;

    // for rotation
    bool isRotating;

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
        DRAG,
        ROTATE
    }
    cursorModes cursorMode;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        cursorMode = cursorModes.IDLE;
        SetCursor();

        camTransform = Camera.main.transform;
        scrollDist = camTransform.position.y;

        uip = GameObject.Find("UI").GetComponent<UIProcessing>();
        terrainMap = Terrain.activeTerrain;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMouseOverGameWindow)
        {
            ZoomCamera();

            if (!rightClickDrag)
            {
                PanCamera();
            }

            DragCamera();

            RotateCamera();

            KeepCameraAtTerrainHeight();

            KeepCameraInBounds();

            SetCursor();
        }        
    }

    void KeepCameraInBounds()
    {
        if (camTransform.position.x < -(terrainMap.terrainData.size.x / 2))
        {
            camTransform.position = new Vector3(-(terrainMap.terrainData.size.x / 2), camTransform.position.y, camTransform.position.z);
        }

        if (camTransform.position.x > (terrainMap.terrainData.size.x / 2))
        {
            camTransform.position = new Vector3((terrainMap.terrainData.size.x / 2), camTransform.position.y, camTransform.position.z);
        }

        if (camTransform.position.z < -(terrainMap.terrainData.size.z / 2))
        {
            camTransform.position = new Vector3(camTransform.position.x, camTransform.position.y, -(terrainMap.terrainData.size.z / 2));
        }

        if (camTransform.position.z > (terrainMap.terrainData.size.z / 2))
        {
            camTransform.position = new Vector3(camTransform.position.x, camTransform.position.y, (terrainMap.terrainData.size.z / 2));
        }
    }

    void RotateCamera()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            isRotating = true;
        }

        if (Input.GetKey(KeyCode.Mouse2))
        {
            Vector3 rotatePoint = new Vector3(camTransform.position.x, Terrain.activeTerrain.SampleHeight(camTransform.position), camTransform.position.z);

            camTransform.RotateAround(rotatePoint, transform.up, -Input.GetAxis("Mouse X") * rotateSpeed);
        }

        if (Input.GetKeyUp(KeyCode.Mouse2))
        {
            isRotating = false;
        }
    }

    void KeepCameraAtTerrainHeight()
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

        float keyPanSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            keyPanSpeed = panSpeedOnSprintPress;
        }
        else
        {
            keyPanSpeed = panSpeedOnKeyPress;
        }

        //up
        if (Input.GetAxis("Vertical") > 0)
        {
            camTransform.Translate(Vector3.forward * (keyPanSpeed * Mathf.Abs(Input.GetAxis("Vertical")) * Time.deltaTime));

        }
        else if (Input.mousePosition.y >= Screen.height - panBorderSize && Input.mousePosition.y <= Screen.height)
        {
            movingZU = true;
            cursorMode = cursorModes.PANUP;

            float position = (Input.mousePosition.y - (Screen.height - panBorderSize));
            float speed = panFactor * panSpeed * (position / panBorderSize);

            camTransform.Translate(Vector3.forward * (speed * Time.deltaTime));
        }

        //down
        if (Input.GetAxis("Vertical") < 0)
        {
            camTransform.Translate(Vector3.back * (keyPanSpeed * Mathf.Abs(Input.GetAxis("Vertical")) * Time.deltaTime));
        }
        else if (Input.mousePosition.y <= panBorderSize && (Input.mousePosition.y - panBorderSize) >= -panBorderSize)
        {
            movingZD = true;
            cursorMode = cursorModes.PANDOWN;

            float position = (Input.mousePosition.y - panBorderSize);
            float speed = panFactor * panSpeed * Mathf.Abs(-position / -panBorderSize);

            camTransform.Translate(Vector3.back * (speed * Time.deltaTime));
        }

        //right
        if (Input.GetAxis("Horizontal") > 0)
        {
            camTransform.Translate(Vector3.right * (keyPanSpeed * Mathf.Abs(Input.GetAxis("Horizontal")) * Time.deltaTime));
        }
        else if (Input.mousePosition.x >= Screen.width - panBorderSize && Input.mousePosition.x <= Screen.width)
        {
            movingXR = true;
            cursorMode = cursorModes.PANRIGHT;

            float position = (Input.mousePosition.x - (Screen.width - panBorderSize));
            float speed = panFactor * panSpeed * (position / panBorderSize);

            camTransform.Translate(Vector3.right * (speed * Time.deltaTime));
        }

        //left
        if (Input.GetAxis("Horizontal") < 0)
        {
            camTransform.Translate(Vector3.left * (keyPanSpeed * Mathf.Abs(Input.GetAxis("Horizontal")) * Time.deltaTime));
        }
        else if (Input.mousePosition.x <= panBorderSize && (Input.mousePosition.x - panBorderSize) >= -panBorderSize)
        {
            movingXL = true;
            cursorMode = cursorModes.PANLEFT;

            float position = (Input.mousePosition.x - panBorderSize);
            float speed = panFactor * panSpeed * Mathf.Abs(-position / -panBorderSize);

            camTransform.Translate(Vector3.left * (speed * Time.deltaTime));
        }

        if (movingZU && movingXR)
        {
            cursorMode = cursorModes.PANDIAGUR;
        }
        else if (movingZU && movingXL)
        {
            cursorMode = cursorModes.PANDIAGUL;
        }
        else if (movingZD && movingXR)
        {
            cursorMode = cursorModes.PANDIAGDR;
        }
        else if (movingZD && movingXL)
        {
            cursorMode = cursorModes.PANDIAGDL;
        }
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
            lastMouseY = Input.mousePosition.y;
            lastMouseX = Input.mousePosition.x;

            overUI = IsPointerOverUIElement();
        }

        if (Input.GetMouseButton(1) && !overUI)
        {
            if (lastMouseX != Input.mousePosition.x && lastMouseY != Input.mousePosition.y && cursorMode != cursorModes.DRAG)
            {
                rightClickDrag = true;
                cursorMode = cursorModes.DRAG;
            }

            float xDiff = (Mathf.Abs(lastMouseX - Input.mousePosition.x) * dragFactor);
            float yDiff = (Mathf.Abs(lastMouseY - Input.mousePosition.y) * dragFactor);

            if (lastMouseY < Input.mousePosition.y)
            {
                camTransform.Translate(Vector3.back * dragSpeed * yDiff * Time.deltaTime);
            }
            if (lastMouseY > Input.mousePosition.y)
            {
                camTransform.Translate(Vector3.forward * dragSpeed * yDiff * Time.deltaTime);
            }

            if (lastMouseX < Input.mousePosition.x)
            {
                camTransform.Translate(Vector3.left * dragSpeed * xDiff * Time.deltaTime);
            }
            if (lastMouseX > Input.mousePosition.x)
            {
                camTransform.Translate(Vector3.right * dragSpeed * xDiff * Time.deltaTime);
            }

            lastMouseY = Input.mousePosition.y;
            lastMouseX = Input.mousePosition.x;
        }

        if (Input.GetMouseButtonUp(1))
        {
            rightClickDrag = false;
            overUI = false;
        }
    }

    void SetCursor()
    {
        Texture2D cursorIcon = null;

        if (isRotating)
        {
            cursorMode = cursorModes.ROTATE;
        }
        else if (rightClickDrag)
        {
            cursorMode = cursorModes.DRAG;
        }

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
            case cursorModes.ROTATE:
                cursorIcon = GetComponent<NavCursorIcons>().rotate;
                break;
        }

        Cursor.SetCursor(cursorIcon, Vector2.zero, CursorMode.Auto);
    }

    //Gets all event system raycast results of current mouse or touch position. - Credit to daveMennenoh (https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/)
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    //Returns 'true' if we touched or hovering on Unity UI element. - Credit to daveMennenoh (https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/)
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == uip.UILayer)
                return true;
        }
        return false;
    }

    //Returns 'true' if we touched or hovering on Unity UI element. - Credit to daveMennenoh (https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/)
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }

    bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }
}
