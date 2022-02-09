using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingActionTypes
{
    TRAINUNIT,
    UPGRADE
}

[System.Serializable]
public class BaseBuildingAction : BaseAction
{
    [Tooltip("Type of action")]
    public BuildingActionTypes actionType;

    [Tooltip("Number of wood required to process the action")]
    public int woodRequired;
    [Tooltip("Number of ore required to process the action")]
    public int oreRequired;
    [Tooltip("Number of food required to process the action")]
    public int foodRequired;
    [Tooltip("Number of gold required to process the action")]
    public int goldRequired;

    [Tooltip("This value impacts the amount of time it takes to train the new unit.  Training time is directly multiplied by this value.  This value is ignored if action type is not TRAINUNIT.  1 is medium difficulty")]
    [Range(.1f, 5)] public float trainDifficulty;
    [Tooltip("Higher value determines shorter training time on plains biome")]
    [Range(.1f, 10)] public float trainPlainsStrength;
    [Tooltip("Higher value determines shorter training time on beach biome")]
    [Range(.1f, 10)] public float trainBeachStrength;

    [Tooltip("Prefab of unit that will be spawned when trained")]
    public GameObject trainedUnitPrefab;
}
