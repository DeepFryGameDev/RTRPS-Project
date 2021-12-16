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
}
