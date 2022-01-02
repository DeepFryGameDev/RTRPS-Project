using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum villagerClasses
{
    VILLAGER,
    BUILDER,
    GATHERER,
    FARMER,
    MINER,
    LUMBERJACK,
    VENDOR
}

public class VillagerUnit : Unit
{
    public villagerClasses villagerClass;

    [ReadOnly] public bool gatherTaskIsActive, buildTaskIsActive;
    [ReadOnly] public int resourcesHolding = 0;
    [ReadOnly] public float personalBuildProgress;

    [HideInInspector] public float buildTimeElapsed, gatherTimeElapsed;

    [HideInInspector] public GatherTask activeGatherTask = new GatherTask();
    //[HideInInspector] public BuildTask activeBuildTask = new BuildTask();
    [HideInInspector] public Coroutine gatherTaskCoroutine;

    GatherManager gm;
    GatherPhases gatherPhase;
    ResourceTypes originalResourceType;

    BuildManager bm;
    BuildPhases buildPhase;
    [HideInInspector] public BaseBuilding chosenBuilding;

    void SetBaseAttributes()
    {
        usesEnergy = true;

        if (villagerClass.Equals(villagerClasses.VILLAGER))
        {
            SetStrength(1);
            SetStamina(1);
            SetAgility(1);
            SetLuck(1);
            SetIntelligence(1);
            SetWillpower(1);
            SetMovement(1);
        }
        else if (villagerClass.Equals(villagerClasses.BUILDER))
        {
            SetStrength(3);
            SetStamina(2);
            SetAgility(1);
            SetLuck(2);
            SetIntelligence(1);
            SetWillpower(2);
            SetMovement(2);
        }
        else if (villagerClass.Equals(villagerClasses.LUMBERJACK))
        {
            SetStrength(4);
            SetStamina(2);
            SetAgility(1);
            SetLuck(1);
            SetIntelligence(2);
            SetWillpower(1);
            SetMovement(2);
        }
        else if (villagerClass.Equals(villagerClasses.MINER))
        {
            SetStrength(2);
            SetStamina(4);
            SetAgility(1);
            SetLuck(1);
            SetIntelligence(1);
            SetWillpower(2);
            SetMovement(2);
        }
        else if (villagerClass.Equals(villagerClasses.FARMER))
        {
            SetStrength(1);
            SetStamina(2);
            SetAgility(1);
            SetLuck(1);
            SetIntelligence(4);
            SetWillpower(2);
            SetMovement(2);
        }
    }

    private void Awake()
    {
        up = FindObjectOfType<UnitProcessing>();
        um = FindObjectOfType<UnitMovement>();
        gm = FindObjectOfType<GatherManager>();
        bm = FindObjectOfType<BuildManager>();

        SetUnitProcessingVars();

        UnitAwake();
    }

    #region Gathering

    public void PrepareGather(Resource resource)
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

    IEnumerator GatherResource()
    {
        while (gatherTaskIsActive)
        {
            switch (gatherPhase)
            {
                case GatherPhases.SEEKINGRESOURCE:
                    bool changedResource = false;

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
                            changedResource = true;
                        }                        
                    }

                    if (activeGatherTask.resource.unitsInteracting.Count == activeGatherTask.resource.maxUnitsGathering) // If at any point there are x units already on the resource(GatherManager.maxUnitsGatheringFromOneResource.Count)
                    {
                        activeGatherTask.resource = GetClosestResource(); // set the closest resource of same type
                        changedResource = true;
                    }

                    if (resourcesHolding < GetCarryLimit() && (changedResource || agent.velocity == Vector3.zero)) // if carryLimit has not been reached
                    {
                        // move to chosen resource
                        um.ProcessMoveVillagerUnitInGatherTask(false, this, activeGatherTask.resource, null);
                    }

                    if (!agent.pathPending && IsAtDest(activeGatherTask.resource.transform)) // once in range of chosen resource
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

                    transform.LookAt(activeGatherTask.resource.transform); // look at resource (in case it was moved between gathering)

                    if (resourcesHolding < GetCarryLimit() && activeGatherTask.resource.resourcesRemaining > 0) // while carry limit has not been reached
                    {
                        // gather once
                        //yield return new WaitForSeconds(GetGatherTime()); //simulates time gathering
                        yield return StartCoroutine(SimGatherTime()); //simulates time gathering
                        AddResourcetoUnit(); // adds resource to unit
                        StartCoroutine(gm.ShowResourceGatherUX(this.gameObject, activeGatherTask.resource.resourceType, 1, true)); // show UX feedback

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

                    if (activeGatherTask.depot != GetClosestDepot(activeGatherTask.resource)) // If at any point during this phase the depot is null (player or enemy destroyed it)
                    {
                        activeGatherTask.depot = GetClosestDepot(activeGatherTask.resource); // Set closest depot
                        changedDepot = true;
                    }

                    if (agent.velocity == Vector3.zero || changedDepot) // if haven't started moving to depot or depot was changed
                    {
                        // move to depot
                        um.ProcessMoveVillagerUnitInGatherTask(true, this, activeGatherTask.resource, activeGatherTask.depot);
                    }

                    if (!agent.pathPending && IsAtDest(activeGatherTask.depot.transform)) // once in range of depot
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
    }

    IEnumerator SimGatherTime()
    {
        gatherTimeElapsed = 0;

        while (gatherTimeElapsed < GetGatherTime())
        {
            gatherTimeElapsed += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }

    void AddResourcetoUnit()
    {
        activeGatherTask.resource.resourcesRemaining--;
        resourcesHolding++;
    }

    void DropResourceOffAtDepot()
    {
        if (activeGatherTask.resource.resourceType == ResourceTypes.WOOD)
        {
            GameObject.FindObjectOfType<PlayerResources>().wood += resourcesHolding;
        } else if (activeGatherTask.resource.resourceType == ResourceTypes.ORE)
        {
            GameObject.FindObjectOfType<PlayerResources>().ore += resourcesHolding;
        } else if (activeGatherTask.resource.resourceType == ResourceTypes.FOOD)
        {
            GameObject.FindObjectOfType<PlayerResources>().food += resourcesHolding;
        }

        StartCoroutine(gm.ShowResourceGatherUX(activeGatherTask.depot.gameObject, activeGatherTask.resource.resourceType, resourcesHolding, true));
        StartCoroutine(gm.ShowResourceGatherUX(this.gameObject, activeGatherTask.resource.resourceType, resourcesHolding, false));

        resourcesHolding = 0;
    }

    bool IsAtDest(Transform dest)
    {
        bool atDest = false;

        if (Vector3.Distance(transform.position, dest.position) <= GetComponent<NavMeshAgent>().stoppingDistance &&
           GetComponent<NavMeshAgent>().remainingDistance <= GetComponent<NavMeshAgent>().stoppingDistance)
        {
            atDest = true;
        }

        return atDest;
    }

    Depot GetClosestDepot(Resource resource)
    {
        GameObject[] objs;
        objs = GameObject.FindGameObjectsWithTag("Depot");

        depotTypes chosenDepotType = new depotTypes();

        if (activeGatherTask.resource.resourceType == ResourceTypes.FOOD)
        {
            chosenDepotType = depotTypes.FOOD;
        } else if (activeGatherTask.resource.resourceType == ResourceTypes.ORE)
        {
            chosenDepotType = depotTypes.ORE;
        } else if (activeGatherTask.resource.resourceType == ResourceTypes.WOOD)
        {
            chosenDepotType = depotTypes.WOOD;
        }

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 tempPos = transform.position;

        foreach (GameObject depot in objs)
        {
            if (depot.GetComponent<Depot>().depotType == depotTypes.ALL || depot.GetComponent<Depot>().depotType == chosenDepotType)
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

    Resource GetClosestResource()
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

    public void StopGathering()
    {
        StopCoroutine(gatherTaskCoroutine);
    }

    #endregion

    #region Building

    public void PrepareBuilding(GameObject building)
    {
        buildTimeElapsed = 0;
        buildTaskIsActive = true;
        buildPhase = BuildPhases.MOVETOBUILDING;

        // Start building
        StartCoroutine(BuildBuilding(building.GetComponent<BuildInProgress>()));
    }

    IEnumerator BuildBuilding(BuildInProgress bip)
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

                    if (bip.progress < 100) // if progress=100 has not been reached
                    {
                        // move to build
                        um.ProcessMoveVillagerUnitToBuildInProgress(this, bip); // this needs to be updated
                    }

                    if (!agent.pathPending && IsAtDest(bip.transform)) // once in range of chosen build
                    {
                        StopAgentMovement(); // stop moving

                        buildPhase = buildPhase = BuildPhases.PROCESSBUILD; // set buildPhase to PROCESSBUILD
                    }

                    break;
                case BuildPhases.PROCESSBUILD:
                    // add to build in progress units interacting
                    if (!bip.unitsInteracting.Contains(this))
                    {
                        bip.unitsInteracting.Add(this);
                    }

                    if (bip.progress < 100)
                    {
                        //yield return new WaitForSeconds(GetBuildTime()); // simulates building time
                        yield return StartCoroutine(SimBuildTime());
                        ContributeToBuild(bip);
                    }

                    if (bip.progress == 100)
                    {
                        // cancel this task
                        bm.FinishBuildingProcess(bip);
                        buildTaskIsActive = false;
                    }

                    break;
            }
            yield return null;
        }                    
    }

    IEnumerator SimBuildTime()
    {
        buildTimeElapsed = 0;

        while (buildTimeElapsed < GetBuildTime())
        {
            buildTimeElapsed += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }        
    }

    private void ContributeToBuild(BuildInProgress bip)
    {
        if (personalBuildProgress == 100)
        {
            personalBuildProgress = 0;
        }

        if (bip.progress < 100)
        {
            float adjBuildProgress = GetBuildPersonalProgress();
            personalBuildProgress += adjBuildProgress; 

            // show UX feedback for this
        }

        if (personalBuildProgress >= 100)
        {
            float adjProgress = GetBuildTotalProgress();
            bip.IncreaseProgress(adjProgress); 
            personalBuildProgress = 100;

            // show UX feedback for this
        }

        if (bip.progress > 100)
        {
            bip.progress = 100;
        }
    }

    #endregion

    void StopAgentMovement()
    {
        agent.isStopped = true;
        agent.enabled = false;
        agent.enabled = true;
    }

    protected override void SetUnitProcessingVars()
    {
        base.SetUnitProcessingVars();

        SetBaseAttributes();

        SetMaxHP();
        SetHP(GetMaxHP());
        SetMaxMP();
        SetMP(GetMaxMP());

        SetGraphic();
    }

    void SetMaxHP()
    {
        this.SetMaxHP(Mathf.RoundToInt((GetStamina() * up.vilHPFromStaminaFactor) + GetWillpower() * up.vilHPFromWillpowerFactor));
    }

    void SetMaxMP()
    {
        this.SetMaxMP(Mathf.RoundToInt(GetStamina() * up.vilEnergyFromStaminaFactor));
    }

    public float GetBuildTime() // uses agility, intelligence, and willpower
    {
        float tempRate;

        tempRate = bm.maxBuildTime - ((GetAgility() * bm.buildTimeAgilityFactor) + (GetIntelligence() * bm.buildTimeIntelligenceFactor) + (GetWillpower() * bm.buildTimeWillpowerFactor));

        if (tempRate < bm.minBuildTime)
            return bm.minBuildTime;

        return tempRate;
    }

    float GetBuildPersonalProgress() // uses strength and intelligence
    {
        float tempPerProg;

        tempPerProg = (bm.buildPerProgFactor * ((GetStrength() * bm.buildPerProgStrengthFactor) + GetIntelligence() * bm.buildPerProgIntelligenceFactor));

        if (tempPerProg < bm.minBuildPerProg)
            return bm.minBuildPerProg;

        if (tempPerProg > bm.maxBuildPerProg)
            return bm.maxBuildPerProg;

        return tempPerProg;
    }

    float GetBuildTotalProgress() // uses willpower and stamina
    {
        float tempTotProg;

        tempTotProg = (bm.buildTotProgFactor * ((GetWillpower() * bm.buildTotProgWillpowerFactor) + GetStamina() * bm.buildTotProgStaminaFactor));

        if (tempTotProg < bm.minBuildTotProg)
            return bm.minBuildTotProg;

        if (tempTotProg > bm.maxBuildTotProg)
            return bm.maxBuildTotProg;

        return tempTotProg;
    }

    public int GetCarryLimit()
    {
        if (activeGatherTask.resource.resourceType == ResourceTypes.WOOD)
        {
            return Mathf.RoundToInt(GetStrength() * up.vilWoodCarryLimitFromStrengthFactor);
        } else if (activeGatherTask.resource.resourceType == ResourceTypes.ORE)
        {
            return Mathf.RoundToInt(GetStamina() * up.vilOreCarryLimitFromStaminaFactor);
        } else if (activeGatherTask.resource.resourceType == ResourceTypes.FOOD)
        {
            return Mathf.RoundToInt(GetIntelligence() * up.vilFoodCarryLimitFromIntelligenceFactor);
        }

        return 0;
    }

    public float GetGatherTime() // uses variety of stats based on resource
    {
        float tempRate = 0;

        switch (activeGatherTask.resource.resourceType)
        {
            case ResourceTypes.WOOD: // strength and willpower
                tempRate = gm.maxGatherTime - ((GetStrength() * gm.gatherWoodTimeStrengthFactor) + (GetWillpower() * gm.gatherWoodTimeWillpowerFactor));
                break;
            case ResourceTypes.ORE: // strength and stamina
                tempRate = gm.maxGatherTime - ((GetStrength() * gm.gatherOreTimeStrengthFactor) + (GetWillpower() * gm.gatherOreTimeStaminaFactor));
                break;
            case ResourceTypes.FOOD: // intelligence and willpower
                tempRate = gm.maxGatherTime - ((GetIntelligence() * gm.gatherFoodTimeIntelligenceFactor) + (GetWillpower() * gm.gatherFoodTimeWillpowerFactor));
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

    void SetGraphic()
    {
        UnitSpriteGraphics usg = up.GetComponent<UnitSpriteGraphics>();

        switch (villagerClass)
        {
            case villagerClasses.VILLAGER:
                SetFaceGraphic(usg.VillagerFace);
                break;
            case villagerClasses.FARMER:
                SetFaceGraphic(usg.FarmerFace);
                break;
            case villagerClasses.LUMBERJACK:
                SetFaceGraphic(usg.LumberjackFace);
                break;
            case villagerClasses.MINER:
                SetFaceGraphic(usg.MinerFace);
                break;
            case villagerClasses.BUILDER:
                SetFaceGraphic(usg.BuilderFace);
                break;
        }
    }
}
