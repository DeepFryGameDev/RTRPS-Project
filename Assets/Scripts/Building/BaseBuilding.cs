using System.Collections.Generic;
using UnityEngine;

public enum depotResources // used to determine the type of resource that the depot accepts
{
    NA,
    WOOD,
    ORE,
    FOOD,
    ALL
}

// this script contains the parameters of all buildings
[System.Serializable]
public class BaseBuilding
{
    [Tooltip("Name of the building")]
    public string name;
    [Tooltip("Building's icon to be displayed in the UI and on action buttons")]
    public Sprite icon;
    [Tooltip("Shortcut key to press for action to be performed")]
    public KeyCode shortcutKey;
    [Tooltip("Set to blueprint prefab to be used when placing the building to be built")]
    public GameObject blueprintPrefab;
    [Tooltip("Set to in progress prefab to be used during the process of building the completed building")]
    public GameObject inProgressPrefab;
    [Tooltip("Set to the fully built building prefab")]
    public GameObject completePrefab;
    [Tooltip("Number of wood required to create the building")]
    public int woodRequired;
    [Tooltip("Number of ore required to create the building")]
    public int oreRequired;
    [Tooltip("Number of food required to create the building")]
    public int foodRequired;
    [Tooltip("Number of gold required to create the building")]
    public int goldRequired;
    [Tooltip("Max durability (health) of the completed building")]
    public int maxDurability;
    [Tooltip("This value impacts the amount of time units will take to contribute to personal progress.  Build time is directly multiplied by this value.")]
    [Range(.1f ,10)] public float buildDifficulty;
    [Tooltip("Max number of units that can simultaneously help to build the building in progress")]
    public int maxUnitsInteracting;

    [Tooltip("If it's a depot, the type of resource that the depot will accept. Set to NA if the building is not a depot")]
    public depotResources depotResource;

    [Tooltip("List of actions by ID the building can perform")]
    public List<int> buildingActions;

    [HideInInspector] public int currentDurability; // used to determine health of the building.  When this is 0, the building should be destroyed

    [HideInInspector] public int level; // used to determine stats of the building and which actions can be used
}
