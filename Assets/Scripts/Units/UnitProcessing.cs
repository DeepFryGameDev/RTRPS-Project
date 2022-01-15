using UnityEngine;

// Contains calculations and values for unit processing
public class UnitProcessing : MonoBehaviour
{
    [Tooltip("When units level up, their required EXP to next level is calculated by their level * this value")]
    public float toNextLevelFactor;

    [Tooltip("How long it takes for villager to drop off resources at a depot")]
    public float vilResourceDropoffTime;

    [Tooltip("Villager unit's HP is calculated by their stamina value * this value + their willpower value * the below value")]
    public float vilHPFromStaminaFactor;
    [Tooltip("Villager unit's HP is calculated by their stamina value * the above value + their willpower value * this value")]
    public float vilHPFromWillpowerFactor;
    [Tooltip("Villager unit's EP is calculated by their stamina value * this value")]
    public float vilEnergyFromStaminaFactor;
    [Tooltip("Villager unit's carry limit for wood is calculated by their strength * this value")]
    public float vilWoodCarryLimitFromStrengthFactor;
    [Tooltip("Villager unit's carry limit for ore is calculated by their stamina * this value")]
    public float vilOreCarryLimitFromStaminaFactor;
    [Tooltip("Villager unit's carry limit for wood is calculated by their intelligence * this value")]
    public float vilFoodCarryLimitFromIntelligenceFactor;
}
