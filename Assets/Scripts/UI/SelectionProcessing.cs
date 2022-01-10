using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionProcessing : MonoBehaviour
{
    [HideInInspector] public List<Unit> selectedUnits = new List<Unit>();
    GameObject selectedGO;

    // For multi-selection
    RectTransform selectionBox;
    Vector2 startPos;
    List<Unit> unitsToSelect = new List<Unit>();
    

    UIProcessing uip;
    UnitProcessing up;
    NavMovement nm;
    GatherManager gm;

    // for multi select
    bool leftClickDrag, inClick;
    float lastMouseX, lastMouseY;

    // for double click
    bool isClicked = false, doubleClicked = false;
    int frameCount;

    private void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        up = FindObjectOfType<UnitProcessing>();
        nm = FindObjectOfType<NavMovement>();
        gm = FindObjectOfType<GatherManager>();

        selectionBox = uip.transform.Find("MultiSelectCanvas/SelectionBox").GetComponent<RectTransform>();
        uip.ShowUnitPanels(false);
        uip.ShowMultipleUnitsPanel(false);

        frameCount = 0;
    }

    private void Update()
    {
        SetSelection();
        CheckForFocusSelection();
    }

    private void CheckForFocusSelection()
    {
        if (selectedUnits.Count == 1 && Input.GetKeyDown(KeyCode.F))
        {
            FocusCameraOnSelection(selectedGO);
        }
    }

    public void FocusCameraOnSelection(GameObject obj)
    {
        nm.FocusSelection(obj);
    }

    IEnumerator CheckForDoubleClick()
    {
        doubleClicked = false;

        while (frameCount < uip.selectedUnitDoubleClickFrameBuffer && !doubleClicked)
        {
            frameCount++;
            yield return new WaitForEndOfFrame();
        }

        frameCount = 0;
        isClicked = false;
    }

    void SetSelection()
    {
        if (!IsPointerOverUIElement())
        {
            // clear prior selections
            if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Escape))) 
            {
                switch (uip.uiMode)
                {
                    case UIModes.IDLE:

                        break;
                    case UIModes.UNIT:
                        if (!uip.actionButtonClicked)
                        {
                            foreach (Unit u in selectedUnits)
                            {
                                u.isSelected = false;
                            }
                            foreach (Unit unit in selectedUnits)
                            {
                                unit.GetComponent<Outline>().OutlineWidth = up.highlightWidth; // in case it wasn't properly set back during multi select
                                HighlightSelection(unit.GetComponent<Outline>(), false);
                            }

                            selectedUnits.Clear();

                            uip.resetUI = true;
                            uip.uiMode = UIModes.IDLE;

                            nm.DisableCamFocus();
                        }                        

                        break;
                    case UIModes.RESOURCE:

                        uip.currentResource = null;

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

                        nm.DisableCamFocus();

                        break;

                    case UIModes.BUILDING:

                        uip.HighlightResource(uip.currentBuilding.GetComponent<Outline>(), false);
                        uip.currentBuilding = null;

                        uip.resetUI = true;
                        uip.uiMode = UIModes.IDLE;

                        nm.DisableCamFocus();
                        break;

                    case UIModes.BUILDINGINPROG:

                        if (uip.currentBip)
                        {
                            uip.HighlightResource(uip.currentBip.GetComponent<Outline>(), false);
                            uip.currentBip = null;
                        }                        

                        uip.resetUI = true;
                        uip.uiMode = UIModes.IDLE;

                        nm.DisableCamFocus();
                        break;
                }
            }  
            
            // set new selection
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                RaycastHit[] hits;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hits = Physics.RaycastAll(ray, 1000);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.CompareTag("Unit"))
                    {
                        hit.transform.GetComponent<Unit>().isSelected = true;

                        selectedUnits.Add(hit.transform.GetComponent<Unit>());

                        HighlightSelection(hit.transform.GetComponent<Outline>(), true);
                        uip.SetCurrentUnit(hit.transform.GetComponent<Unit>());

                        uip.resetUI = true;
                        uip.uiMode = UIModes.UNIT;

                        if (frameCount < uip.selectedUnitDoubleClickFrameBuffer && isClicked)
                        {
                            doubleClicked = true;
                            FocusCameraOnSelection(hit.transform.gameObject);
                        }

                        if (!isClicked)
                        {
                            isClicked = true;
                            StartCoroutine(CheckForDoubleClick());
                        }

                    } else if (hit.transform.CompareTag("Resource"))
                    {
                        uip.currentResource = hit.transform.GetComponent<Resource>();

                        HighlightSelection(hit.transform.GetComponent<Outline>(), true);

                        uip.resetUI = true;
                        uip.uiMode = UIModes.RESOURCE;

                        if (frameCount < uip.selectedUnitDoubleClickFrameBuffer && isClicked)
                        {
                            doubleClicked = true;
                            FocusCameraOnSelection(hit.transform.gameObject);
                        }

                        if (!isClicked)
                        {
                            isClicked = true;
                            StartCoroutine(CheckForDoubleClick());
                        }
                    } else if (hit.transform.CompareTag("CompletedBuilding"))
                    {
                        uip.currentBuilding = GetCompletedBuilding(hit.transform.gameObject);
                        HighlightSelection(uip.currentBuilding.GetComponent<Outline>(), true);

                        uip.resetUI = true;
                        uip.uiMode = UIModes.BUILDING;

                        if (frameCount < uip.selectedUnitDoubleClickFrameBuffer && isClicked)
                        {
                            doubleClicked = true;
                            FocusCameraOnSelection(hit.transform.gameObject);
                        }

                        if (!isClicked)
                        {
                            isClicked = true;
                            StartCoroutine(CheckForDoubleClick());
                        }
                    } else if (hit.transform.CompareTag("BuildingInProgressChild"))
                    {
                        uip.currentBip = GetBuildInProgress(hit.transform.gameObject);
                        HighlightSelection(uip.currentBip.GetComponent<Outline>(), true);

                        uip.resetUI = true;
                        uip.uiMode = UIModes.BUILDINGINPROG;

                        if (frameCount < uip.selectedUnitDoubleClickFrameBuffer && isClicked)
                        {
                            doubleClicked = true;
                            FocusCameraOnSelection(hit.transform.gameObject);
                        }

                        if (!isClicked)
                        {
                            isClicked = true;
                            StartCoroutine(CheckForDoubleClick());
                        }
                    }
                }
            }

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
    }

    void UpdateSelectionBox (Vector2 curMousePos)
    {
        if (!selectionBox.gameObject.activeInHierarchy)        
            selectionBox.gameObject.SetActive(true);

        float width = curMousePos.x - startPos.x;
        float height = curMousePos.y - startPos.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        selectionBox.anchoredPosition = startPos + new Vector2(width / 2, height / 2);

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

    void ReleaseSelectionBox()
    {
        selectionBox.gameObject.SetActive(false);

        foreach (Unit unit in unitsToSelect)
        {
            unit.isSelected = true;
            selectedUnits.Add(unit);
        }

        if (selectedUnits.Count > 0)
        {
            uip.SetCurrentUnit(selectedUnits[0]);
            uip.resetUI = true;
        }    
    }

    void HighlightSelection(Outline ol, bool highlight)
    {
        ol.enabled = highlight;
    }

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
