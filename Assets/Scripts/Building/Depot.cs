using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum depotTypes
{
    ALL,
    WOOD,
    ORE,
    FOOD
}
public class Depot : MonoBehaviour
{
    public float interactionBounds;
    public depotTypes depotType;

    [HideInInspector] public BaseBuilding building;

    // Start is called before the first frame update
    void Start()
    {
        building.currentDurability = building.maxDurability;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactionBounds);
    }
}
