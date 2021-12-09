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

    public List<VillagerUnit> unitsInteracting;

    [HideInInspector] public int resourcesRemaining;

    // Start is called before the first frame update
    void Start()
    {
        
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
