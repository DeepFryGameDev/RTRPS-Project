using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProcessing : MonoBehaviour
{
    [Tooltip("When units level up, their required EXP to next level is calculated by their level * this value")]
    public float toNextLevelFactor;

    [Tooltip("Villager unit's HP is calculated by their stamina value * this value + their willpower value * the below value")]
    public float villagerHPFromStaminaFactor;
    [Tooltip("Villager unit's HP is calculated by their stamina value * the above value + their willpower value * this value")]
    public float villagerHPFromWillpowerFactor;
    [Tooltip("Villager unit's EP is calculated by their stamina value * this value")]
    public float villagerEnergyFromStaminaFactor;
}
