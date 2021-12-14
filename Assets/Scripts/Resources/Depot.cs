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
