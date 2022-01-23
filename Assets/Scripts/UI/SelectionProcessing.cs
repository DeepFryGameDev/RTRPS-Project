using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// This script handles the process of selecting objects with mouse input
public class SelectionProcessing : MonoBehaviour
{
    [HideInInspector] public List<Unit> selectedUnits = new List<Unit>(); // list of units selected by player.  A single selected unit is represented by the first index of this list

    UIProcessing uip;
    UIPrefabManager uipm;
    UnitProcessing up;
    NavMovement nm;
    BuildManager bm;
    GatherManager gm;

    // For multi-selection
    RectTransform selectionBox; // set to the selection box object in UI (contains an image with border and clear background to simulate a selection box effect)
    Vector2 selectionOriginPos; // on mouse click down, starting position is set for selection box to be processed from the origin
    List<Unit> unitsToSelect = new List<Unit>(); // temporary list of units to be selected/highlighted when selection box is over them
    bool leftClickDrag; // used to determine if left click is being held and the cursor's position is different from it's origin on mouse click down
    bool inClick; // used to determine if left click down has occurred, but not yet released
    float lastMouseX, lastMouseY; // coordinates of last position of mouse X and Y position

    // for double click
    bool isClicked = false; // set to true when left click has occurred outside of double click window (less frames than uip.selectedUnitDoubleClickFrameBuffer)
    bool doubleClicked = false; // set to true when second click has been registered inside of double click window
    int frameCount; // frame count tracker to check if double click has occurred

    private void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        uipm = FindObjectOfType<UIPrefabManager>();
        up = FindObjectOfType<UnitProcessing>();
        nm = FindObjectOfType<NavMovement>();
        bm = FindObjectOfType<BuildManager>();
        gm = FindObjectOfType<GatherManager>();

        selectionBox = uip.transform.Find("MultiSelectCanvas/SelectionBox").GetComponent<RectTransform>(); // initializes the selection box object

        frameCount = 0; // initializes frameCount to 0 for double click to be checked
    }

    private void Update()
    {
        SetSelection();
    }

    void SetSelection() // clears previous selections and sets new one - UI is updated in UIProcessing using the values here 
    {
        if (!IsPointerOverUIElement() && !uip.optionsMenuOpened)
        {
            // clear prior selections on left mouse click or if escape key is pressed
            ClearSelection();

            // set new selection on left mouse click
            SetNewSelection();

            // check for multi-select
            ProcessMultiSelect();
        }
    }

    void ClearSelection() // handles clearing selection on left mouse click or escape key is pressed
    {
        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Escape)))
        {
            switch (uip.uiMode)
            {
                case UIModes.IDLE:

                    break;
                case UIModes.UNIT:

                    if (!uip.actionButtonClicked && !bm.buildActionClicked && !gm.gatherActionClicked && !gm.resourceClickedInAction) // Keeps from clearing the UI entirely if build/gather action is still active
                    {
                        foreach (Unit unit in selectedUnits)
                        {
                            unit.GetComponent<Outline>().OutlineWidth = uip.defaultOutlineWidth; // in case it wasn't properly set back during multi select
                            HighlightSelection(unit.GetComponent<Outline>(), false);
                        }

                        selectedUnits.Clear();

                        uip.resetUI = true;
                        uip.uiMode = UIModes.IDLE;

                        if (nm.focusObj)
                        {
                            nm.DisableCamFocus();
                        }
                    }

                    if (!uip.actionButtonClicked && bm.buildActionClicked) // Allows for BuildManager to clear the building action if cancelled, without clearing the rest of the UI
                    {
                        bm.buildActionClicked = false;
                    }

                    if (!uip.actionButtonClicked && gm.gatherActionClicked)
                    {
                        gm.gatherActionClicked = false;
                    }

                    break;
                case UIModes.RESOURCE:

                    uip.selectedResource = null;

                    Resource[] allResources = FindObjectsOfType<Resource>();
                    foreach (Resource res in allResources)
                    {
                        if (res.GetComponent<Outline>() && res.GetComponent<Outline>().enabled == true)
                        {
                            HighlightSelection(res.GetComponent<Outline>(), false);
                        }
                    }

                    uip.resetUI = true;
                    uip.uiMode = UIModes.IDLE;

                    if (nm.focusObj)
                    {
                        nm.DisableCamFocus();
                    }

                    break;

                case UIModes.BUILDING:

                    uip.HighlightResourceOrBuilding(uip.selectedCompletedBuilding.GetComponent<Outline>(), false);
                    uip.selectedCompletedBuilding = null;

                    uip.resetUI = true;
                    uip.uiMode = UIModes.IDLE;

                    if (nm.focusObj)
                    {
                        nm.DisableCamFocus();
                    }

                    break;

                case UIModes.BUILDINGINPROG:

                    if (uip.selectedBIP)
                    {
                        uip.HighlightResourceOrBuilding(uip.selectedBIP.GetComponent<Outline>(), false);
                        uip.selectedBIP = null;
                    }

                    uip.resetUI = true;
                    uip.uiMode = UIModes.IDLE;

                    if (nm.focusObj)
                    {
                        nm.DisableCamFocus();
                    }

                    break;
            }
        }
    }

    void SetNewSelection() // sets new selection of object based on left mouse click
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !bm.blueprintOpen)
        {
            inClick = true;

            lastMouseY = Input.mousePosition.y; // To help determine if mouse is dragging for multi-select
            lastMouseX = Input.mousePosition.x; // To help determine if mouse is dragging for multi-select

            selectionOriginPos = Input.mousePosition;

            RaycastHit[] hits;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray, 1000);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.CompareTag("Unit") || hit.transform.CompareTag("Resource") || hit.transform.CompareTag("CompletedBuilding") || hit.transform.CompareTag("BuildingInProgressChild"))
                {
                    ProcessDoubleClick(hit);
                }

                if (hit.transform.CompareTag("Unit"))
                {
                    selectedUnits.Add(hit.transform.GetComponent<Unit>());

                    HighlightSelection(hit.transform.GetComponent<Outline>(), true);
                    uip.SetCurrentUnit(hit.transform.GetComponent<Unit>());

                    uip.resetUI = true;
                    uip.uiMode = UIModes.UNIT;
                    break;
                }
                else if (hit.transform.CompareTag("Resource") && !uip.actionButtonClicked)
                {
                    if (gm.resourceClickedInAction)
                    {
                        gm.resourceClickedInAction = false;
                    } else
                    {
                        uip.selectedResource = hit.transform.GetComponent<Resource>();

                        HighlightSelection(hit.transform.GetComponent<Outline>(), true);

                        uip.resetUI = true;
                        uip.uiMode = UIModes.RESOURCE;

                        break;
                    }
                }
                else if (hit.transform.CompareTag("CompletedBuilding"))
                {
                    uip.selectedCompletedBuilding = GetCompletedBuilding(hit.transform.gameObject);
                    HighlightSelection(uip.selectedCompletedBuilding.GetComponent<Outline>(), true);

                    uip.resetUI = true;
                    uip.uiMode = UIModes.BUILDING;

                    break;
                }
                else if (hit.transform.CompareTag("BuildingInProgressChild"))
                {
                    uip.selectedBIP = GetBuildInProgress(hit.transform.gameObject);
                    HighlightSelection(uip.selectedBIP.GetComponent<Outline>(), true);

                    uip.resetUI = true;
                    uip.uiMode = UIModes.BUILDINGINPROG;

                    break;
                }
            }
        }
    }

    // handles processing for double click
    void ProcessDoubleClick(RaycastHit hit)
    {
        if (frameCount <= uip.selectedObjectDoubleClickFrameBuffer && isClicked)
        {
            doubleClicked = true;
            nm.FocusSelection(hit.transform.gameObject);
        }

        if (!isClicked)
        {
            isClicked = true;
            StartCoroutine(CheckForDoubleClick());
        }
    }

    // coroutine to confirm if double click has succeeded
    IEnumerator CheckForDoubleClick()
    {
        doubleClicked = false;

        while (frameCount <= uip.selectedObjectDoubleClickFrameBuffer && !doubleClicked)
        {
            frameCount++;
            yield return new WaitForEndOfFrame();
        }

        frameCount = 0;
        isClicked = false;
    }

    // handles processing for multi-select
    void ProcessMultiSelect()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            inClick = false;

            if (leftClickDrag)
            {
                ReleaseSelectionBox();
                leftClickDrag = false;
            }
        }

        if (Input.GetKey(KeyCode.Mouse0) && inClick)
        {
            if (lastMouseX != Input.mousePosition.x && lastMouseY != Input.mousePosition.y)
            {
                leftClickDrag = true;
            }

            if (leftClickDrag)
                UpdateSelectionBox(Input.mousePosition);
        }
    } 

    // handles the various tasks of highlighting and selecting units in the selection box
    void UpdateSelectionBox (Vector2 curMousePos)
    {
        if (!selectionBox.gameObject.activeInHierarchy)
            uipm.ShowUIObject(selectionBox.gameObject, true);

        float width = curMousePos.x - selectionOriginPos.x;
        float height = curMousePos.y - selectionOriginPos.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        selectionBox.anchoredPosition = selectionOriginPos + new Vector2(width / 2, height / 2);

        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
            {              
                if (!unitsToSelect.Contains(unit) && unitsToSelect.Count < uip.selectedUnitsMax)
                {
                    HighlightSelection(unit.GetComponent<Outline>(), true);
                    unitsToSelect.Add(unit);
                }                
            } else
            {
                HighlightSelection(unit.GetComponent<Outline>(), false);
                if (unitsToSelect.Contains(unit))
                {
                    unitsToSelect.Remove(unit);
                }
            }
        }
    }

    // handles the process of releasing the left mouse click when selection box is opened
    void ReleaseSelectionBox()
    {
        uipm.ShowUIObject(selectionBox.gameObject, false);

        foreach (Unit unit in unitsToSelect)
        {
            selectedUnits.Add(unit);
        }

        if (selectedUnits.Count > 0)
        {
            uip.SetCurrentUnit(selectedUnits[0]);
            uip.selectedUnit.GetComponent<Outline>().OutlineWidth = uip.hoveredUnitsOutlineWidth;
            uip.resetUI = true;
            uip.uiMode = UIModes.UNIT;
        }    
    }

    // handles the highlighting of an object.  This is done by enabling/disabling the 'outline' script attached to them
    void HighlightSelection(Outline ol, bool highlight)
    {
        ol.enabled = highlight;
    }

    // returns the CompletedBuilding script attached to the parent of an object that is hit with raycast
    CompletedBuilding GetCompletedBuilding(GameObject obj)
    {
        Transform temp = obj.transform;

        while (true)
        {
            if (temp.GetComponent<CompletedBuilding>())
            {
                return temp.GetComponent<CompletedBuilding>();
            } else
            {
                temp = temp.parent;

                if (temp.gameObject.name == "BuildManager")
                {
                    return null;
                }
            }
        }
    }

    // returns the BuildInProgress script attached to the parent of an object that is hit with raycast
    BuildInProgress GetBuildInProgress(GameObject obj)
    {
        Transform temp = obj.transform;

        while (true)
        {
            if (temp.GetComponent<BuildInProgress>())
            {
                return temp.GetComponent<BuildInProgress>();
            }
            else
            {
                temp = temp.parent;

                if (temp.gameObject.name == "BuildManager")
                {
                    return null;
                }
            }
        }
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
}
