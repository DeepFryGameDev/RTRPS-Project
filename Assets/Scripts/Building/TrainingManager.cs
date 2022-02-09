using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    [Tooltip("Minimum amount of time (in seconds) that a building will spend training a new unit, before difficulty.")]
    public float minBaseTrainTime;
    [Tooltip("Maximum amount of time (in seconds) that a building will spend training a new unit, before difficulty.")]
    public float maxBaseTrainTime;
    [Tooltip("Value multiplied by the base train time (calculated based upon biome the building was placed on.  The higher the value, the bigger the difference in biome affecting training strength.)")]
    public float baseTrainTimeFactor;

    [Tooltip("Number of maximum amount of training actions that can be queued")]
    public int maxTrainingActions = 10;

    [Tooltip("Maximum number of vector 3 units a unit can move after being trained.")]
    public float maxTrainWalkPointDistance = 3;

    public Transform unitsParent;

    public BaseTrainAction GetTrainActionFromBuildingAction(BaseBuildingAction bba) 
    {
        BaseTrainAction bta = new BaseTrainAction
        {
            name = bba.name,
            icon = bba.icon,
            shortcutKey = bba.shortcutKey,
            levelRequired = bba.levelRequired,

            actionScript = bba.actionScript,
            actionType = bba.actionType,

            trainBeachStrength = bba.trainBeachStrength,
            trainPlainsStrength = bba.trainPlainsStrength,

            trainDifficulty = bba.trainDifficulty,

            trainedUnitPrefab = bba.trainedUnitPrefab,

            woodRequired = bba.woodRequired,
            oreRequired = bba.oreRequired,
            foodRequired = bba.foodRequired,
            goldRequired = bba.goldRequired
        };

        return bta;
    }
}
