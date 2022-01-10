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
    public new string name;
    public Sprite icon;
    public bool canShrink;
    public float interactionBounds;
    public ResourceTypes resourceType;
    public int totalResources;
    [Tooltip("How many units can be gathering from this resource")]
    public int maxUnitsGathering;

    [ReadOnly] public List<VillagerUnit> unitsInteracting;

    [ReadOnly] public int resourcesRemaining;

    float defaultScale;

    // Start is called before the first frame update
    void Start()
    {
        resourcesRemaining = totalResources;
        defaultScale = transform.localScale.x;

        if (GetComponent<Outline>())
            GetComponent<Outline>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (canShrink)
        {
            float newScale = defaultScale * ((float)resourcesRemaining / (float)totalResources);

            transform.localScale = new Vector3(newScale, newScale, newScale);

            if (totalResources != 0 && resourcesRemaining == 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactionBounds);
    }
}
