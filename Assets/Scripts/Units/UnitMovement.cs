using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] float rClickFrameCountFactor = 1;

    List<Unit> selectedUnits;
    UIProcessing uip;
    float rClickFrameCount;
    NavMeshAgent agent;

    TerrainCollider tcol;
    Ray terrainRay;

    // Start is called before the first frame update
    void Start()
    {
        selectedUnits = GetComponent<SelectedUnitProcessing>().selectedUnits;
        uip = GameObject.Find("UI").GetComponent<UIProcessing>();
        rClickFrameCount = 0;
        tcol = Terrain.activeTerrain.GetComponent<TerrainCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        RightClickFrameCounter();
        CheckForMoveUnit();
    }

    void RightClickFrameCounter()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            rClickFrameCount = 0;
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            rClickFrameCount = rClickFrameCount + (rClickFrameCountFactor * Time.deltaTime);
        }
    }

    void CheckForMoveUnit()
    {
        if (!IsPointerOverUIElement() && selectedUnits.Count > 0)
        {
            if (Input.GetKeyUp(KeyCode.Mouse1) && rClickFrameCount >= 0 && rClickFrameCount <= 1)
            {
                bool canMove = true;

                RaycastHit[] hits;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                hits = Physics.RaycastAll(ray, 1000);

                // Checking if unable to move
                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.gameObject.CompareTag("Building") || hit.transform.gameObject.CompareTag("Unit"))
                    {
                        canMove = false;
                    }

                    if (hit.transform.gameObject.CompareTag("BiomeTile"))
                    {
                        BiomeTile tileHit = hit.transform.GetComponent<BiomeTile>();
                        if (tileHit.biomeType == BiomeTypes.MOUNTAIN ||
                            tileHit.biomeType == BiomeTypes.OCEAN ||
                            tileHit.biomeType == BiomeTypes.RIVER ||
                            tileHit.biomeType == BiomeTypes.LAKE
                            ) 
                        canMove = false;
                    }
                }

                // Checking if any resource is clicked
                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.gameObject.CompareTag("Resource"))
                    {
                        Debug.Log("Hit " + hit.transform.name +", move to RIGHT in front of this and start harvesting");
                        canMove = false;
                    }
                }

                // If able to move, gather world position and move
                if (canMove)
                {           
                    agent = selectedUnits[0].GetComponent<NavMeshAgent>();

                    //agent.ResetPath();
                    //agent.isStopped = true;
                    agent.SetDestination(GetWorldPosition());
                }
            }
        }
    }

    Vector3 GetWorldPosition()
    {
        if (!IsPointerOverUIElement() && selectedUnits.Count > 0)
        {
            if (Input.GetKeyUp(KeyCode.Mouse1) && rClickFrameCount >= 0 && rClickFrameCount <= 1)
            {
                terrainRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitData;

                if (tcol.Raycast(terrainRay, out hitData, 1000))
                {
                    return hitData.point;
                }
            }
        }
        return new Vector3(0, 0, 0);
    }

    void MoveUnit() 
    { 
    
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
