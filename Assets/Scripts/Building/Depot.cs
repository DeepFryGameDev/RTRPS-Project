public class Depot : CompletedBuilding
{
    public int buildingID; // used to retrieve building's parameters

    void Start()
    {
        building = FindObjectOfType<BuildManager>().buildings[buildingID]; // sets building to index of BuildManager.buildings

        // I wanted to run the below in Awake() in CompletedBuilding, but for some reason the variables are not being set.  This should be look into later.
        building.level = 1; // defaults building to level 1
        building.currentDurability = building.maxDurability; // defaults current durability to max durability
    }
}
