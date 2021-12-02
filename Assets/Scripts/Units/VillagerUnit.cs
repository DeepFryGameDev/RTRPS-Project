using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum villagerClasses
{
    VILLAGER,
    HARVESTER,
    BUILDER,
    FARMER
}

public class VillagerUnit : Unit
{
    public villagerClasses villagerClass;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TooltipProcessing();
    }    
}
