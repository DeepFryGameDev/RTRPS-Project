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

    void prototyping()
    {
        /*
         * Preparation:
         * set activeGatherTask.resource to the resource requested
         * set gatherPhase = SEEKINGRESOURCE
         * 
         * 
         * 
         * GatherResource coroutine loop should use the 4 modes from gatherPhase to be run while gatherTaskIsActive:
         * 
         * 1. SEEKINGRESOURCE
         * 
         * confirm chosen resource is not null.  If at any point it is null during this phase, the closest resource of same type should be set to new chosen resource
         * if carryLimit has not been reached - move to chosen resource, while continuing to check it is not null.  If it is, run above logic and loop back to here
         * if closest resource turns null at any point, all resources on the map have been depleted, and the coroutine can be exit by setting gatherTaskIsActive to false and using yield break.
         * 
         * once in range of chosen resource, set gatherPhase to GATHERING:
         * 2. GATHERING
         * 
         * stop moving
         * look at resource
         * while carry limit has not been reached:
         * gather from it once
         * if resources still remain on this resource, continue to gather from it
         * if resources on this are gone (resource = null), set chosen resource to closest resource of this type and set mode back to SEEKINGRESOURCE.
         * if closest resource type is null (no more resources of this type available) set bool 'resourcesGoneButStillCarrying' to true
         * 
         * once carry limit has been reached (or resourcesGoneButStillCarrying is true), set gatherPhase to MOVETODEPOT:
         * 3. MOVETODEPOT 
         * 
         * Find closest depot, and move to it
         * while not in range of closest depot:
         * move to depot.  If at any point during this phase the depot is null (player or enemy destroyed it), search for closest depot again and continue moving
         * 
         * 
         * Once in range of depot, set gatherPhase to DEPOSITING:
         * 4. DEPOSITING
         * 
         * stop moving
         * look at depot
         * deposit resources into the chosen depot
         * 
         * if resourcesGoneButStillCarrying is true, exit from the coroutine by setting gatherTaskIsActive to false and using yield break.
         * 
         * if this point is reached, loop back to the beginning phase - (line 106), SEEKINGRESOURCE
         */
    }

    public IEnumerator PrepareGathering(Resource resource)
    {
        ResourceTypes tempResourceType = resource.resourceType;
        activeGatherTask.resource = resource;

        bool isAtResource = false;
        while (!isAtResource)
        {
            yield return new WaitForEndOfFrame();
            if (!agent.pathPending)
            {
                isAtResource = IsAtDest();
            }
        }

        if (resource == null)
            resource = GetClosestResource(tempResourceType);

        StopAgentMovement();

        transform.LookAt(resource.transform);

        BeginGathering(resource);        
    }

    public void BeginGathering(Resource resource)
    {
        resource.unitsInteracting.Add(this);
        gatherTaskIsActive = true;

        activeGatherTask.depot = GetClosestDepot(resource);

        gatherTaskCoroutine = StartCoroutine(GatherResource());
    }

    IEnumerator GatherResource()
    {
        while (gatherTaskIsActive)
        {
            if (activeGatherTask.resource.resourcesRemaining == 0) //someone else has already depleted these resources, but this unit is already en route
            {
                activeGatherTask.resource = GetClosestResource(activeGatherTask.resource.resourceType);

                um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, activeGatherTask.depot);

                bool isAtResource = false;
                while (!isAtResource)
                {
                    yield return new WaitForEndOfFrame();
                    if (!agent.pathPending)
                    {
                        if (activeGatherTask.resource == null && resourcesHolding == 0)
                        {
                            activeGatherTask.resource = GetClosestResource(activeGatherTask.resource.resourceType);
                            um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, activeGatherTask.depot);
                        }
                        isAtResource = IsAtDest();
                    }
                }
                StopAgentMovement();
            }

            while (resourcesHolding < GetCarryLimit() && activeGatherTask.resource.resourcesRemaining > 0)
            {
                yield return new WaitForSeconds(1);
                AddResourcetoUnit();
                //Debug.Log(gameObject.name + " carrying " + activeGatherTask.resource.resourceType + " - " + resourcesHolding + "/" + GetCarryLimit());
                StartCoroutine(gm.ShowResourceGatherUX(this.gameObject, activeGatherTask.resource.resourceType, 1, true));
            }
            
            um.ProcessMoveVillagerUnitInTask(true, this, activeGatherTask.resource, activeGatherTask.depot);

            bool isAtDepot = false;
            while (!isAtDepot)
            {
                yield return new WaitForEndOfFrame();
                if (!agent.pathPending)
                {
                    if (activeGatherTask.resource == null && resourcesHolding == 0)
                    {
                        activeGatherTask.resource = GetClosestResource(activeGatherTask.resource.resourceType);
                        um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, activeGatherTask.depot);
                    }
                    isAtDepot = IsAtDest();
                }
            }
            StopAgentMovement();

            yield return new WaitForSeconds(up.vilResourceDropoffTime); // simulates dropping resource off
            DropResourceOffAtDepot();

            if (activeGatherTask.resource != null) // Check if resource still has any left (object hasn't been destroyed).  If Yes, wait 1 second, and move unit back to gather more.
            {                
                um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, activeGatherTask.depot);
                bool isAtResource = false;
                while (!isAtResource)
                {
                    yield return new WaitForEndOfFrame();
                    if (!agent.pathPending)
                    {
                        if (activeGatherTask.resource == null && resourcesHolding == 0)
                        {
                            activeGatherTask.resource = GetClosestResource(activeGatherTask.resource.resourceType);
                            um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, activeGatherTask.depot);
                        }
                        isAtResource = IsAtDest();
                    }
                }
                StopAgentMovement();
            } else if (activeGatherTask.resource == null) // Ensure resource still has any remaining (object has been destroyed).  If so, continue.  If not, check for closest of same resource with some remaining, and start there
            {
                activeGatherTask.resource = GetClosestResource(activeGatherTask.resource.resourceType);

                if (activeGatherTask.resource == null)
                {
                    gatherTaskIsActive = false;
                }
                else
                {
                    um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, activeGatherTask.depot);

                    bool isAtResource = false;
                    while (!isAtResource)
                    {
                        yield return new WaitForEndOfFrame();
                        if (!agent.pathPending)
                        {
                            if (activeGatherTask.resource == null && resourcesHolding == 0)
                            {
                                activeGatherTask.resource = GetClosestResource(activeGatherTask.resource.resourceType);
                                um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, activeGatherTask.depot);
                            }
                            isAtResource = IsAtDest();
                        } 
                    }
                    StopAgentMovement();
                }
            }            
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

    bool IsAtDest()
    {
        return GetComponent<NavMeshAgent>().remainingDistance <= GetComponent<NavMeshAgent>().stoppingDistance;
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

    Resource GetClosestResource(ResourceTypes resourceType)
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
            if (resourceToCheck.resourceType == resourceType &&
                resourceToCheck.resourcesRemaining > 0)
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

    int GetCarryLimit()
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
