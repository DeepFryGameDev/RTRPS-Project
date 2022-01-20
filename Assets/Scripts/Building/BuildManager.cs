using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// used by VillagerUnit to process phases of working on the build
public enum BuildPhases
{
    MOVETOBUILDING,
    PROCESSBUILD
}

public class BuildManager : MonoBehaviour
{
    [Tooltip("Minimum amount of time (in seconds) that a unit will spend increasing their personal progress to the build.")]
    public float minBuildTime;
    [Tooltip("Maximum amount of time (in seconds) that a unit will spend increasing their personal progress to the build.")]
    public float maxBuildTime;
    [Tooltip("Personal progress build time is impacted by the unit's agility * this value (+ Intelligence and Willpower factors)")]
    public float buildTimeAgilityFactor;
    [Tooltip("Personal progress build time is impacted by the unit's intelligence * this value (+ Agility and Willpower factors)")]
    public float buildTimeIntelligenceFactor;
    [Tooltip("Personal progress build time is impacted by the unit's willpower * this value (+ Intelligence and Agility factors)")]
    public float buildTimeWillpowerFactor;

    [Tooltip("Minimum amount a unit's personal progress can be increased by")]
    public float minBuildPerProg;
    [Tooltip("Maximum amount a unit's personal progress can be increased by")]
    public float maxBuildPerProg;
    [Tooltip("Below personal progress calculations are added and then multiplied by this factor to determine personal progress")]
    public float buildPerProgFactor;
    [Tooltip("Personal progress build value is impacted by the unit's strength * this value (+ Intelligence factor) and then multiplied by buildPerProgFactor")]
    public float buildPerProgStrengthFactor;
    [Tooltip("Personal progress build value is impacted by the unit's intelligence * this value (+ Strength factor) and then multiplied by buildPerProgFactor")]
    public float buildPerProgIntelligenceFactor;

    [Tooltip("Minimum amount a building's total progress can be increased by one unit")]
    public float minBuildTotProg;
    [Tooltip("Maximum amount a building's total progress can be increased by one unit")]
    public float maxBuildTotProg;
    [Tooltip("Below total progress calculations are added and then multiplied by this factor to determine personal progress")]
    public float buildTotProgFactor;
    [Tooltip("Total progress build value is impacted by the unit's willpower * this value (+ Stamina factor) and then multiplied by buildTotProgFactor")]
    public float buildTotProgWillpowerFactor;
    [Tooltip("Total progress build value is impacted by the unit's stamina * this value (+ Willpower factor) and then multiplied by buildTotProgFactor")]
    public float buildTotProgStaminaFactor;

    [Tooltip("List of all buildings that can be built in the game")]
    public List<BaseBuilding> buildings = new List<BaseBuilding>();

    [HideInInspector] public BaseBuilding chosenBuilding; // used by buildingAction to process action for building chosen by player
    [HideInInspector] public bool blueprintOpen, blueprintClosed; // used when blueprint is still on the field or when it has been cancelled/placed
    [HideInInspector] public bool buildActionClicked; // used when 'build' action has been chosen (before building action)
    [HideInInspector] public bool buildingActionClicked; // used when 'building' action has been chosen (after 'build' action)

    UIProcessing uip; // used to get selected units as well as check if build action has been clicked and change actionButtonClicked
    UIPrefabManager uipm; // used to get various UI components to be adjusted
    AnimationManager am;
    List<BaseBuilding> availableBuildings = new List<BaseBuilding>(); // list of available buildings that can be built
    GameObject actionSpacer; // Grid Layout Group for the action buttons to be placed

    float defaultCanvasHeight; // set to the default height of the build action panel
    bool panelShown; // used to determine when build panel is available

    private void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        am = FindObjectOfType<AnimationManager>();
        uipm = FindObjectOfType<UIPrefabManager>();

        actionSpacer = uipm.buildActionPanel.transform.Find("BuildSpacer").gameObject; // sets Grid Layout Group for action buttons to be placed

        defaultCanvasHeight = uipm.buildActionPanel.GetComponent<RectTransform>().rect.height; // sets default canvas height based on canvas height when game is started

        ShowBuildPanel(false);
    }

    private void Update()
    {      
        if (buildActionClicked)
        {
            ProcessActionClicked(); // process actions if a build action has been chosen
        }

        CheckIfActionNoLongerClicked(); // process cancel actions if player cancels them
    }

    public void StartBuildingProcess(GameObject newBuild)
    {
        foreach (Unit unit in uip.selectedUnits)
        {
            if (uip.GetVillagerUnit(unit) &&
                (uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.VILLAGER || uip.GetVillagerUnit(unit).villagerClass == VillagerClasses.BUILDER)
                )
            {
                if (uip.GetVillagerUnit(unit).gatherTaskIsActive)
                {
                    uip.GetVillagerUnit(unit).gatherTaskIsActive = false;
                    uip.GetVillagerUnit(unit).StopGathering();
                    uip.GetVillagerUnit(unit).CompleteGatheringTask();
                }
                if (uip.GetVillagerUnit(unit).buildTaskIsActive)
                {
                    uip.GetVillagerUnit(unit).buildTaskIsActive = false;
                    uip.GetVillagerUnit(unit).StopBuilding();
                    uip.GetVillagerUnit(unit).CompleteBuildTask();
                }

                uip.GetVillagerUnit(unit).PrepareBuilding(newBuild);
            }
        }
    }

    public void FinishBuildingProcess(BuildInProgress bip)
    {
        if (!bip.destroyed)
        {
            // set to destroyed
            bip.destroyed = true;

            // Instantiate bip.building.completed building
            GameObject newBuilding = Instantiate(bip.building.completePrefab, bip.transform.position, bip.transform.rotation, transform);

            // show UX feedback for completion
            ShowBuildProgressUX(newBuilding, false, 100);

            // Destroy bip gameobject
            Destroy(bip.gameObject);
        }
    }

    void ProcessActionClicked()
    {
        if (!panelShown)
        {
            ShowBuildPanel(true); // shows build panel with available buildings to select
        }

        if (buildingActionClicked && blueprintClosed) // if a building action has been selected and blueprint has been closed or cancelled
        {
            blueprintClosed = false;
            blueprintOpen = false;
            buildingActionClicked = false;
            buildActionClicked = false;

            uip.actionButtonClicked = false;

            ShowBuildPanel(false);
        }

        if (buildingActionClicked && !blueprintOpen) // if a building action has been selected and blueprint is not yet on the field
        {
            blueprintOpen = true;
            GameObject blueprint = Instantiate(chosenBuilding.blueprintPrefab, transform);
            blueprint.GetComponent<Blueprint>().building = chosenBuilding;
        }
    }

    void CheckIfActionNoLongerClicked()
    {
        if (buildingActionClicked && Input.GetKeyDown(KeyCode.Escape)) // cancels from placing a building (in blueprint mode)
        {
            buildingActionClicked = false;

        } else if (buildActionClicked && Input.GetKeyDown(KeyCode.Escape)) // cancels building action selection
        {
            uip.actionButtonClicked = false;
            //buildActionClicked = false;
            panelShown = false;
            ShowBuildPanel(false);
        }
    }

    void ShowBuildPanel(bool show)
    {
       if (show)
        {
            SetAvailableBuildings(); // generate list of available buildings to place

            SetActionButtons(); // set the action buttons for each available building

            // adjust height of panel based on available buildings
            int canvasHeightFactor = Mathf.FloorToInt(availableBuildings.Count / 3);
            float adjHeight = defaultCanvasHeight + (uip.buildingCanvasButtonAdjustment * (float)canvasHeightFactor);

            RectTransform rt = uipm.buildActionPanel.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.rect.width, adjHeight);

            // show the panel
            am.ProcessOpenAnim(uipm.buildActionPanel, true);

            panelShown = true;
        } else
        {
            am.ProcessOpenAnim(uipm.buildActionPanel, false);

            panelShown = false;
        }

        //uipm.ShowUIObject(uipm.buildActionPanel, show); // display/hide the canvas
    }

    void SetActionButtons()
    {
        // clear any old action buttons
        foreach (Transform child in actionSpacer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (BaseBuilding building in availableBuildings)
        {
            // prepare action button
            GameObject buildActionGO = GameObject.Instantiate(uipm.actionButton) as GameObject;
            buildActionGO.transform.SetParent(actionSpacer.transform, false);

            // Set Icon
            buildActionGO.transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>().sprite = building.icon;

            // Set Name
            buildActionGO.transform.Find("SkillName").GetComponent<TMP_Text>().text = building.name;

            // Set Shortcut
            buildActionGO.transform.Find("ShortcutKeyFrame/ShortcutKey").GetComponent<TMP_Text>().text = building.shortcutKey.ToString();

            // Set Action
            BuildingAction ba = buildActionGO.AddComponent(typeof(BuildingAction)) as BuildingAction;
            ba.building = building;

            // Set Unit
            ba.unit = uip.selectedUnits[0];
        }
    }

    // set buildings to be used in action panel - this will be updated to accomodate the chosen building and building level
    void SetAvailableBuildings()
    {
        availableBuildings.Clear();

        //for now, just add the test building
        availableBuildings.Add(buildings[0]);
        //availableBuildings.Add(buildings[1]);
        //availableBuildings.Add(buildings[2]);
        //availableBuildings.Add(buildings[3]);
    }

    // Displays user feedback when personal or total progress to the building has been made - progress icon will be displayed above the object (unit/building) and slowly moves upward along y position and fades out
    public void ShowBuildProgressUX(GameObject source, bool personalProgress, float progress)
    {
        // prepare buildUX
        GameObject buildUX = uipm.gatherBuildUX;

        if (personalProgress)
        {
            buildUX.transform.Find("UXIcon").GetComponent<Image>().sprite = uipm.personalProgressIcon;
        } else
        {
            progress = Mathf.RoundToInt(progress);
            buildUX.transform.Find("UXIcon").GetComponent<Image>().sprite = uipm.totalProgressIcon;
        }

        buildUX.transform.Find("UXText").GetComponent<TMP_Text>().text = progress.ToString() + "%";

        StartCoroutine(uip.DisplayUX(source, buildUX, true));
    }
}
