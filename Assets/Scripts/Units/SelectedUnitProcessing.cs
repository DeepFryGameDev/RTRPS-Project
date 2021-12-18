using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectedUnitProcessing : MonoBehaviour
{
    [ReadOnly] public List<Unit> selectedUnits;

    // For multi-selection
    RectTransform selectionBox;
    Vector2 startPos;
    List<Unit> unitsToSelect = new List<Unit>();

    // For clearing selected units
    bool unitsCleared = false;

    UIProcessing uip;
    UnitProcessing up;
    NavMovement nm;

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

        selectionBox = uip.transform.Find("MultiSelectCanvas/SelectionBox").GetComponent<RectTransform>();
        uip.ShowPanels(false);
        uip.ShowMultipleUnitsPanel(false);

        frameCount = 0;
    }

    private void Update()
    {
        SetSelectedUnits();
        FocusUnit();
    }

    private void FocusUnit()
    {
        if (selectedUnits.Count == 1 && Input.GetKeyDown(KeyCode.F))
        {
            FocusCameraOnUnit(selectedUnits[0]);
        }
    }

    public void FocusCameraOnUnit(Unit unit)
    {
        nm.FocusUnit(unit);
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

    void SetSelectedUnits()
    {
        if (!IsPointerOverUIElement())
        {
            if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Escape)) && !unitsCleared)
            {
                foreach (Unit u in selectedUnits)
                {
                    u.isSelected = false;
                }
                foreach (Unit unit in selectedUnits)
                {
                    unit.GetComponent<Outline>().OutlineWidth = up.highlightWidth; // in case it wasn't properly set back during multi select
                    HighlightUnit(unit, false);
                }
                
                selectedUnits.Clear();
                unitsCleared = true;

                nm.DisableCamFocus();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                inClick = true;

                lastMouseY = Input.mousePosition.y; // To help determine if mouse is dragging for multi-select
                lastMouseX = Input.mousePosition.x; // To help determine if mouse is dragging for multi-select

                RaycastHit[] hits;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hits = Physics.RaycastAll(ray, 1000);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.gameObject.CompareTag("Unit"))
                    {
                        hit.transform.GetComponent<Unit>().isSelected = true;
                        /*if (IfVillager(hit.transform.GetComponent<Unit>()))
                        {
                            AddVillagerToSelectedUnits(hit.transform.GetComponent<VillagerUnit>());
                        }*/

                        selectedUnits.Add(hit.transform.GetComponent<Unit>());

                        HighlightUnit(hit.transform.GetComponent<Unit>(), true);
                        unitsCleared = false;
                        uip.SetCurrentUnit(hit.transform.GetComponent<Unit>());
                        uip.resetUI = true;

                        if (frameCount < uip.selectedUnitDoubleClickFrameBuffer && isClicked)
                        {
                            doubleClicked = true;
                            FocusCameraOnUnit(hit.transform.GetComponent<Unit>());
                        }

                        if (!isClicked)
                        {
                            isClicked = true;
                            StartCoroutine(CheckForDoubleClick());
                        }
                    }
                }

                startPos = Input.mousePosition;
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
                    HighlightUnit(unit, true);
                    unitsToSelect.Add(unit);
                }                
            } else
            {
                HighlightUnit(unit, false);
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
            unitsCleared = false;

            uip.SetCurrentUnit(selectedUnits[0]);
            uip.resetUI = true;
        }    
    }

    void HighlightUnit(Unit unit, bool highlight)
    {
        unit.GetComponent<Outline>().enabled = highlight;
    }

    void AddVillagerToSelectedUnits(VillagerUnit vu)
    {
        selectedUnits.Add(vu);
    }

    bool IfVillager(Unit unit)
    {
        VillagerUnit tryVillager = unit as VillagerUnit;

        if (tryVillager != null)
        {
            return true;
        } else
        {
            return false;
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
