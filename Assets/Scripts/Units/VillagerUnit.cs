using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum villagerClasses
{
    VILLAGER,
    HARVESTER,
    BUILDER,
    FARMER,
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
            // Ensure resource still has any remaining.  If so, continue.  If not, set gatherTaskIsActive = false and skip this.
            while (resourcesHolding < 10 && activeGatherTask.resource.resourcesRemaining > 0)
            {
                yield return new WaitForSeconds(1);
                AddResourcetoUnit();
            }

            um.ProcessMoveVillagerUnitInTask(true, this, activeGatherTask.resource, activeGatherTask.depot);

            bool isAtDepot = false;
            while (!isAtDepot)
            {
                isAtDepot = IsAtDest();
                yield return new WaitForEndOfFrame();
            }

            DropResourceOffAtDepot();

            if (activeGatherTask.resource.resourcesRemaining > 0) // Check if resource still has any left.  If Yes, wait 1 second, and move unit back to gather more.
            {
                yield return new WaitForSeconds(1); // simulates dropping resource off
                um.ProcessMoveVillagerUnitInTask(false, this, activeGatherTask.resource, activeGatherTask.depot);

                bool isAtResource = false;
                while (!isAtResource)
                {
                    isAtResource = IsAtDest();
                    yield return new WaitForEndOfFrame();
                }
            } else
            {
                gatherTaskIsActive = false;
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
        GameObject[] depots;
        depots = GameObject.FindGameObjectsWithTag("Depot");

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 tempPos = transform.position;

        foreach (GameObject depot in depots)
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
        } else if (villagerClass.Equals(villagerClasses.BUILDER))
        {
            SetStrength(3);
            SetStamina(2);
            SetAgility(1);
            SetLuck(2);
            SetIntelligence(1);
            SetWillpower(2);
            SetMovement(2);
        } else if (villagerClass.Equals(villagerClasses.FARMER))
        {
            SetStrength(1);
            SetStamina(3);
            SetAgility(2);
            SetLuck(1);
            SetIntelligence(2);
            SetWillpower(2);
            SetMovement(2);
        }

    }

    void SetMaxHP()
    {
        this.SetMaxHP(Mathf.RoundToInt((GetStamina() * up.vilHPFromStaminaFactor) + GetWillpower() * up.vilHPFromWillpowerFactor));
    }

    void SetMaxMP()
    {
        this.SetMaxMP(Mathf.RoundToInt(GetStamina() * up.vilEnergyFromStaminaFactor));
    }
}
