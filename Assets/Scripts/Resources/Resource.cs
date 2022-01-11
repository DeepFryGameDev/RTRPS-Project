using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* icon attribution 
 * Rock icon: Icons made by "https://www.flaticon.com/authors/icongeek26" - Icongeek26 from "https://www.flaticon.com/" 
 * Tree icon: Icons made by "https://www.freepik.com" - Freepik from "https://www.flaticon.com/"
 * Farm icon: Icons made by "https://www.flaticon.com/authors/photo3idea-studio" - photo3idea_studio from "https://www.flaticon.com/"
 */

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
}
