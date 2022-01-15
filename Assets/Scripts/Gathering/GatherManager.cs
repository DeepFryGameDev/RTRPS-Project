using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// phases of the gather process being used by a villager unit
public enum GatherPhases
{
    SEEKINGRESOURCE,
    GATHERING,
    MOVETODEPOT,
    DEPOSITING
}

public class GatherManager : MonoBehaviour
{
    [Tooltip("Minimum amount of time to gather a resource")]
    public float minGatherTime;
    [Tooltip("Maximum amount of time to gather a resource")]
    public float maxGatherTime;
    [Tooltip("Decreases gather time of wood by this value * unit's strength (+ Willpower")]
    public float gatherWoodTimeStrengthFactor;
    [Tooltip("Decreases gather time of wood by this value * unit's willpower (+ Strength")]
    public float gatherWoodTimeWillpowerFactor;
    [Tooltip("Decreases gather time of ore by this value * unit's strength (+ Stamina")]
    public float gatherOreTimeStrengthFactor;
    [Tooltip("Decreases gather time of ore by this value * unit's stamina (+ Strength")]
    public float gatherOreTimeStaminaFactor;
    [Tooltip("Decreases gather time of food by this value * unit's intelligence (+ Willpower")]
    public float gatherFoodTimeIntelligenceFactor;
    [Tooltip("Decreases gather time of food by this value * unit's willpower (+ Intelligence")]
    public float gatherFoodTimeWillpowerFactor;

    UIProcessing uip; // used for highlighting resources at the start of gather process, determining if gather action has been clicked, and returning selected units
    UIPrefabManager uipm; // used to return resource icons
    UnitMovement um; // used to call StartGathering method

    [HideInInspector] public bool gatherActionClicked;
    [HideInInspector] public bool resourceClickedInAction;

    private void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        uipm = FindObjectOfType<UIPrefabManager>();
        um = FindObjectOfType<UnitMovement>();
    }

    private void Update()
    {
        // this processes the gather resources method if player chooses 'gather' action in the action buttons
        if (gatherActionClicked) // if gather action was clicked by player
        {
            ProcessActionClicked(); // starts gathering process if a resource is clicked
        }

        CheckIfActionNoLongerClicked(); // disables potential for gathering process if a cancel command is input (player hits escape)
    }

    // Displays user feedback when a resource has been gathered/deposited - resource icon will be displayed above the object (unit/depot) and slowly moves upward along y position and fades out
    public void ShowResourceGatherUX(GameObject source, ResourceTypes resourceType, int gathered, bool plus)
    {
        // set icon
        Sprite icon = null;

        switch (resourceType)
        {
            case ResourceTypes.WOOD:
                icon = uipm.woodResourceIcon;
                break;
            case ResourceTypes.ORE:
                icon = uipm.oreResourceIcon;
                break;
            case ResourceTypes.FOOD:
                icon = uipm.foodResourceIcon;
                break;
        }

        string sign;
        if (plus)
        {
            sign = "+";
        }
        else
        {
            sign = "-";
        }


        // prepare GatherUX
        GameObject gatherUX = uipm.gatherBuildUX;
        gatherUX.transform.Find("UXText").GetComponent<TMP_Text>().text = sign + gathered.ToString();
        gatherUX.transform.Find("UXIcon").GetComponent<Image>().sprite = icon;

        StartCoroutine(uip.DisplayUX(source, gatherUX, false));
    }

    // processes gather action if gather action has been clicked and a resource is hovered
    void ProcessActionClicked()
    {        
        CheckIfResourceIsHovered();
    }

    // cancels gather action if player presses escape after the action has been clicked
    void CheckIfActionNoLongerClicked()
    {
        if (gatherActionClicked && Input.GetKeyDown(KeyCode.Escape))
        {
            uip.actionButtonClicked = false;
        }
    }

    // If resource is being hovered by mouse cursor, a further check will take place if the selected unit has the capability to gather the resource chosen
    void CheckIfResourceIsHovered()
    {
        // loop through all resources and check if they are highlighted.  if yes, remove highlight
        Resource[] allResources = FindObjectsOfType<Resource>();
        foreach (Resource res in allResources)
        {
            if (res.GetComponent<Outline>() && res.GetComponent<Outline>().enabled == true)
            {
                uip.HighlightResourceOrBuilding(res.GetComponent<Outline>(), false);
            }
        }

        RaycastHit[] hits;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        hits = Physics.RaycastAll(ray, 1000);

        // Checking if mouse cursor is over a resource
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("Resource"))
            {
                CheckIfSelectedUnitCanGather(hit);
            }
        }
    }

    // If selected unit has capability to gather the chosen resource, begin gather process is run
    private void CheckIfSelectedUnitCanGather(RaycastHit hit)
    {
        //check for each resource type/unit type
        // check if should be able to gather
        bool canGather = false;

        foreach (Unit unit in uip.selectedUnits)
        {
            if (hit.transform.GetComponent<Resource>().resourceType == ResourceTypes.WOOD &&
                (((VillagerUnit)unit).villagerClass == VillagerClasses.VILLAGER ||
                ((VillagerUnit)unit).villagerClass == VillagerClasses.GATHERER ||
                ((VillagerUnit)unit).villagerClass == VillagerClasses.LUMBERJACK))
            {
                canGather = true;
            }

            if (hit.transform.GetComponent<Resource>().resourceType == ResourceTypes.ORE &&
                (((VillagerUnit)unit).villagerClass == VillagerClasses.VILLAGER ||
                ((VillagerUnit)unit).villagerClass == VillagerClasses.GATHERER ||
                ((VillagerUnit)unit).villagerClass == VillagerClasses.MINER))
            {
                canGather = true;
            }

            if (hit.transform.GetComponent<Resource>().resourceType == ResourceTypes.FOOD &&
                (((VillagerUnit)unit).villagerClass == VillagerClasses.VILLAGER ||
                ((VillagerUnit)unit).villagerClass == VillagerClasses.GATHERER ||
                ((VillagerUnit)unit).villagerClass == VillagerClasses.FARMER))
            {
                canGather = true;
            }
        }

        if (canGather)
        {
            BeginGatherProcess(hit);
        }
    }

    // Highlights the resource upon mouseover and begins gather process if it is clicked
    private void BeginGatherProcess(RaycastHit hit)
    {
        uip.HighlightResourceOrBuilding(hit.transform.GetComponent<Outline>(), true);        

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            resourceClickedInAction = true;
            uip.actionButtonClicked = false;
            um.StartGathering(hit.transform.GetComponent<Resource>());
        }
    }


}
