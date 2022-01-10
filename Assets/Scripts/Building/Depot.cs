using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Depot : CompletedBuilding
{
    public int buildingID;

    // Start is called before the first frame update
    void Start()
    {
        building = FindObjectOfType<BuildManager>().buildings[buildingID];

        building.level = 1;
        building.currentDurability = building.maxDurability;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
