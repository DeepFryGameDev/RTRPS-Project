using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceTypes
{
    WOOD,
    ORE,
    FOOD
}

public class Resource : MonoBehaviour
{
    public float interactionBounds;
    public ResourceTypes resourceType;
    public int totalResources;

    [HideInInspector] public List<VillagerUnit> unitsInteracting;

    [ReadOnly] public int resourcesRemaining;

    // Start is called before the first frame update
    void Start()
    {
        resourcesRemaining = totalResources;
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
