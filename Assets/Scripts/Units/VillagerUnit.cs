using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        up = FindObjectOfType<UnitProcessing>();
        SetUnitProcessingVars();
    }

    public void BeginGathering(Resource resource)
    {
        resource.unitsInteracting.Add(this);
        Debug.Log("Gathering " + resource.resourceType);
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
        this.SetMaxHP(Mathf.RoundToInt((GetStamina() * up.villagerHPFromStaminaFactor) + GetWillpower() * up.villagerHPFromWillpowerFactor));
    }

    void SetMaxMP()
    {
        this.SetMaxMP(Mathf.RoundToInt(GetStamina() * up.villagerEnergyFromStaminaFactor));
    }
}
