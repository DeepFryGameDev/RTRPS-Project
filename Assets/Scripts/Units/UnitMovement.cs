using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class UnitMovement : MonoBehaviour
{
    [Tooltip("A larger value results in more time allowed for right mouse click to be registered as a move action.")]
    [SerializeField] float rClickFrameCountFactor = 1;
    [Tooltip("Unit's navmesh move speed is determined by their movement rating * this value.")]
    [SerializeField] [Range(1, 25)] float moveSpeedFactor = 10;

    // For resources
    float stopRadius;
    bool resourceClicked;
    Resource chosenResource;

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

        if (resourceClicked)
        {
            StopUnitMovementAtResource();
        }
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
                Vector3 resourcePos = new Vector3();
                resourceClicked = false;

                stopRadius = 0;

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
                    if (IfVillager(selectedUnits[0]))
                    {
                        if (hit.transform.gameObject.CompareTag("Resource"))
                        {
                            resourceClicked = true;
                            resourcePos = hit.transform.position;
                            resourcePos.y = Terrain.activeTerrain.SampleHeight(hit.transform.position);

                            chosenResource = hit.transform.GetComponent<Resource>();
                            stopRadius = chosenResource.interactionBounds;                            
                        }
                    } else
                    {
                        canMove = false;
                    }                    
                }

                // If able to move, gather world position and move
                if (canMove)
                {
                    ProcessMoveUnit(resourcePos);
                }
            }
        }
    }

    private void ProcessMoveUnit(Vector3 resourcePos)
    {
        agent = selectedUnits[0].GetComponent<NavMeshAgent>();

        // Ensure movement speed is set appropriately
        agent.speed = (agent.GetComponent<Unit>().GetMovement() * moveSpeedFactor);

        // Set stopping distance for any resource clicked
        agent.stoppingDistance = stopRadius;

        // Stop any current navigation
        agent.enabled = false;
        agent.enabled = true;

        // Move to where mouse is clicked (or resource if clicked)
        if (!resourceClicked)
        {
            agent.SetDestination(GetWorldPosition());
        }
        else
        {
            agent.SetDestination(resourcePos);
        }
    }

    void StopUnitMovementAtResource()
    {
        bool arrived = agent.remainingDistance <= agent.stoppingDistance;

        if (arrived)
        {
            resourceClicked = false;

            agent.isStopped = true;
            agent.enabled = false;

            agent.GetComponent<VillagerUnit>().BeginGathering(chosenResource);
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

    bool IfVillager(Unit unit)
    {
        VillagerUnit tryVillager = unit as VillagerUnit;

        if (tryVillager != null)
        {
            return true;
        }
        else
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
