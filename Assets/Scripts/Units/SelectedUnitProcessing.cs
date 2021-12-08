using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectedUnitProcessing : MonoBehaviour
{
    public List<Unit> selectedUnits;

    UIProcessing uip;

    private void Start()
    {
        uip = GameObject.Find("UI").GetComponent<UIProcessing>();
    }

    private void Update()
    {
        SetSelectedUnits();
    }

    void SetSelectedUnits()
    {
        if (!IsPointerOverUIElement())
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Escape))
            {
                foreach (Unit u in selectedUnits)
                {
                    u.isSelected = false;
                }
                selectedUnits.Clear();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                RaycastHit[] hits;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hits = Physics.RaycastAll(ray, 1000);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.gameObject.CompareTag("Unit"))
                    {
                        hit.transform.GetComponent<Unit>().isSelected = true;
                        if (IfVillager(hit.transform.GetComponent<Unit>()))
                        {
                            AddVillagerToSelectedUnits(hit.transform.GetComponent<VillagerUnit>());
                        }
                    }
                }
            }
        }        
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
