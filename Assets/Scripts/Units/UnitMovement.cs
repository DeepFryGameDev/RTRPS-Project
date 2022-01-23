using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Used to handle movement in the game world
public class UnitMovement : MonoBehaviour
{
    [Tooltip("A larger value results in more time allowed for right mouse click to be registered as a move action.")]
    [SerializeField] float rClickFrameCountFactor = 1;
    [Tooltip("The number of frames between 0 and this number for a right mouse click to be registered as a move action.")]
    [SerializeField] float rClickMaxFrames = 1;

    [Tooltip("Unit's navmesh base move speed to be calculated before factor.")]
    [Range(1, 5)] public float moveSpeedBaseline = 1;
    [Tooltip("Unit's navmesh move speed is determined by the baseline + their agility * agility factor + their movement rating * this value.")]
    [Range(0.1f, 25)] public float moveSpeedFactor = 2.5f;
    [Tooltip("Unit's navmesh move speed is determined by the baseline + their movement rating * move speed factor + their movement rating * this value.")]
    [Range(0.1f, 25)] public float moveSpeedAgilityFactor = 2;

    // For resources
    Resource chosenResource; // set to the resource that the player has clicked
    GatherManager gm;

    // For building
    BuildInProgress buildInProg; // set to the build in progress that the player has clicked

    // For moving
    Vector3 agentDestination; // set to the target destination depending on which type of object has been clicked
    UIProcessing uip; // used to obtain the selected unit(s) as well as determine if the unit is of type Villager
    NavMovement nm;

    // For right click frame measuring
    float rClickFrameCount; // used to determine if a single right mouse click has been registered, instead of holding the click down.

    void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        gm = FindObjectOfType<GatherManager>();
        nm = FindObjectOfType<NavMovement>();
    }

    void Update()
    {
        RightClickFrameCounter(); // used to measure if a right click has been registered as a single click or a 'hold'
        CheckForMoveUnit(); // Checks if a unit should be moved, and where they should move to
    }

    void CheckForMoveUnit() // Determines if user is able to move to requested object by user, and processes move/gather/build where necessary
    {
        if (!IsPointerOverUIElement() && uip.selectedUnits.Count > 0)
        {
            if (Input.GetKeyUp(KeyCode.Mouse1) && rClickFrameCount >= 0 && rClickFrameCount <= rClickMaxFrames)
            {
                RaycastHit[] mouseHits = GetRaycastHitsAtMousePosition(); // Sets raycast hits when clicking

                // If able to move
                if (IfUnitCanMove(mouseHits))
                {
                    string tagClicked = GetClicked(mouseHits);

                    switch (tagClicked)
                    {
                        case "Resource":

                            if (IfUnitCanGather(uip.selectedUnit, chosenResource)) // If player's selected unit(s) are capable of gathering the requested resource
                            {
                                StartGathering(chosenResource); // Start gathering process
                            }
                            else
                            {
                                MoveUnitAndShowUX(); // Otherwise, move unit to the world position clicked - This may be able to be removed
                            }

                            break;
                        case "BuildingInProgress":

                            if (IfUnitCanBuild(uip.selectedUnit)) // If player's selected unit(s) are capable of working on the requested building in progress
                            {              
                                StartBuild(buildInProg); // Start building process
                            }
                            else
                            {
                                MoveUnitAndShowUX(); // Otherwise, move unit to the world position clicked - This may be able to be removed
                            }

                            break;
                        default:

                            MoveUnitAndShowUX(); // If resource or build in progress hasn't been clicked, move unit to the world position at mouse cursor

                            break;
                    }
                }
            }
        }
    }

    string GetClicked(RaycastHit[] hits) // Returns tag of clicked object
    {
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.CompareTag("Resource"))
            {
                if (hit.transform.GetComponent<Resource>())
                {
                    chosenResource = hit.transform.GetComponent<Resource>();
                }
                else
                {
                    chosenResource = hit.transform.parent.GetComponent<Resource>();
                }

                return "Resource";
            }
            if (hit.transform.CompareTag("BuildingInProgressChild") || hit.transform.CompareTag("BuildingInProgress"))
            {
                if (hit.transform.GetComponent<BuildInProgress>())
                {
                    buildInProg = hit.transform.GetComponent<BuildInProgress>();
                }
                else
                {
                    buildInProg = hit.transform.parent.GetComponent<BuildInProgress>();
                }
                return "BuildingInProgress";
            }
        }

        return string.Empty;
    }

    #region Movement

    bool IfUnitCanMove(RaycastHit[] hits) // Returns true if unit has not clicked on an interactable object
    {
        // Checking if unable to move
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("CompletedBuilding") || hit.transform.gameObject.CompareTag("Unit"))
            {
                return false;
            }
        }

        return true;
    }

    void ProcessMoveUnit(Unit unit) // Stops any prior movement and starts new movement to world position for a unit using NavMesh Agent
    {
        // Ensure movement speed is set appropriately
        unit.agent.speed = unit.GetMoveSpeed();

        // Stop any current navigation
        unit.agent.enabled = false;
        unit.agent.enabled = true;

        // Set villager active task to false as they are being moved manually
        if (IfVillager(unit.GetComponent<Unit>()))
        {
            if (unit.GetComponent<VillagerUnit>().gatherTaskIsActive)
            {
                unit.GetComponent<VillagerUnit>().gatherTaskIsActive = false;
                unit.GetComponent<VillagerUnit>().StopGathering();
                unit.GetComponent<VillagerUnit>().CompleteGatheringTask();
            }

            if (unit.GetComponent<VillagerUnit>().buildTaskIsActive)
            {
                unit.GetComponent<VillagerUnit>().buildTaskIsActive = false;
                unit.GetComponent<VillagerUnit>().StopBuilding();
                unit.GetComponent<VillagerUnit>().CompleteBuildTask();
            }
        }

        unit.agent.SetDestination(GetWorldPosition());
    }

    void MoveUnitAndShowUX() // Shows UX for moving the unit and calls the ProcessMoveUnit function
    {
        // Show UX feedback cursor animation at world position
        ShowMoveUnitUX();

        // Move unit to point clicked
        foreach (Unit unit in uip.selectedUnits)
        {
            ProcessMoveUnit(unit);
        }
    }

    #endregion

    #region Building

    public void ProcessMoveVillagerUnitToBuildInProgress(Unit unit, BuildInProgress bip) // Reset NavMeshAgent movement and move to the build in progress position
    {
        // Reset agent's path
        unit.agent.ResetPath();

        // Ensure movement speed is set appropriately
        unit.agent.speed = unit.GetMoveSpeed();

        // Move to the build
        agentDestination = bip.transform.position;

        //Debug.Log("Moving unit " + unit.gameObject.name + " - toDepot: " + toDepot);

        unit.agent.SetDestination(agentDestination);
    }

    bool IfUnitCanBuild(Unit unit) // returns true if selected unit is capable of processing build command
    {
        if (((VillagerUnit)unit).villagerClass == VillagerClasses.VILLAGER ||
            ((VillagerUnit)unit).villagerClass == VillagerClasses.BUILDER)
        {
            return true;
        }

        return false;
    }

    void PrepareUnitForBuild(Unit unit, BuildInProgress bip) // Stops any navigation and active tasks on the unit, and starts new building task on them
    {
        // Ensure movement speed is set appropriately
        unit.agent.speed = unit.GetMoveSpeed();

        // Stop any current navigation
        unit.agent.enabled = false;
        unit.agent.enabled = true;

        // Set villager active task to false as they are being moved manually
        if (IfVillager(uip.GetVillagerUnit(unit)))
        {
            if (uip.GetVillagerUnit(unit).gatherTaskIsActive)
            {
                uip.GetVillagerUnit(unit).gatherTaskIsActive = false;
                uip.GetVillagerUnit(unit).StopGathering();
                uip.GetVillagerUnit(unit).CompleteGatheringTask();
            }
        }

        if (uip.GetVillagerUnit(unit).buildTaskIsActive)
        {
            uip.GetVillagerUnit(unit).buildTaskIsActive = false;
            uip.GetVillagerUnit(unit).StopBuilding();
            uip.GetVillagerUnit(unit).CompleteBuildTask();
        }

        unit.GetComponent<VillagerUnit>().PrepareBuilding(bip.gameObject);
    }

    void StartBuild(BuildInProgress bip) // Starts build process for each selected unit
    {
        // Turn off action button clicked
        uip.actionMode = ActionModes.IDLE;

        // Show UX feedback cursor animation
        StartCoroutine(ShowTaskConfirmationUX(GetScreenPosition()));

        if (bip.GetComponent<Outline>())
            StartCoroutine(uip.HighlightConfirmedResourceOrBIP(bip.GetComponent<Outline>()));

        foreach (Unit unit in uip.selectedUnits)
        {
            PrepareUnitForBuild(unit, bip);
        }
    }

    #endregion

    #region Gathering

    public void StartGathering(Resource resource) // Ensures selected unit is able to gather the type of requested resource, and begins the gathering process
    {
        // Turn off action button clicked
        uip.actionMode = ActionModes.IDLE;

        // Show UX feedback cursor animation
        StartCoroutine(ShowTaskConfirmationUX(GetScreenPosition()));

        if (resource.GetComponent<Outline>())
            StartCoroutine(uip.HighlightConfirmedResourceOrBIP(resource.GetComponent<Outline>()));

        foreach (Unit unit in uip.selectedUnits)
        {
            if (IfUnitCanGather(unit, resource))
            {
                PrepareUnitForGathering(unit, resource);
            }
        }
    }

    public void ProcessMoveVillagerUnitInGatherTask(bool toDepot, Unit unit, Resource resource, Depot depot) // Reset NavMeshAgent movement and move to the resource position
    {
        // Reset agent's path
        unit.agent.ResetPath();

        // Ensure movement speed is set appropriately
        unit.agent.speed = unit.GetMoveSpeed();

        // Move to the depot or resource
        if (toDepot)
        {
            agentDestination = depot.transform.position;
        }
        else
        {
            agentDestination = resource.transform.position;
        }

        unit.agent.SetDestination(agentDestination);
    }

    bool IfUnitCanGather(Unit unit, Resource resource) // returns true if selected unit is capable of processing gather command
    {
        switch (resource.resourceType)
        {
            case ResourceTypes.WOOD:
                if (uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.VILLAGER || uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.GATHERER
                    || uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.LUMBERJACK)
                    return true;
                break;
            case ResourceTypes.ORE:
                if (uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.VILLAGER || uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.GATHERER
                    || uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.MINER)
                    return true;
                break;
            case ResourceTypes.FOOD:
                if (uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.VILLAGER || uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.GATHERER
                    || uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.FARMER)
                    return true;
                break;
        }

        return false;
    }

    void PrepareUnitForGathering(Unit unit, Resource resource) // Stops any navigation and active tasks on the unit, and starts new gathering task on them
    {
        // Ensure movement speed is set appropriately
        unit.agent.speed = unit.GetMoveSpeed();

        // Stop any current navigation
        unit.agent.enabled = false;
        unit.agent.enabled = true;

        // Set villager active task to false as they are being moved manually
        if (IfVillager(uip.GetVillagerUnit(unit)))
        {
            if (uip.GetVillagerUnit(unit).gatherTaskIsActive)
            {
                uip.GetVillagerUnit(unit).gatherTaskIsActive = false;
                uip.GetVillagerUnit(unit).StopGathering();
                uip.GetVillagerUnit(unit).CompleteGatheringTask();
            }
        }

        if (uip.GetVillagerUnit(unit).buildTaskIsActive)
        {
            uip.GetVillagerUnit(unit).buildTaskIsActive = false;
            uip.GetVillagerUnit(unit).StopBuilding();
            uip.GetVillagerUnit(unit).CompleteBuildTask();
        }

        unit.GetComponent<VillagerUnit>().PrepareGather(resource);
    }

    #endregion

    #region Utilities

    static RaycastHit[] GetRaycastHitsAtMousePosition() // Returns array of objects hit at mouse position
    {
        RaycastHit[] hits;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        hits = Physics.RaycastAll(ray, 1000);
        return hits;
    }

    Vector3 GetWorldPosition() // Returns world position at mouse position if the right click has been registered as a 'single click'
    {
        TerrainCollider tcol = Terrain.activeTerrain.GetComponent<TerrainCollider>();

        if (!IsPointerOverUIElement() && uip.selectedUnits.Count > 0)
        {
            if (Input.GetKeyUp(KeyCode.Mouse1) && rClickFrameCount >= 0 && rClickFrameCount <= rClickMaxFrames)
            {
                RaycastHit hitData;
                Ray terrainRay = Camera.main.ScreenPointToRay(Input.mousePosition); ;

                if (tcol.Raycast(terrainRay, out hitData, 1000))
                {
                    return hitData.point;
                }
            }
        }
        return new Vector3(0, 0, 0);
    }

    Vector3 GetScreenPosition() // Teturns screen position from mouse position
    {
        Vector3 screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 20);
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(screenPoint);

        return screenPos;
    }

    void RightClickFrameCounter() // Determines if right click should be registered as a 'single click'
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

    bool IfVillager(Unit unit) // Returns true if unit is considered a 'villager' class
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

    void ShowMoveUnitUX() // Displays move UX at world position
    {
        FindObjectOfType<CursorManager>().GenerateMarker(GetWorldPosition());
    }

    IEnumerator ShowTaskConfirmationUX(Vector3 dest) // Handles UX for when a task has been confirmed
    {
        bool targetHidden = false;

        GameObject targetAnim = Instantiate(GameObject.FindObjectOfType<NavCursorIcons>().taskTargetPrefab, dest, Quaternion.identity, uip.transform);

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

    #endregion

    #region UI Utilities

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

    #endregion

}
