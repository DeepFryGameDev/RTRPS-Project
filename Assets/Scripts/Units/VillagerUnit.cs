using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum villagerClasses
{
    VILLAGER,
    BUILDER,
    FARMER,
    MINER,
    LUMBERJACK,
    VENDOR
}

public class VillagerUnit : Unit
{
    public villagerClasses villagerClass;

    [ReadOnly] public bool gatherTaskIsActive;
    [ReadOnly] public int resourcesHolding = 0;

    [HideInInspector] public GatherTask activeGatherTask = new GatherTask();
    [HideInInspector] public Coroutine gatherTaskCoroutine;

    GatherManager gm;
    GatherPhases gatherPhase;
    ResourceTypes originalResourceType;

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
            SetStrength(1);
            SetStamina(4);
            SetAgility(2);
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

        SetUnitProcessingVars();

        UnitAwake();
    }

    public void PrepareGather(Resource resource)
    {
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
                        um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, null);
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
                        yield return new WaitForSeconds(1); //simulates time gathering - this will be updated to take into account unit's stats
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
                        um.ProcessMoveVillagerUnitInTask(true, this, activeGatherTask.resource, activeGatherTask.depot);
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

    void StopAgentMovement()
    {
        agent.isStopped = true;
        agent.enabled = false;
        agent.enabled = true;
    }

    public void StopGathering()
    {
        StopCoroutine(gatherTaskCoroutine);
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
        }
    }
}
