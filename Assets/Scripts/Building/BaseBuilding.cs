using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum depotResources
{
    NA,
    WOOD,
    ORE,
    FOOD,
    ALL
}

[System.Serializable]
public class BaseBuilding
{
    public string name;
    public Sprite icon;
    public KeyCode shortcutKey;
    public GameObject blueprintPrefab;
    public GameObject inProgressPrefab;
    public GameObject completePrefab;
    public int woodRequired;
    public int oreRequired;
    public int foodRequired;
    public int goldRequired;
    public int maxDurability;
    public int maxUnitsInteracting;

    public depotResources depotResource;

    public List<BaseAction> actions;

    [HideInInspector] public int currentDurability;
    [HideInInspector] public int level;
}
