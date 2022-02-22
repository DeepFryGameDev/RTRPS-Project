using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// this script handles the camera's movement around the world
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
    [Tooltip("Multiplied by the scrollFactor to determine how quickly to zoom using scroll wheel on mouse.")]
    [SerializeField] [Range(1f, 10f)] float scrollSpeed;
    [Tooltip("Multiplied by the scrollSpeed to determine how quickly to zoom using scroll wheel on mouse.")]
    [SerializeField] [Range(1f, 5f)] float scrollFactor;
    [Tooltip("A larger value represents a larger jump to world position from where the right mouse button was originally clicked.")]
    [SerializeField] [Range(0.1f, 5.0f)] float dragFactor;
    [Tooltip("A larger value represents a larger jump to world position along x axis from where the right mouse button was originally clicked.")]
    [SerializeField] [Range(0.1f, 5.0f)] float xAxisDragFactor;
    [Tooltip("A larger value represents a larger jump to world position along y axis from where the right mouse button was originally clicked.")]
    [SerializeField] [Range(0.1f, 5.0f)] float yAxisDragFactor;
    [Tooltip("How fast to move the camera when dragging after holding right mouse button.")]
    [SerializeField] [Range(1f, 50f)] float dragSpeed;
    [Tooltip("Closest y position the camera is able to zoom into.")]
    [SerializeField] [Range(1f, 10f)] float minScroll;
    [Tooltip("Farthest y position the camera is able to zoom out to.")]
    [SerializeField] [Range(10f, 50f)] float maxScroll;
    [Tooltip("How quickly the camera rotates when holding down the scroll wheel.")]
    [SerializeField] [Range(1f, 20f)] float rotateSpeed;
    [Tooltip("How far the camera can zoom in.")]
    [SerializeField] [Range(0, 50f)] float minFOV;
    [Tooltip("How far the camera can zoom out.")]
    [SerializeField] [Range(50f, 100f)] float maxFOV;

    // for focusing on selection - focus will center the camera to that object and follow it
    public CinemachineFreeLook focusCam; // Camera to be used for focusing on object
    GameObject objToFocus; // the object to focus
    [HideInInspector] public bool focusObj; // if object should be focused

    CursorManager cm; // used to update cursor mode depending on action being taken
    BuildManager bm; // for checking if build action has been clicked.  This allows for WASD keys to be used as building shortcut keys and will not move the camera when build action is clicked

    Terrain terrainMap; // set as the active terrain
    Transform camTransform; // set as the main camera's transform

    // for drag movement
    UIProcessing uip; // used to gather the UI layer
    bool rightClickDrag; // used to check if the mouse cursor has moved while being right clicked
    bool overUI; // returns true if the mouse cursor is over any UI objects
    float lastMouseX, lastMouseY; // sets mouse position upon right click down

    // for rotation
    bool inRotate;

    // To keep camera above terrain
    float terrainDist;

    void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        cm = FindObjectOfType<CursorManager>();
        bm = FindObjectOfType<BuildManager>();

        camTransform = Camera.main.transform;
        terrainDist = camTransform.position.y;

        terrainMap = Terrain.activeTerrain;

        Cursor.lockState = CursorLockMode.Confined; // Keeps mouse cursor from leaving the game window

        cm.cursorMode = cursorModes.IDLE; // Starts the game in IDLE mode
        cm.SetCursor(); // Sets the cursor to default due to IDLE mode
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMouseOverGameWindow && !uip.optionsMenuOpened) // ensures mouse is over the game window when trying to perform actions
        {
            ZoomCamera(); // Handles zoom functionality with scroll wheel

            if (!rightClickDrag) // if the right mouse button is not being held down
            {
                PanCamera(); // Handles moving the camera if cursor touches the border of the game window or WASD is pressed. 
            }

            DragCamera(); // Handles moving camera based on the direct mouse position if right mouse button is being held

            RotateCamera(); // Handles rotating camera around point where mouse scroll wheel is clicked
        }

        KeepCameraAtTerrainHeight(); // Keeps camera at consistant distance above terrain

        if (!focusObj)
        {
            KeepCameraInBounds(); // Keeps camera from being able to leave the terrain view
        }        
    }

    #region Camera Zoom

    void ZoomCamera() // Moves camera up and down Y axis based on scroll value (scrollDist)
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (!focusObj) // if not focusing any object
            {
                float newFOV = camTransform.GetComponent<Camera>().fieldOfView;
                float adjFOV = scroll * (scrollSpeed * scrollFactor);

                newFOV -= adjFOV;

                camTransform.GetComponent<Camera>().fieldOfView = Mathf.Clamp(newFOV, minFOV, maxFOV);
            }
            else // if focusing an object
            {
                float newFOV = focusCam.m_Lens.FieldOfView;
                float adjFOV = scroll * (scrollSpeed * scrollFactor);

                newFOV -= adjFOV;

                focusCam.m_Lens.FieldOfView = Mathf.Clamp(newFOV, minFOV, maxFOV);
            }   
        }
    }

    #endregion

    #region Camera Pan

    void PanCamera() // If mouse moves to border or WASD is pressed, the camera is moved.  Border thickness and move speed is adjusted with public vars in editor
    {
        Vector3 pos = camTransform.position;
        bool movingZU = false, movingZD = false, movingXL = false, movingXR = false;

        if (cm.cursorMode != cursorModes.IDLE)
        {
            cm.cursorMode = cursorModes.IDLE;
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
            
            if (focusObj)
            {
                DisableCamFocus();
            }            
        }
        else if (Input.mousePosition.y >= Screen.height - panBorderSize && Input.mousePosition.y <= Screen.height && !inRotate)
        {
            movingZU = true;
            cm.cursorMode = cursorModes.PANUP;

            float position = (Input.mousePosition.y - (Screen.height - panBorderSize));
            float speed = panFactor * panSpeed * (position / panBorderSize);

            camTransform.Translate(Vector3.forward * (speed * Time.deltaTime));

            if (focusObj)
            {
                DisableCamFocus();
            }
        }

        //down
        if (Input.GetAxis("Vertical") < 0)
        {
            camTransform.Translate(Vector3.back * (keyPanSpeed * Mathf.Abs(Input.GetAxis("Vertical")) * Time.deltaTime));

            if (focusObj)
            {
                DisableCamFocus();
            }
        }
        else if (Input.mousePosition.y <= panBorderSize && (Input.mousePosition.y - panBorderSize) >= -panBorderSize && !inRotate)
        {
            movingZD = true;
            cm.cursorMode = cursorModes.PANDOWN;

            float position = (Input.mousePosition.y - panBorderSize);
            float speed = panFactor * panSpeed * Mathf.Abs(-position / -panBorderSize);

            camTransform.Translate(Vector3.back * (speed * Time.deltaTime));

            if (focusObj)
            {
                DisableCamFocus();
            }
        }

        //right
        if (Input.GetAxis("Horizontal") > 0)
        {
            camTransform.Translate(Vector3.right * (keyPanSpeed * Mathf.Abs(Input.GetAxis("Horizontal")) * Time.deltaTime));

            if (focusObj)
            {
                DisableCamFocus();
            }
        }
        else if (Input.mousePosition.x >= Screen.width - panBorderSize && Input.mousePosition.x <= Screen.width && !inRotate)
        {
            movingXR = true;
            cm.cursorMode = cursorModes.PANRIGHT;

            float position = (Input.mousePosition.x - (Screen.width - panBorderSize));
            float speed = panFactor * panSpeed * (position / panBorderSize);

            camTransform.Translate(Vector3.right * (speed * Time.deltaTime));

            if (focusObj)
            {
                DisableCamFocus();
            }
        }

        //left
        if (Input.GetAxis("Horizontal") < 0)
        {
            camTransform.Translate(Vector3.left * (keyPanSpeed * Mathf.Abs(Input.GetAxis("Horizontal")) * Time.deltaTime));

            if (focusObj)
            {
                DisableCamFocus();
            }
        }
        else if (Input.mousePosition.x <= panBorderSize && (Input.mousePosition.x - panBorderSize) >= -panBorderSize && !inRotate)
        {
            movingXL = true;
            cm.cursorMode = cursorModes.PANLEFT;

            float position = (Input.mousePosition.x - panBorderSize);
            float speed = panFactor * panSpeed * Mathf.Abs(-position / -panBorderSize);

            camTransform.Translate(Vector3.left * (speed * Time.deltaTime));

            if (focusObj)
            {
                DisableCamFocus();
            }
        }

        if (movingZU && movingXR)
        {
            cm.cursorMode = cursorModes.PANDIAGUR;
        }
        else if (movingZU && movingXL)
        {
            cm.cursorMode = cursorModes.PANDIAGUL;
        }
        else if (movingZD && movingXR)
        {
            cm.cursorMode = cursorModes.PANDIAGDR;
        }
        else if (movingZD && movingXL)
        {
            cm.cursorMode = cursorModes.PANDIAGDL;
        }
    }
    #endregion

    #region Camera Drag

    // Drags camera when right mouse button is clicked - this will move the camera in accordance with the position of the mouse cursor
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
            if (lastMouseX != Input.mousePosition.x && lastMouseY != Input.mousePosition.y && cm.cursorMode != cursorModes.DRAG)
            {
                rightClickDrag = true;
                cm.cursorDrag = true;
                cm.cursorMode = cursorModes.DRAG;

                if (focusObj)
                {
                    DisableCamFocus();
                }
            }

            float xDiff = (Mathf.Abs(lastMouseX - Input.mousePosition.x) * dragFactor);
            float yDiff = (Mathf.Abs(lastMouseY - Input.mousePosition.y) * dragFactor);

            if (lastMouseY < Input.mousePosition.y)
            {
                camTransform.Translate(Vector3.back * (dragSpeed * yAxisDragFactor) * yDiff * Time.deltaTime);
            }
            if (lastMouseY > Input.mousePosition.y)
            {
                camTransform.Translate(Vector3.forward * (dragSpeed * yAxisDragFactor) * yDiff * Time.deltaTime);
            }

            if (lastMouseX < Input.mousePosition.x)
            {
                camTransform.Translate(Vector3.left * (dragSpeed * xAxisDragFactor) * xDiff * Time.deltaTime);
            }
            if (lastMouseX > Input.mousePosition.x)
            {
                camTransform.Translate(Vector3.right * (dragSpeed * xAxisDragFactor) * xDiff * Time.deltaTime);
            }

            lastMouseY = Input.mousePosition.y;
            lastMouseX = Input.mousePosition.x;
        }

        if (Input.GetMouseButtonUp(1))
        {
            rightClickDrag = false;
            cm.cursorDrag = false;
            overUI = false;
        }
    }

    #endregion

    #region Camera Rotation

    void RotateCamera() // If mouse scroll wheel is clicked, rotates camera around based on mouse position
    {
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            inRotate = true;
            cm.cursorRotate = true;
        }

        if (Input.GetKey(KeyCode.Mouse2))
        {
            if (!focusObj)
            {
                Vector3 rotatePoint = new Vector3(camTransform.position.x, Terrain.activeTerrain.SampleHeight(camTransform.position), camTransform.position.z);

                camTransform.RotateAround(rotatePoint, transform.up, -Input.GetAxis("Mouse X") * rotateSpeed);
            } else
            {
                focusCam.m_XAxis.Value -= (Input.GetAxis("Mouse X") * rotateSpeed);
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse2))
        {
            inRotate = false;
            cm.cursorRotate = false;
        }
    }

    #endregion

    #region Camera Consistency

    void KeepCameraInBounds() // Keeps camera from being able to leave the boundary of the terrain
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

    void KeepCameraAtTerrainHeight() // Keeps camera at consistent distance above terrain
    {
        float setHeight = Terrain.activeTerrain.SampleHeight(camTransform.position);

        if (GetComponent<NavInterface>().terrainHeight > setHeight)
        {
            setHeight = GetComponent<NavInterface>().terrainHeight;
        }

        float newHeight = (setHeight + terrainDist) - GetComponent<NavInterface>().terrainHeight;

        camTransform.position = new Vector3(camTransform.position.x, newHeight, camTransform.position.z);
    }

    #endregion

    #region Camera Focus

    public void DisableCamFocus() // Turns camera focus off
    {
        if (focusObj)
        {
            focusObj = false;

            focusCam.Follow = null;
            focusCam.LookAt = null;

            focusCam.gameObject.SetActive(false);
        }
    }

    public void FocusSelection(GameObject obj) // Turns camera focus on
    {
        objToFocus = obj;

        focusCam.m_Lens.FieldOfView = camTransform.GetComponent<Camera>().fieldOfView;

        focusCam.Follow = objToFocus.transform;
        focusCam.LookAt = objToFocus.transform;
        focusCam.gameObject.SetActive(true);

        focusObj = true;
    }

    #endregion

    // Gets all event system raycast results of current mouse or touch position. - Credit to daveMennenoh (https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/)
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    // Returns 'true' if we touched or hovering on Unity UI element. - Credit to daveMennenoh (https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/)
    bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == uip.UILayer)
                return true;
        }
        return false;
    }

    // Returns 'true' if we touched or hovering on Unity UI element. - Credit to daveMennenoh (https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/)
    bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }

    // Returns 'true' if the mouse cursor is over the game window
    bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }
}
