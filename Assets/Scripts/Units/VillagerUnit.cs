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

    UnitMovement um;

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
        SetUnitProcessingVars();

        UnitAwake();
    }

    public void BeginGathering(Resource resource)
    {
        resource.unitsInteracting.Add(this);
        gatherTaskIsActive = true;

        activeGatherTask.resource = resource;
        activeGatherTask.depot = GetClosestDepot(resource);

        gatherTaskCoroutine = StartCoroutine(GatherResource());
    }

    public IEnumerator GatherResource()
    {
        while (gatherTaskIsActive)
        {
            while (resourcesHolding < GetCarryLimit() && activeGatherTask.resource.resourcesRemaining > 0)
            {
                yield return new WaitForSeconds(1);
                AddResourcetoUnit();
                Debug.Log(gameObject.name + " carrying " + activeGatherTask.resource.resourceType + " - " + resourcesHolding + "/" + GetCarryLimit());
            }

            Debug.Log(gameObject.name + " en route to depot - " + activeGatherTask.depot.gameObject.name);
            um.ProcessMoveVillagerUnitInTask(true, this, activeGatherTask.resource, activeGatherTask.depot);

            bool isAtDepot = false;
            while (!isAtDepot)
            {
                isAtDepot = IsAtDest();
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(up.vilResourceDropoffTime); // simulates dropping resource off
            DropResourceOffAtDepot();

            Debug.Log(gameObject.name + " dropped off resources at depot - " + activeGatherTask.depot.gameObject.name);

            if (activeGatherTask.resource.resourcesRemaining > 0) // Check if resource still has any left.  If Yes, wait 1 second, and move unit back to gather more.
            {
                Debug.Log(gameObject.name + " going back for more - " + activeGatherTask.resource.gameObject.name);
                
                um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, activeGatherTask.depot);
                bool isAtResource = false;
                while (!isAtResource)
                {
                    isAtResource = IsAtDest();
                    yield return new WaitForEndOfFrame();
                }
            } else if (activeGatherTask.resource.resourcesRemaining == 0) // Ensure resource still has any remaining.  If so, continue.  If not, check for closest of same resource with some remaining, and start there
            {
                activeGatherTask.resource = GetClosestResource(activeGatherTask.resource);

                if (activeGatherTask.resource == null)
                {
                    Debug.Log(gameObject.name + " - No more resources remaining at activeGatherTask.resource, and no closest resource of same type found.  Stopping gather task");
                    gatherTaskIsActive = false;
                }
                else
                {
                    Debug.Log(gameObject.name + " - No more resources remaining at activeGatherTask.resource, going to closest resource of same type - " + activeGatherTask.resource.gameObject.name);

                    um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, activeGatherTask.depot);

                    bool isAtResource = false;
                    while (!isAtResource)
                    {
                        isAtResource = IsAtDest();
                        yield return new WaitForEndOfFrame();
                    }
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

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 tempPos = transform.position;

        foreach (GameObject depot in objs)
        {
            Vector3 diff = depot.transform.position - tempPos;
            if (diff.sqrMagnitude < distance)
            {
                closest = depot;
                distance = diff.sqrMagnitude;
            }
        }

        return closest.GetComponent<Depot>();
    }

    Resource GetClosestResource(Resource resource)
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
            if (resourceToCheck.resourceType == activeGatherTask.resource.resourceType &&
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
