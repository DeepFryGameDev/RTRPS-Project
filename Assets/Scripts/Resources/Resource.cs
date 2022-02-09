using System.Collections.Generic;
using UnityEngine;

// resource type that can be set to a resource object
public enum ResourceTypes
{
    WOOD,
    ORE,
    FOOD
}

// This script contains parameters for the set resource as well as handles the shrinking of gameobject scale when the number of resources is lowered
public class Resource : MonoBehaviour
{
    [Tooltip("Name of the actual object. ie 'Sequoia Tree'")]
    public new string name;
    [Tooltip("Icon to be displayed in UI when selecting the object")]
    public Sprite icon;
    [Tooltip("If object can be shrunk when gathering from it.  This is for UX purposes")]
    public bool canShrink;
    [Tooltip("Type of resource gathered from this object")]
    public ResourceTypes resourceType;
    [Tooltip("Total number of resources that can be gathered from this object")]
    public int totalResources;
    [Tooltip("How many units can be gathering from this resource")]
    public int maxUnitsGathering;

    // Units that are currently interacting with the object.  When they are moving from the object to the depot, they are removed from the list until they are actually interacting again.
    [HideInInspector] public List<VillagerUnit> unitsInteracting; 

    // Number of resources that are remaining that can still be gathered from the object
    [HideInInspector] public int resourcesRemaining;

    // Default scale of the object to be used for shrinking
    float defaultScale;

    void Start()
    {
        resourcesRemaining = totalResources; // initializes remaining resources to the total number of resources

        defaultScale = transform.localScale.x; // sets default scale to the current scale of the object

        if (GetComponent<Outline>())
            GetComponent<Outline>().enabled = false; // turns off any outline enabled before gameplay starts
    }

    void Update()
    {
        if (canShrink) // This provides user feedback for the number of wood/ore/food remaining on this resource, in relation to its total resources, by lowering it's scale and creating a "shrinking" effect
        {
            float newScale = defaultScale * ((float)resourcesRemaining / (float)totalResources);

            transform.localScale = new Vector3(newScale, newScale, newScale);

            if (totalResources != 0 && resourcesRemaining == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
