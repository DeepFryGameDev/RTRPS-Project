using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Types of villager class
public enum VillagerClasses
{
    VILLAGER,
    BUILDER,
    GATHERER,
    FARMER,
    MINER,
    LUMBERJACK,
    VENDOR
}

// This script houses all processes specific to villager units
public class VillagerUnit : Unit
{
    public VillagerClasses villagerClass; // Each villager unit must have 1 villager class assigned

    [HideInInspector] public bool gatherTaskIsActive; // Set to true as long as villager is in gather task.
    [HideInInspector] public int resourcesHolding = 0; // How many resources the unit is holding during gather phase
    [HideInInspector] public float gatherTimeElapsed; // Unit's time that has elapsed for current gather on the resource

    [HideInInspector] public bool buildTaskIsActive; // Set to true as long as villager is in build task
    [HideInInspector] public float personalBuildProgress; // Unit's personal build progress toward the completed build
    [HideInInspector] public float buildTimeElapsed; // Unit's time that has elapsed for current personal build progress

    [HideInInspector] public GatherTask activeGatherTask = new GatherTask(); // Holds the depot and resource for the active gather task for the unit
    [HideInInspector] public Coroutine gatherTaskCoroutine; // The gather task loop is set in a coroutine var so it can be stopped easily at any point
    [HideInInspector] public Coroutine gatherSimTimeCoroutine; // The simulation time for gathering is set in a coroutine var so it can be stopped easily at any point
    [HideInInspector] public Coroutine buildTaskCoroutine; // The build task loop is set in a coroutine var so it can be stopped easily at any point
    [HideInInspector] public Coroutine buildSimTimeCoroutine; // The simulation time for building is set in a coroutine var so it can be stopped easily at any point
    [HideInInspector] public BaseBuilding chosenBuilding; // Set to the building object that is being built

    GatherManager gm; // Used for showing Gather UX and retrieving variables for gather time/volume calculation
    PlayerResources pr; // Used for updating the count of player's held resources
    BuildManager bm; // Used for finishing the building process and retrieving variables for build time/progress calculation
    ResourceTypes originalResourceType; // Original resource type chosen so a new one can be chosen if the chosen resource is destroyed

    GatherPhases gatherPhase; // Used to keep track of the current phase of gathering task
    BuildPhases buildPhase;   // Used to keep track of the current phase of build task

    bool isMoving; // Used to track if unit is moving so destination is only set once

    List<GameObject> listOfRecursiveChildren = new List<GameObject>(); // Used to check child gameObjects recursively to the interaction collision

    private void Awake()
    {
        up = FindObjectOfType<UnitProcessing>();
        uip = FindObjectOfType<UIProcessing>();
        um = FindObjectOfType<UnitMovement>();
        gm = FindObjectOfType<GatherManager>();
        bm = FindObjectOfType<BuildManager>();
        pr = FindObjectOfType<PlayerResources>();

        SetUnitProcessingVars();

        UnitAwake();
    }

    #region Villager Class Stats and Actions

    void SetBaseAttributes() // Used to set base stats for villager units
    {
        baseUnit.usesEnergy = true;

        if (villagerClass.Equals(VillagerClasses.VILLAGER))
        {
            baseUnit.SetStrength(1);
            baseUnit.SetStamina(1);
            baseUnit.SetAgility(1);
            baseUnit.SetLuck(1);
            baseUnit.SetIntelligence(1);
            baseUnit.SetWillpower(1);
            baseUnit.SetMovement(1);
        }
        else if (villagerClass.Equals(VillagerClasses.BUILDER))
        {
            baseUnit.SetStrength(3);
            baseUnit.SetStamina(2);
            baseUnit.SetAgility(1);
            baseUnit.SetLuck(2);
            baseUnit.SetIntelligence(1);
            baseUnit.SetWillpower(2);
            baseUnit.SetMovement(2);
        }
        else if (villagerClass.Equals(VillagerClasses.LUMBERJACK))
        {
            baseUnit.SetStrength(4);
            baseUnit.SetStamina(2);
            baseUnit.SetAgility(1);
            baseUnit.SetLuck(1);
            baseUnit.SetIntelligence(2);
            baseUnit.SetWillpower(1);
            baseUnit.SetMovement(2);
        }
        else if (villagerClass.Equals(VillagerClasses.MINER))
        {
            baseUnit.SetStrength(2);
            baseUnit.SetStamina(4);
            baseUnit.SetAgility(1);
            baseUnit.SetLuck(1);
            baseUnit.SetIntelligence(1);
            baseUnit.SetWillpower(2);
            baseUnit.SetMovement(2);
        }
        else if (villagerClass.Equals(VillagerClasses.FARMER))
        {
            baseUnit.SetStrength(1);
            baseUnit.SetStamina(2);
            baseUnit.SetAgility(1);
            baseUnit.SetLuck(1);
            baseUnit.SetIntelligence(4);
            baseUnit.SetWillpower(2);
            baseUnit.SetMovement(2);
        }
    }

    protected override void SetUnitProcessingVars() // Initializes required default variables for the villager units
    {
        base.SetUnitProcessingVars();

        SetBaseAttributes();

        SetMaxHP();
        baseUnit.SetHP(baseUnit.GetMaxHP());
        SetMaxEnergy();
        baseUnit.SetMP(baseUnit.GetMaxMP());

        SetGraphic();
    }

    void SetMaxHP() // Sets unit's max HP based on Stamina and Willpower
    {
        baseUnit.SetMaxHP(Mathf.RoundToInt((baseUnit.GetStamina() * up.vilHPFromStaminaFactor) + baseUnit.GetWillpower() * up.vilHPFromWillpowerFactor));
    }

    void SetMaxEnergy() // Sets unit's max Energy based on Stamina
    {
        baseUnit.SetMaxMP(Mathf.RoundToInt(baseUnit.GetStamina() * up.vilEnergyFromStaminaFactor));
    }

    void SetGraphic() // Sets unit's face graphic
    {
        UnitSpriteGraphics usg = up.GetComponent<UnitSpriteGraphics>();

        switch (villagerClass)
        {
            case VillagerClasses.VILLAGER:
                baseUnit.SetFaceGraphic(usg.VillagerFace);
                break;
            case VillagerClasses.FARMER:
                baseUnit.SetFaceGraphic(usg.FarmerFace);
                break;
            case VillagerClasses.LUMBERJACK:
                baseUnit.SetFaceGraphic(usg.LumberjackFace);
                break;
            case VillagerClasses.MINER:
                baseUnit.SetFaceGraphic(usg.MinerFace);
                break;
            case VillagerClasses.BUILDER:
                baseUnit.SetFaceGraphic(usg.BuilderFace);
                break;
        }
    }

    #endregion

    #region Gathering

    public void PrepareGather(Resource resource) // Sets required variables for gathering task to be created and started
    {
        gatherTimeElapsed = 0;

        // set activeGatherTask.resource to the resource requested
        activeGatherTask.resource = resource;
        originalResourceType = resource.resourceType;

        // set gatherPhase = SEEKINGRESOURCE
        gatherPhase = GatherPhases.SEEKINGRESOURCE;

        // turn on gatherTaskIsActive
        gatherTaskIsActive = true;

        // start GatherResource
        gatherTaskCoroutine = StartCoroutine(GatherResource());
    }    

    IEnumerator GatherResource() // The loop to be run while gather task is active - this holds the actual process of gathering resources and dropping them off at the closest applicable depot
    {
        while (gatherTaskIsActive)
        {
            switch (gatherPhase)
            {
                case GatherPhases.SEEKINGRESOURCE:

                    if (resourcesHolding == GetCarryLimit()) // if carryLimit has been reached
                    {
                        gatherPhase = GatherPhases.MOVETODEPOT; // set gatherPhase to MOVETODEPOT (this would occur if player attempts to send unit to gather if they have already reached carryLimit)
                        continue;
                    }

                    if (activeGatherTask.resource == null) // confirm chosen resource is not null.
                    {
                        if (GetClosestResource() == null) // if closest resource turns null at any point
                        {
                            // all resources on the map have been depleted, and the coroutine can be exit by setting gatherTaskIsActive to false and using yield break.
                            gatherTaskIsActive = false;
                            yield break;
                        } else
                        {   // If at any point it is null during this phase, but there are still resources available, the closest resource of same type should be set to new chosen resource
                            activeGatherTask.resource = GetClosestResource();
                        }                        
                    }

                    if (activeGatherTask.resource.unitsInteracting.Count == activeGatherTask.resource.maxUnitsGathering) // If at any point there are x units already on the resource(GatherManager.maxUnitsGatheringFromOneResource.Count)
                    {
                        activeGatherTask.resource = GetClosestResource(); // set the closest resource of same type
                        isMoving = false; // reset move so below can be re-run
                    }

                    if (resourcesHolding < GetCarryLimit() && !isMoving) // if carryLimit has not been reached
                    {
                        // move to chosen resource
                        um.ProcessMoveVillagerUnitInGatherTask(false, this, activeGatherTask.resource, null);
                        isMoving = true;
                    }

                    if (!agent.pathPending && IsAtDest(GetInteractionCollision(activeGatherTask.resource.gameObject))) // once in range of chosen resource
                    {
                        StopAgentMovement(); // stop moving

                        activeGatherTask.resource.unitsInteracting.Add(this); // add unit to resource's unitsInteracting

                        gatherPhase = GatherPhases.GATHERING; // set gatherPhase to GATHERING
                    }

                    break;
            case GatherPhases.GATHERING:

                    if (resourcesHolding < GetCarryLimit() && (activeGatherTask.resource == null || activeGatherTask.resource.resourcesRemaining == 0)) // if resources on this are gone and can carry more
                    {
                        // set mode back to SEEKINGRESOURCE
                        gatherPhase = GatherPhases.SEEKINGRESOURCE;
                        continue;
                    }

                    transform.LookAt(activeGatherTask.resource.transform); // look at resource (in case unit was moved between gathering)

                    if (resourcesHolding < GetCarryLimit() && activeGatherTask.resource.resourcesRemaining > 0) // while carry limit has not been reached
                    {
                        // gather once
                        //yield return new WaitForSeconds(GetGatherTime()); //simulates time gathering
                        yield return gatherSimTimeCoroutine = StartCoroutine(SimGatherTime()); //simulates time gathering
                        AddResourcetoUnit(); // adds resource to unit
                        gm.ShowResourceGatherUX(this.gameObject, activeGatherTask.resource.resourceType, 1, true); // show UX feedback

                        //Debug.Log(gameObject.name + " carrying " + activeGatherTask.resource.resourceType + " - " + resourcesHolding + "/" + GetCarryLimit());
                    }

                    if (resourcesHolding == GetCarryLimit()) // once carry limit has been reached
                    {
                        // remove unit from resource's unitsInteracting
                        activeGatherTask.resource.unitsInteracting.Remove(this);

                        // set gatherPhase to MOVETODEPOT:
                        gatherPhase = GatherPhases.MOVETODEPOT;
                    }

                    break;
            case GatherPhases.MOVETODEPOT:
                    bool changedDepot = false;

                    if (activeGatherTask.depot != GetClosestDepot()) // If at any point during this phase the depot is null (player or enemy destroyed it)
                    {
                        activeGatherTask.depot = GetClosestDepot(); // Set closest depot
                        changedDepot = true;
                    }

                    if (agent.velocity == Vector3.zero || changedDepot) // if haven't started moving to depot or depot was changed
                    {
                        // move to depot
                        um.ProcessMoveVillagerUnitInGatherTask(true, this, activeGatherTask.resource, activeGatherTask.depot);
                    }

                    if (!agent.pathPending && IsAtDest(GetInteractionCollision(activeGatherTask.depot.gameObject))) // once in range of depot
                    {
                        StopAgentMovement(); // stop moving

                        gatherPhase = GatherPhases.DEPOSITING; // set gatherPhase to DEPOSITING
                    }

                    break;
            case GatherPhases.DEPOSITING:
                    transform.LookAt(activeGatherTask.depot.transform); // look at depot

                    yield return new WaitForSeconds(up.vilResourceDropoffTime); // simulates dropping resource off
                    DropResourceOffAtDepot(); // deposit resources into the chosen depot

                    if ((activeGatherTask.resource == null || activeGatherTask.resource.resourcesRemaining == 0) && GetClosestResource() == null) // if no more resources available, exit from the coroutine by setting gatherTaskIsActive to false and using yield break.
                    {
                        gatherTaskIsActive = false;
                        yield break;
                    } else
                    {
                        gatherPhase = GatherPhases.SEEKINGRESOURCE; // if this point is reached, loop back to the beginning phase - SEEKINGRESOURCE
                    }

                    break;
            }

            //yield return new WaitForEndOfFrame();
            yield return null;
        }

        CompleteGatheringTask();
    }

    public void CompleteGatheringTask() // Stops any gather time simulation if it is still in process and stops gathering
    {
        if (gatherSimTimeCoroutine != null)
        {
            StopCoroutine(gatherSimTimeCoroutine);
        }

        if (gatherTaskCoroutine != null)
        {
            StopGathering();
        }
    }

    IEnumerator SimGatherTime() // Simulates the amount of time it takes to gather a resource
    {
        gatherTimeElapsed = 0;

        while (gatherTimeElapsed < GetGatherTime())
        {
            gatherTimeElapsed += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }

    void AddResourcetoUnit() // Removes resource type from resource object and adds it to the unit's current count
    {
        activeGatherTask.resource.resourcesRemaining--;
        resourcesHolding++;
    }

    void DropResourceOffAtDepot() // Processes the depositing of a resource at the current task's depot
    {
        switch (activeGatherTask.resource.resourceType)
        {
            case ResourceTypes.WOOD:
                pr.wood += resourcesHolding;
                break;
            case ResourceTypes.ORE:
                pr.ore += resourcesHolding;
                break;
            case ResourceTypes.FOOD:
                pr.food += resourcesHolding;
                break;
        }

        gm.ShowResourceGatherUX(activeGatherTask.depot.gameObject, activeGatherTask.resource.resourceType, resourcesHolding, true);
        gm.ShowResourceGatherUX(this.gameObject, activeGatherTask.resource.resourceType, resourcesHolding, false);

        resourcesHolding = 0;
    }

    Depot GetClosestDepot() // Returns the closest depot to the unit's position to deposit held resources
    {
        GameObject[] objs;
        objs = GameObject.FindGameObjectsWithTag("Depot");

        depotResources chosenDepotType = new depotResources();

        switch (activeGatherTask.resource.resourceType)
        {
            case ResourceTypes.FOOD:
                chosenDepotType = depotResources.FOOD;
                break;
            case ResourceTypes.ORE:
                chosenDepotType = depotResources.ORE;
                break;
            case ResourceTypes.WOOD:
                chosenDepotType = depotResources.WOOD;
                break;
        }

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 tempPos = transform.position;

        foreach (GameObject depot in objs)
        {
            if (depot.GetComponent<Depot>().building.depotResource == depotResources.ALL || depot.GetComponent<Depot>().building.depotResource == chosenDepotType)
            {
                Vector3 diff = depot.transform.position - tempPos;
                if (diff.sqrMagnitude < distance)
                {
                    closest = depot;
                    distance = diff.sqrMagnitude;
                }
            }
        }

        return closest.GetComponent<Depot>();
    }

    Resource GetClosestResource() // Returns the closest resource of original resource type to the unit's position
    {
        GameObject[] objs;
        objs = GameObject.FindGameObjectsWithTag("Resource");

        Resource closest = null;
        float distance = Mathf.Infinity;
        Vector3 tempPos = transform.position;

        foreach (GameObject resourceObj in objs)
        {
            Resource resourceToCheck;
            if (resourceObj.GetComponent<Resource>())
            {
                resourceToCheck = resourceObj.GetComponent<Resource>();
            } else
            {
                resourceToCheck = resourceObj.transform.parent.GetComponent<Resource>();
            }

            if (resourceToCheck.resourceType == originalResourceType &&
                resourceToCheck.resourcesRemaining > 0 &&
                resourceToCheck != activeGatherTask.resource)
            {
                Vector3 diff = resourceObj.transform.position - tempPos;
                if (diff.sqrMagnitude < distance)
                {
                    closest = resourceToCheck;
                    distance = diff.sqrMagnitude;
                }
            }
        }

        return closest;
    }

    public void StopGathering() // Immediately stops the gather task 
    {
        isMoving = false;
        StopCoroutine(gatherTaskCoroutine);
    }

    public int GetCarryLimit() // Returns the carry limit based on resource type chosen and unit's stats
    {
        switch (activeGatherTask.resource.resourceType)
        {
            case ResourceTypes.WOOD:
                return Mathf.RoundToInt(baseUnit.GetStrength() * up.vilWoodCarryLimitFromStrengthFactor);
            case ResourceTypes.ORE:
                return Mathf.RoundToInt(baseUnit.GetStamina() * up.vilOreCarryLimitFromStaminaFactor);
            case ResourceTypes.FOOD:
                return Mathf.RoundToInt(baseUnit.GetIntelligence() * up.vilFoodCarryLimitFromIntelligenceFactor);
        }

        return 0;
    }

    public float GetGatherTime() // Returns gather time based on resource type chosen and unit's stats
    {
        float tempRate;

        switch (activeGatherTask.resource.resourceType)
        {
            case ResourceTypes.WOOD: // strength and willpower
                tempRate = gm.maxGatherTime - ((baseUnit.GetStrength() * gm.gatherWoodTimeStrengthFactor) + (baseUnit.GetWillpower() * gm.gatherWoodTimeWillpowerFactor));
                break;
            case ResourceTypes.ORE: // strength and stamina
                tempRate = gm.maxGatherTime - ((baseUnit.GetStrength() * gm.gatherOreTimeStrengthFactor) + (baseUnit.GetWillpower() * gm.gatherOreTimeStaminaFactor));
                break;
            case ResourceTypes.FOOD: // intelligence and willpower
                tempRate = gm.maxGatherTime - ((baseUnit.GetIntelligence() * gm.gatherFoodTimeIntelligenceFactor) + (baseUnit.GetWillpower() * gm.gatherFoodTimeWillpowerFactor));
                break;
            default:
                Debug.LogError("GetGatherTime - Invalid resource type");
                tempRate = 0;
                break;
        }

        if (tempRate < gm.minGatherTime)
            return gm.minGatherTime;

        return tempRate;
    }

    #endregion

    #region Building

    public void PrepareBuilding(GameObject building) // Sets required variables for build task to be created and started
    {
        buildTimeElapsed = 0;
        buildTaskIsActive = true;
        buildPhase = BuildPhases.MOVETOBUILDING;

        // Start building
        buildTaskCoroutine = StartCoroutine(BuildBuilding(building.GetComponent<BuildInProgress>()));
    }

    IEnumerator BuildBuilding(BuildInProgress bip) // The states to be run while build task is active - this holds the actual process of moving to a building in progress and contributing to it
    {
        while (buildTaskIsActive)
        {
            switch (buildPhase)
            {
                case BuildPhases.MOVETOBUILDING:
                    if (bip == null || bip.progress == 100 || // confirm chosen building is not null or completed. (it was destroyed/completed while en route)
                        bip.unitsInteracting.Count == bip.building.maxUnitsInteracting) //  Or, there are too many units already working on the build
                    {
                        // stop agent movement
                        StopAgentMovement();

                        // cancel this task
                        buildTaskIsActive = false;

                        // break from loop
                        break;
                    }

                    if (bip.progress < 100 && agent.velocity == Vector3.zero) // if progress=100 has not been reached
                    {
                        // move to build
                        um.ProcessMoveVillagerUnitToBuildInProgress(this, bip); // this possibly needs to be updated
                    }

                    if (agent.velocity != Vector3.zero && IsAtDest(GetInteractionCollision(bip.gameObject))) // once in range of chosen build
                    {
                        StopAgentMovement(); // stop moving

                        transform.LookAt(bip.transform); // look at build

                        buildPhase = buildPhase = BuildPhases.PROCESSBUILD; // set buildPhase to PROCESSBUILD
                    }

                    break;
                case BuildPhases.PROCESSBUILD:

                    // add to build in progress units interacting
                    if (!bip.unitsInteracting.Contains(this))
                    {
                        bip.unitsInteracting.Add(this);
                    }

                    if (bip.destroyed)
                    {
                        buildTaskIsActive = false;
                        break;
                    }

                    if (bip.progress < 100)
                    {
                        //yield return new WaitForSeconds(GetBuildTime()); // simulates building time
                        yield return buildSimTimeCoroutine = StartCoroutine(SimBuildTime(bip));

                        if (bip.progress < 100)
                            ContributeToBuild(bip);
                    }

                    if (bip.progress == 100 && !bip.destroyed)
                    {
                        // cancel this task
                        bm.FinishBuildingProcess(bip);
                        buildTaskIsActive = false;
                    }

                    break;
            }
            yield return null;
        }

        CompleteBuildTask();
    }

    public void CompleteBuildTask() // Stops any build time simulation if it is still in process and stops building
    {
        personalBuildProgress = 0;
        if (buildSimTimeCoroutine != null)
        {
            StopCoroutine(buildSimTimeCoroutine);
        }

        if (buildTaskCoroutine != null)
        {
            StopBuilding();
        }
    }

    IEnumerator SimBuildTime(BuildInProgress bip) // Simulates the amount of time it takes to contribute to a build
    {
        buildTimeElapsed = 0;

        while (buildTimeElapsed < GetBuildTime() && bip.progress < 100)
        {
            buildTimeElapsed += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }

    public void StopBuilding() // Immediately stops the build task
    {
        StopCoroutine(buildTaskCoroutine);
    }

    void ContributeToBuild(BuildInProgress bip) // Increases the personal build progress, and when personal build progress reaches 100%, increases progress to the build in progress
    {
        if (personalBuildProgress == 100)
        {
            personalBuildProgress = 0;
        }

        if (bip.progress < 100)
        {
            float adjBuildProgress = GetBuildPersonalProgress();
            personalBuildProgress += adjBuildProgress;

            bm.ShowBuildProgressUX(this.gameObject, true, personalBuildProgress);
        }

        if (personalBuildProgress >= 100)
        {
            float adjProgress = GetBuildTotalProgress();
            bip.IncreaseProgress(adjProgress); 
            personalBuildProgress = 100;

            if (bip != null)
            {
                bm.ShowBuildProgressUX(bip.gameObject, false, bip.progress);
            }            
        }

        if (bip.progress > 100)
        {
            bip.progress = 100;
        }
    }

    public float GetBuildTime() // Returns amount of time to run personal build using unit's agility, intelligence, and willpower
    {
        float tempRate;

        tempRate = bm.maxBuildTime - ((baseUnit.GetAgility() * bm.buildTimeAgilityFactor) + (baseUnit.GetIntelligence() * bm.buildTimeIntelligenceFactor) + (baseUnit.GetWillpower() * bm.buildTimeWillpowerFactor));

        if (tempRate < bm.minBuildTime)
            return bm.minBuildTime;

        return tempRate;
    }

    float GetBuildPersonalProgress() // Returns value to increase personal build progress using strength and intelligence
    {
        float tempPerProg;

        tempPerProg = (bm.buildPerProgFactor * ((baseUnit.GetStrength() * bm.buildPerProgStrengthFactor) + baseUnit.GetIntelligence() * bm.buildPerProgIntelligenceFactor));

        if (tempPerProg < bm.minBuildPerProg)
            return bm.minBuildPerProg;

        if (tempPerProg > bm.maxBuildPerProg)
            return bm.maxBuildPerProg;

        return tempPerProg;
    }

    float GetBuildTotalProgress() // Returns value to increase total build's progress using willpower and stamina
    {
        float tempTotProg;

        tempTotProg = (bm.buildTotProgFactor * ((baseUnit.GetWillpower() * bm.buildTotProgWillpowerFactor) + baseUnit.GetStamina() * bm.buildTotProgStaminaFactor));

        if (tempTotProg < bm.minBuildTotProg)
            return bm.minBuildTotProg;

        if (tempTotProg > bm.maxBuildTotProg)
            return bm.maxBuildTotProg;

        return tempTotProg;
    }

    #endregion

    #region Utilities

    bool IsAtDest(InteractionCollision destInteractionCollision) // Returns true if unit collides with destination's interaction collider
    {
        if (destInteractionCollision.unitsInteracting.Contains(this))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void StopAgentMovement() // Stops agent from moving by setting isStopped to true and turning agent off and back on
    {
        agent.isStopped = true;
        agent.enabled = false;
        agent.enabled = true;
        isMoving = false;
    }

    InteractionCollision GetInteractionCollision(GameObject objToCheck) // Sets the destInteractionCollision to the target object's attached InteractionCollision
    {
        listOfRecursiveChildren.Clear();

        GetRecursiveChildren(objToCheck);

        foreach (GameObject obj in listOfRecursiveChildren)
        {
            if (obj.GetComponent<InteractionCollision>())
            {
                return obj.GetComponent<InteractionCollision>();
            }
        }

        return null;
    }

    private void GetRecursiveChildren(GameObject obj) // Finds all children recursively attached to provided object to find the Interaction Collision attached to it
    {
        if (null == obj)
            return;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
                continue;
            //child.gameobject contains the current child you can do whatever you want like add it to an array
            listOfRecursiveChildren.Add(child.gameObject);
            GetRecursiveChildren(child.gameObject);
        }
    }

    #endregion
}
