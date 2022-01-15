using System.Collections.Generic;
using UnityEngine;

// Handles detecting when a unit collides with a resource or building for interaction
public class InteractionCollision : MonoBehaviour
{
    [HideInInspector] public List<Unit> unitsInteracting = new List<Unit>(); // list of units that are currently touching the collider of this object

    private void OnTriggerEnter(Collider other) // Adds unit to units interacting
    {
        if (other.CompareTag("Unit") && other.GetComponent<VillagerUnit>() && (other.GetComponent<VillagerUnit>().buildTaskIsActive || other.GetComponent<VillagerUnit>().gatherTaskIsActive))
        {
            //Debug.Log("Adding " + other.gameObject.name + " to list");
            unitsInteracting.Add(other.GetComponent<Unit>());
        }        
    }

    private void OnTriggerExit(Collider other) // Removes unit from units interacting
    {
        if (other.CompareTag("Unit") && other.GetComponent<VillagerUnit>())
        {
            //Debug.Log("Removing " + other.gameObject.name + " from list");
            unitsInteracting.Remove(other.GetComponent<Unit>());
        }
    }
}
