using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class UnitMovement : MonoBehaviour
{
    [Tooltip("A larger value results in more time allowed for right mouse click to be registered as a move action.")]
    [SerializeField] float rClickFrameCountFactor = 1;
    [Tooltip("Unit's navmesh base move speed to be calculated before factor.")]
    [SerializeField] [Range(1, 5)] float moveSpeedBaseline = 1;
    [Tooltip("Unit's navmesh move speed is determined by the baseline + their movement rating * this value.")]
    [SerializeField] [Range(0.1f, 25)] float moveSpeedFactor = 10;

    // For resources
    float stopRadius;
    bool resourceClicked;
    Resource chosenResource;

    // For moving
    //float collisionCheckDistance = 3;
    bool isMoving;
    Vector3 agentDestination;

    List<Unit> selectedUnits;
    UIProcessing uip;
    float rClickFrameCount;
    NavMeshAgent agent;
    TerrainCollider tcol;
    Ray terrainRay;
    CursorManager cm;

    // Start is called before the first frame update
    void Start()
    {
        selectedUnits = GetComponent<SelectedUnitProcessing>().selectedUnits;
        uip = GameObject.Find("UI").GetComponent<UIProcessing>();
        rClickFrameCount = 0;
        tcol = Terrain.activeTerrain.GetComponent<TerrainCollider>();
        cm = GameObject.Find("UI/MoveTargetAnim").GetComponent<CursorManager>();
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

        CheckForUnitCollision();
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

        SwitchNavMeshAgent(agent, false);

        // Ensure movement speed is set appropriately
        agent.speed = moveSpeedBaseline + (agent.GetComponent<Unit>().GetMovement() * moveSpeedFactor);

        // Set stopping distance for any resource clicked
        agent.stoppingDistance = stopRadius;

        // Stop any current navigation
        agent.enabled = false;
        agent.enabled = true;

        // To check for collision (currently not being used)
        isMoving = true;

        // Set villager active task to false as they are being moved manually
        if (IfVillager(agent.GetComponent<Unit>()))
        {
            if (agent.GetComponent<VillagerUnit>().gatherTaskIsActive)
            {
                agent.GetComponent<VillagerUnit>().gatherTaskIsActive = false;
                agent.GetComponent<VillagerUnit>().StopGathering();
            }
        }

        // Move to where mouse is clicked (or resource if clicked)
        if (!resourceClicked)
        {
            agentDestination = GetWorldPosition();
        }
        else
        {
            agentDestination = resourcePos;
        }

        // Show UX feedback cursor animation
        ShowCursorAnim(resourceClicked);

        agent.SetDestination(agentDestination);
    }

    void StopUnitMovementAtResource()
    {
        bool arrived = agent.remainingDistance <= agent.stoppingDistance;

        if (arrived)
        {
            resourceClicked = false;
            isMoving = false;

            agent.isStopped = true;
            agent.enabled = false;

            SwitchNavMeshAgent(agent, true);

            agent.transform.LookAt(chosenResource.transform);

            agent.GetComponent<VillagerUnit>().BeginGathering(chosenResource);
        }
    }

    public void ProcessMoveVillagerUnitInTask(bool toDepot, Unit unit, Resource resource, Depot depot)
    {
        agent = unit.GetComponent<NavMeshAgent>();

        SwitchNavMeshAgent(agent, false);

        // Ensure movement speed is set appropriately
        agent.speed = (unit.GetMovement() * moveSpeedFactor);

        // Set stopping distance for any resource clicked
        if (toDepot)
        {
            agent.stoppingDistance = depot.interactionBounds;
        } else
        {
            agent.stoppingDistance = resource.interactionBounds;
        }        

        // To check for collision (currently not being used)
        isMoving = true;

        // Move to where mouse is clicked (or resource if clicked)
        if (toDepot)
        {
            agentDestination = depot.transform.position;
        }
        else
        {
            agentDestination = resource.transform.position;
        }

        agent.SetDestination(agentDestination);
    }

    void SwitchNavMeshAgent(NavMeshAgent agent, bool shouldBeObstacle)
    {
        if (shouldBeObstacle)
        {
            agent.GetComponent<NavMeshAgent>().enabled = false;
            agent.GetComponent<NavMeshObstacle>().enabled = true;
        } else
        {
            agent.GetComponent<NavMeshObstacle>().enabled = false;
            agent.GetComponent<NavMeshAgent>().enabled = true;            
        }
    }

    void CheckForUnitCollision()
    {
        if (isMoving)
        {
            /*RaycastHit[] hits;
            Ray ray = new Ray(agent.transform.position, agentDestination);
            hits = Physics.RaycastAll(ray, 1);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.gameObject.CompareTag("Unit"))
                {
                    Debug.Log("Reset path");
                    Debug.Log(hit.transform.gameObject.name);

                    // delete below after testing
                    resourceClicked = false;
                    isMoving = false;

                    agent.isStopped = true;
                    agent.enabled = false;
                }
            }*/
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

    Vector3 GetScreenPosition()
    {
        Vector3 screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 20);
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(screenPoint);

        return screenPos;
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

    void ShowCursorAnim(bool isResourceTarget)
    {
        if (isResourceTarget) //Show task target
        {
            StartCoroutine(TaskTargetAnim());
        } else // Show move target
        {
            MoveTargetAnim();
        }
    }

    void MoveTargetAnim()
    {
        cm.GenerateMarker(agentDestination);
    }

    IEnumerator TaskTargetAnim()
    {
        bool targetHidden = false;

        //instantiate prefab at mouse click location in world
        GameObject targetAnim = Instantiate(GameObject.FindObjectOfType<NavCursorIcons>().taskTargetPrefab, GetScreenPosition(), Quaternion.identity, uip.transform);

        float fadeFactor = GameObject.FindObjectOfType<NavCursorIcons>().taskAnimFadeFactor;
        float animScale = GameObject.FindObjectOfType<NavCursorIcons>().taskAnimBaseScale;

        targetAnim.transform.localScale = new Vector3(animScale, animScale, animScale);
        targetAnim.transform.eulerAngles = new Vector3(0, 90, 270); //faces prefab towards camera

        while (!targetHidden)
        {
            float lowerScale = fadeFactor * Time.deltaTime;

            targetAnim.transform.localScale = new Vector3(targetAnim.transform.localScale.x - lowerScale, targetAnim.transform.localScale.y - lowerScale, targetAnim.transform.localScale.z - lowerScale);
            
            if (targetAnim.transform.localScale.x <= 0)
            {
                Destroy(targetAnim);
                targetHidden = true;
            }
            yield return new WaitForEndOfFrame();
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
