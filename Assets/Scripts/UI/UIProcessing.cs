using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Different states that the UI can be in at any given time.  IDLE will display nothing, while the other states will display correspondingly
public enum UIModes {
    IDLE,
    UNIT,
    BUILDINGINPROG,
    BUILDING,
    RESOURCE
}

public enum ActionModes
{
    IDLE,
    BUILD,
    BLUEPRINT,
    GATHER
}

// This script handles all interface processing when clicking on an object in the world
public class UIProcessing : MonoBehaviour
{
    [Tooltip("Set this to value for UI Layer - default is 5")]
    public int UILayer;

    [Tooltip("Number of maximum amount of units that can be selected at once")]
    public int selectedUnitsMax = 20;
    [Tooltip("Space between each selected unit in multiselect UI")]
    public float selectedUnitsUIEdgeSpace = 10;

    [Tooltip("Default outline width for highlighting objects")]
    [Range(1, 10)] public float defaultOutlineWidth = 5;
    [Tooltip("Width of outline for hovered units in multiselect UI")]
    [Range(1, 10)] public float hoveredUnitsOutlineWidth = 8;

    [Tooltip("Range of frames that mouse clicks need to be within to be considered a double click")]
    [Range(1, 50)] public int selectedObjectDoubleClickFrameBuffer = 10;

    [Tooltip("Speed of action button fade in/out when clicked")]
    public float actionButtonFadeSpeed;
    [Tooltip("How far to fade out when action button is clicked")]
    [Range(0, 255)] public float actionButtonFadeMin;
    [Tooltip("Duration in seconds of outline when starting a task")]
    [SerializeField] float taskConfirmationOutlineDuration;
    [Tooltip("Width of outline when clicking a task")]
    [SerializeField] [Range(1, 10)] float taskConfirmationOutlineWidth;

    [Tooltip("Distance of pixels above unit that UX will appear")]
    public float gatherBuildUXYDistance;
    [Tooltip("Default Scale of UX Feedback")]
    public float gatherBuildUXBaseScale;
    [Tooltip("Speed of UX feedback on screen moving upwards along y position")]
    public float gatherBuildUXSpeed;
    [Tooltip("How quickly the UX Feedback fades")]
    public float gatherBuildUXFadeFactor;
    [Tooltip("How fast the UX Feedback for personal build progress spins")]
    public float personalBuildUXSpinRate;
    [Tooltip("How fast the UX Feedback for total build progress spins")]
    public float totalBuildUXSpinRate;
    [Tooltip("Base Spin Factor for build progress UX feedback")]
    public float buildUXSpinFactor;

    [Tooltip("Canvas height is changed based on how many rows of actions are available, multiplied by this value.  This should be set to the action button's prefab height with some additional space above and below.  30 is default.")]
    public float buildingCanvasButtonAdjustment = 30;

    [Tooltip("Color for action icons when player does not have available resources to perform it")]
    public Color resourcesUnavailableForActionColor;

    [HideInInspector] public bool actionButtonClicked; // To know when an action button has been clicked
    bool multipleUnitButtonsGenerated; // To know when to reset multiple unit buttons on panel

    [HideInInspector] public bool optionsMenuOpened;

    [HideInInspector] public bool resetUI; // used to determine when new object has been clicked so UI can be refreshed

    [HideInInspector] public List<Unit> selectedUnits; // List of units selected by player with multiselect
    [HideInInspector] public BuildInProgress selectedBIP; // BuildInProgress selected by player
    [HideInInspector] public CompletedBuilding selectedCompletedBuilding; // Completed Building selected by player
    [HideInInspector] public Resource selectedResource; // Resource selected by player
    [HideInInspector] public Unit selectedUnit; // Current unit to be displayed in the UI

    [HideInInspector] public UIModes uiMode; // current state of UI
    [HideInInspector] public ActionModes actionMode;

    BuildManager bm; // used to check if build action has been selected
    GatherManager gm; // used to check if gather action has been selected
    AnimationManager am; // used to process UI animations
    GathererActions gathererActions; // used to retrieve the full list of potential gatherer actions
    BuilderActions builderActions; // used to retrieve the full list of potential builder actions
    BiomeTile biomeTile; // used to get biome information for selected object
    UIPrefabManager uipm; // used to retrieve UI elements in hierarchy

    void Start()
    {
        bm = FindObjectOfType<BuildManager>();
        gm = FindObjectOfType<GatherManager>();
        am = FindObjectOfType<AnimationManager>();
        uipm = transform.GetComponent<UIPrefabManager>();
        selectedUnits = FindObjectOfType<SelectionProcessing>().selectedUnits;
        gathererActions = FindObjectOfType<GathererActions>();
        builderActions = FindObjectOfType<BuilderActions>();

        biomeTile = new BiomeTile();

        //initialize top bar UI icons
        transform.Find("MainCanvas/TopBar/ResourceSpacer/Lumber/Icon").GetComponent<Image>().sprite = uipm.woodResourceIcon;
        transform.Find("MainCanvas/TopBar/ResourceSpacer/Ore/Icon").GetComponent<Image>().sprite = uipm.oreResourceIcon;
        transform.Find("MainCanvas/TopBar/ResourceSpacer/Food/Icon").GetComponent<Image>().sprite = uipm.foodResourceIcon;
        transform.Find("MainCanvas/TopBar/ResourceSpacer/Gold/Icon").GetComponent<Image>().sprite = uipm.goldResourceIcon;

        uiMode = UIModes.IDLE;
        actionMode = ActionModes.IDLE;
    }

    private void Update()
    {
        UpdateUI();
    }

    // Processes the UI elements to display chosen object's details based on which type of object was clicked
    void UpdateUI()
    {
        if (resetUI) // for things to run once
        {
            resetUI = false;

            switch (uiMode)
            {
                case UIModes.IDLE:
                    HideAllUIPanels(); // Keep all UI elements hidden

                    break;

                case UIModes.UNIT:
                    ShowUnitUI(); // Show Unit panels

                    break;

                case UIModes.BUILDINGINPROG:
                    ShowBuildingInProgressUI(); // Show Building In Progress panels

                    break;

                case UIModes.BUILDING:
                    ShowCompletedBuildingUI(); // Show Completed Building panels

                    break;

                case UIModes.RESOURCE:
                    SetResourceUI(); // Show Resource panels

                    break;
            }
        }

        // to keep updated every frame
        switch (uiMode)
        {
            case UIModes.IDLE:
                // nothing happens every frame during Idle state
                
                break;

            case UIModes.UNIT:
                ProcessUnitDetails(); // Show selected unit details
                break;

            case UIModes.BUILDINGINPROG:
                ProcessBuildingInProgressDetails(); // Show selected building in progress details

                break;

            case UIModes.BUILDING:
                ProcessCompletedBuildingDetails(); // Show completed building details

                break;

            case UIModes.RESOURCE:
                ProcessResourcesDetails(); // Show resource details
                break;
        }
    }

    #region OptionsMenu

    public void ProcessOptionsMenu()
    {
        if (!optionsMenuOpened)
        {
            OpenOptionsMenu();
        }
        else
        {
            CloseOptionsMenu();
        }
    }

    public void OpenOptionsMenu()
    {
        am.ProcessOpenAnim(uipm.optionsPanel, true);
        optionsMenuOpened = true;
    }

    public void CloseOptionsMenu()
    {
        am.ProcessOpenAnim(uipm.optionsPanel, false);
        optionsMenuOpened = false;
    }

    #endregion

    #region Units

    void ShowUnitUI() // Displays all relevant panels for unit selection
    {
        // hide other panels
        HideAllUIPanels();

        // set graphic panel details
        SetUnitGraphicPanel();

        // set action buttons
        SetUnitActionButtons();

        // show panel
        ShowUnitPanels(true);    

        if (selectedUnits.Count > 1) // if multiple units
        {
            if (!multipleUnitButtonsGenerated) // generate multiple unit buttons if they haven't been
            {
                GenerateMultipleUnitsPanel();
            }

            if (!am.GetIsOpen(uipm.multiUnitsPanel)) // show multiple units panel
            {
                // show panels
                ShowMultipleUnitsPanel(true);
            }

        } else
        {
            ShowMultipleUnitsPanel(false);
        }
    }

    void ProcessUnitDetails() // Keeps the appropriate panels updated - Stats panel will change if unit levels up or equips something, as well as any actions learned
    {
        // set stat panel details
        SetUnitStatsPanel();

        // set action panel details
        SetUnitActionPanel();
    }

    void GenerateMultipleUnitsPanel() // Generates the buttons for multiple units when selected
    {
        int selectedUnitsCount = selectedUnits.Count;
        float prefabWidth = uipm.multiUnitsButton.GetComponent<RectTransform>().rect.width;

        Transform spacer = uipm.multiUnitsPanel.transform.Find("MultiUnitsSpacer");

        // set width of rect transform of multiUnitsPanel
        // edge space + (units spacing * unit count) + (count * prefab width)
        float multiUnitSpacing = uipm.multiUnitsPanel.transform.Find("MultiUnitsSpacer").GetComponent<HorizontalLayoutGroup>().spacing;
        float newWidth = (selectedUnitsUIEdgeSpace + (multiUnitSpacing * selectedUnitsCount) + (prefabWidth * selectedUnitsCount));

        RectTransform rt = uipm.multiUnitsPanel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(newWidth, rt.rect.height);

        // clear old units in spacer
        foreach (Transform child in spacer)
        {
            Destroy(child.gameObject);
        }

        // generate buttons and set them as children of spacer
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            GameObject newButton = Instantiate(uipm.multiUnitsButton, spacer);
            newButton.name = i + ") UnitMultiSelectButton";
            newButton.GetComponent<Image>().sprite = selectedUnits[i].baseUnit.GetFaceGraphic();
            newButton.GetComponent<MultiUnitsSelectionButton>().unit = selectedUnits[i];
        }

        multipleUnitButtonsGenerated = true;
    }

    void SetUnitGraphicPanel() // Displays unit name and face graphic to the UI
    {
        string name = string.Empty;

        if (GetVillagerUnit(selectedUnit))
        {
            name = GetVillagerUnit(selectedUnit).villagerClass.ToString();
            name = uipm.UppercaseFirstAndAfterSpaces(name);
        }

        GetTextComp(uipm.unitGraphicPanel.transform.Find("Name")).text = name;

        uipm.unitGraphicPanel.transform.Find("Graphic").GetComponent<Image>().sprite = selectedUnit.baseUnit.GetFaceGraphic();
    }

    void SetUnitStatsPanel() // Displays selected unit's stats to the UI
    {
        if (GetVillagerUnit(selectedUnit))
        {
            // Level
            GetTextComp(uipm.unitStatsPanel.transform.Find("LevelText")).text = selectedUnit.baseUnit.GetLevel().ToString();

            // exp
            GetTextComp(uipm.unitStatsPanel.transform.Find("EXPText")).text = selectedUnit.baseUnit.GetEXP() + "/" + selectedUnit.baseUnit.GetExpToNextLevel();
            GetSlider(uipm.unitStatsPanel.transform.Find("EXPSlider")).fillAmount = (float)selectedUnit.baseUnit.GetEXP() / (float)selectedUnit.baseUnit.GetExpToNextLevel();

            // HP
            GetTextComp(uipm.unitStatsPanel.transform.Find("HPText")).text = selectedUnit.baseUnit.GetHP() + "/" + selectedUnit.baseUnit.GetMaxHP();
            GetSlider(uipm.unitStatsPanel.transform.Find("HPSlider")).fillAmount = (float)selectedUnit.baseUnit.GetHP() / (float)selectedUnit.baseUnit.GetMaxHP();

            // MP
            if (selectedUnit.baseUnit.usesEnergy)
            {
                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("MPIconFrame").gameObject, false);
                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("MPSlider").gameObject, false);
                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("MPText").gameObject, false);

                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("EnergyIconFrame").gameObject, true);
                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("EnergySlider").gameObject, true);
                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("EnergyText").gameObject, true);

                GetTextComp(uipm.unitStatsPanel.transform.Find("EnergyText")).text = selectedUnit.baseUnit.GetMP() + "/" + selectedUnit.baseUnit.GetMaxMP();
                GetSlider(uipm.unitStatsPanel.transform.Find("EnergySlider")).fillAmount = (float)selectedUnit.baseUnit.GetMP() / (float)selectedUnit.baseUnit.GetMaxMP();
            }
            else
            {
                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("MPIconFrame").gameObject, true);
                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("MPSlider").gameObject, true);
                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("MPText").gameObject, true);

                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("EnergyIconFrame").gameObject, false);
                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("EnergySlider").gameObject, false);
                uipm.ShowUIObject(uipm.unitStatsPanel.transform.Find("EnergyText").gameObject, false);

                GetTextComp(uipm.unitStatsPanel.transform.Find("MPText")).text = selectedUnit.baseUnit.GetMP() + "/" + selectedUnit.baseUnit.GetMaxMP();
                GetSlider(uipm.unitStatsPanel.transform.Find("MPSlider")).fillAmount = (float)selectedUnit.baseUnit.GetMP() / (float)selectedUnit.baseUnit.GetMaxMP();
            }

            // Strength
            GetTextComp(uipm.unitStatsPanel.transform.Find("StatIconSpacer/Strength/StrengthText")).text = selectedUnit.baseUnit.GetStrength().ToString();

            // Stamina
            GetTextComp(uipm.unitStatsPanel.transform.Find("StatIconSpacer/Stamina/StaminaText")).text = selectedUnit.baseUnit.GetStamina().ToString();

            // Agility
            GetTextComp(uipm.unitStatsPanel.transform.Find("StatIconSpacer/Agility/AgilityText")).text = selectedUnit.baseUnit.GetAgility().ToString();

            // Luck
            GetTextComp(uipm.unitStatsPanel.transform.Find("StatIconSpacer/Luck/LuckText")).text = selectedUnit.baseUnit.GetLuck().ToString();

            // Intelligence
            GetTextComp(uipm.unitStatsPanel.transform.Find("StatIconSpacer/Intelligence/IntelligenceText")).text = selectedUnit.baseUnit.GetIntelligence().ToString();

            // Willpower
            GetTextComp(uipm.unitStatsPanel.transform.Find("StatIconSpacer/Willpower/WillpowerText")).text = selectedUnit.baseUnit.GetWillpower().ToString();

            // Movement
            GetTextComp(uipm.unitStatsPanel.transform.Find("StatIconSpacer/Movement/MovementText")).text = selectedUnit.baseUnit.GetMovement().ToString();

            // Biome
            biomeTile.SetBiomeID(selectedUnit.transform.position);

            GetTextComp(uipm.unitStatsPanel.transform.Find("BiomeText")).text = uipm.UppercaseFirstAndAfterSpaces(biomeTile.primaryBiomeType.ToString());
        }
        else
        {
            Debug.LogError(selectedUnit.gameObject.name + " is not a villager unit!");
        }
    }

    void SetUnitActionPanel() // Displays the various components of the action panel
    {
        if (GetVillagerUnit(selectedUnit))
        {
            if ((GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.FARMER ||
                GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.LUMBERJACK ||
                GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.MINER ||
                GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.VILLAGER) &&
                !GetVillagerUnit(selectedUnit).buildTaskIsActive)
            {
                SetActionPanelForGatherer();
            }

            if ((GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.BUILDER ||
                      GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.VILLAGER) &&
                      !GetVillagerUnit(selectedUnit).gatherTaskIsActive)
            {
                if (GetVillagerUnit(selectedUnit).buildTaskIsActive)
                {
                    GetTextComp(uipm.unitActionPanel.transform.Find("ActionText")).text = GetVillagerUnit(selectedUnit).personalBuildProgress + "%";

                    GetSlider(uipm.unitActionPanel.transform.Find("ActionSlider")).fillAmount = GetVillagerUnit(selectedUnit).personalBuildProgress / 100;
                    GetSlider(uipm.unitActionPanel.transform.Find("ActionTimeSlider")).fillAmount = GetVillagerUnit(selectedUnit).buildTimeElapsed / GetVillagerUnit(selectedUnit).GetBuildTime();

                    if (!uipm.unitActionPanel.transform.Find("ActionSlider").gameObject.activeInHierarchy)
                    {
                        uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionSlider").gameObject, true);
                        uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionText").gameObject, true);
                    }
                    if (!uipm.unitActionPanel.transform.Find("ActionTimeSlider").gameObject.activeInHierarchy)
                    {
                        uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionTimeSlider").gameObject, true);
                    }
                }
                else
                {
                    if (uipm.unitActionPanel.transform.Find("ActionSlider").gameObject.activeInHierarchy)
                    {
                        uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionSlider").gameObject, false);
                        uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionText").gameObject, false);
                        uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionTimeSlider").gameObject, false);
                    }
                }
            }
        }
        else
        {
            Debug.LogError(selectedUnit.gameObject.name + " is not a villager unit!");
        }
    }

    void SetActionPanelForGatherer() // If villager unit is selected and it is a gatherer, update the UI with appropriate details
    {
        // Update gathering bar if gathering class
        if ((GetVillagerUnit(selectedUnit).gatherTaskIsActive || GetVillagerUnit(selectedUnit).resourcesHolding > 0) && !GetVillagerUnit(selectedUnit).buildTaskIsActive)
        {
            GetTextComp(uipm.unitActionPanel.transform.Find("ActionText")).text = GetVillagerUnit(selectedUnit).resourcesHolding + "/" + GetVillagerUnit(selectedUnit).GetCarryLimit();

            GetSlider(uipm.unitActionPanel.transform.Find("ActionSlider")).fillAmount = (float)GetVillagerUnit(selectedUnit).resourcesHolding / GetVillagerUnit(selectedUnit).GetCarryLimit();
            GetSlider(uipm.unitActionPanel.transform.Find("ActionTimeSlider")).fillAmount = (float)GetVillagerUnit(selectedUnit).gatherTimeElapsed / GetVillagerUnit(selectedUnit).GetGatherTime();

            if (!uipm.unitActionPanel.transform.Find("ActionSlider").gameObject.activeInHierarchy)
            {
                uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionSlider").gameObject, true);
                uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionText").gameObject, true);
            }
            if (!uipm.unitActionPanel.transform.Find("ActionTimeSlider").gameObject.activeInHierarchy)
            {
                uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionTimeSlider").gameObject, true);
            }
        }
        else
        {
            if (uipm.unitActionPanel.transform.Find("ActionSlider").gameObject.activeInHierarchy)
            {
                uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionSlider").gameObject, false);
                uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionText").gameObject, false);
                uipm.ShowUIObject(uipm.unitActionPanel.transform.Find("ActionTimeSlider").gameObject, false);
            }
        }
    }

    void SetUnitActionButtons() // Generates actions in the form of buttons that unit can perform to the UI
    {
        foreach (Transform child in uipm.actionSpacer.transform)
        {
            Destroy(child.gameObject);
        }
        if (GetVillagerUnit(selectedUnit))
        {
            if (GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.BUILDER ||
                GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.VILLAGER)
            {
                // for each builder action in BuilderActions

                foreach (BaseAction action in builderActions.builderActions)
                {
                    if (selectedUnit.baseUnit.GetLevel() >= action.levelRequired)
                    {
                        // Add action button for 'Build'

                        // prepare action button
                        GameObject buildActionGO = GameObject.Instantiate(uipm.actionButton) as GameObject;
                        buildActionGO.transform.SetParent(uipm.actionSpacer.transform, false);

                        // Set Icon
                        buildActionGO.transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>().sprite = action.icon;

                        // Set Name
                        GetTextComp(buildActionGO.transform.Find("SkillName")).text = action.name;

                        // Set Shortcut
                        GetTextComp(buildActionGO.transform.Find("ShortcutKeyFrame/ShortcutKey")).text = action.shortcutKey.ToString();

                        // Set Action
                        BuildAction ba = buildActionGO.AddComponent(typeof(BuildAction)) as BuildAction;
                        ba.action = action;

                        // Set Unit
                        ba.unit = selectedUnit;
                    }
                }
            }

            if (GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.FARMER ||
                    GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.LUMBERJACK ||
                    GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.MINER ||
                    GetVillagerUnit(selectedUnit).villagerClass == VillagerClasses.VILLAGER)
            {

                // for each gatherer action in GathererActions
                foreach (BaseAction action in gathererActions.gathererActions)
                {
                    if (selectedUnit.baseUnit.GetLevel() >= action.levelRequired)
                    {
                        // Add action button for 'Gather'

                        // prepare action button
                        GameObject gatherActionGO = GameObject.Instantiate(uipm.actionButton) as GameObject;
                        gatherActionGO.transform.SetParent(uipm.actionSpacer.transform, false);

                        // Set Icon
                        gatherActionGO.transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>().sprite = action.icon;

                        // Set Name
                        GetTextComp(gatherActionGO.transform.Find("SkillName")).text = action.name;

                        // Set Shortcut
                        GetTextComp(gatherActionGO.transform.Find("ShortcutKeyFrame/ShortcutKey")).text = action.shortcutKey.ToString();

                        // Set Action
                        GatherAction ga = gatherActionGO.AddComponent(typeof(GatherAction)) as GatherAction;
                        ga.action = action;

                        // Set Unit
                        ga.unit = selectedUnit;
                    }
                }

            }
        }
    }

    public void SetCurrentUnit(Unit unitToSet) // Simply updates the selectedUnit to the parameter unitToSet
    {
        selectedUnit = unitToSet;
    }

    public void ShowUnitPanels(bool show) // Shows/hides the unit panels in UI
    {
        //uipm.ShowUIObject(uipm.unitCanvas.gameObject, show);
        //uipm.ShowUIObject(uipm.unitGraphicPanel, show);
        //uipm.ShowUIObject(uipm.unitActionPanel, show);
        //uipm.ShowUIObject(uipm.unitStatsPanel, show);

        // hide other panels
        HideAllUIPanels();

        am.ProcessOpenAnim(uipm.unitCanvas, show);
    }

    public void ShowMultipleUnitsPanel(bool show) // Shows/hides multiple units panel
    {
        //uipm.ShowUIObject(uipm.multiUnitsPanel, show);
        if (show)
        {
            am.ProcessOpenAnim(uipm.multiUnitsPanel, true);
        } else
        {
            multipleUnitButtonsGenerated = false;
            am.ProcessOpenAnim(uipm.multiUnitsPanel, false);
        }
    }

    #endregion

    #region Buildings

    void SetBuildingActionButtons() // Sets the actions that a building can perform - there are none yet.
    {
        if (selectedCompletedBuilding.building.actions.Count > 0)
        {
            foreach (BaseAction action in selectedCompletedBuilding.building.actions)
            {
                // set action buttons in panel
            }
        }
    }

    void SetBuildingStatsPanel(bool isBiP) // Shows the selected buildings stat and attribute parameters
    {
        if (isBiP && selectedBIP != null) // build in progress
        {
            DisplayAndSetBuildingInProgressToStatsPanel(); // if building in progress is selected, display the progress, level, depot resource icon, and biome of the building
        }
        else // completed building
        {
            DisplayAndSetCompletedBuildingToStatsPanel(); // if completed building is selected, display the durability, level, depot resource icon, and biome of the building
        }
    }

    void DisplayAndSetCompletedBuildingToStatsPanel()
    {
        // Hide progress elements and show durability elements
        uipm.ShowUIObject(uipm.buildingStatsPanel.transform.Find("DurabilitySlider").gameObject, true);
        uipm.ShowUIObject(uipm.buildingStatsPanel.transform.Find("DurabilityText").gameObject, true);
        uipm.ShowUIObject(uipm.buildingStatsPanel.transform.Find("ProgressSlider").gameObject, false);
        uipm.ShowUIObject(uipm.buildingStatsPanel.transform.Find("ProgressText").gameObject, false);

        // durability
        GetTextComp(uipm.buildingStatsPanel.transform.Find("DurabilityText")).text = selectedCompletedBuilding.building.currentDurability.ToString() + "/" + selectedCompletedBuilding.building.maxDurability.ToString();
        GetSlider(uipm.buildingStatsPanel.transform.Find("DurabilitySlider")).fillAmount = selectedCompletedBuilding.building.currentDurability / selectedCompletedBuilding.building.maxDurability;

        // level
        GetTextComp(uipm.buildingStatsPanel.transform.Find("LevelText")).text = selectedCompletedBuilding.building.level.ToString();

        // depot icon
        if (selectedCompletedBuilding.building.depotResource != depotResources.NA)
        {
            switch (selectedCompletedBuilding.building.depotResource)
            {
                case depotResources.FOOD:
                    uipm.buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = uipm.foodResourceIcon;
                    break;
                case depotResources.ORE:
                    uipm.buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = uipm.oreResourceIcon;
                    break;
                case depotResources.WOOD:
                    uipm.buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = uipm.woodResourceIcon;
                    break;
                case depotResources.ALL:
                    uipm.buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = uipm.allResourceIcon;
                    break;
            }
        }

        // biome
        biomeTile.SetBiomeID(selectedCompletedBuilding.transform.position);

        GetTextComp(uipm.buildingStatsPanel.transform.Find("BiomeText")).text = uipm.UppercaseFirstAndAfterSpaces(biomeTile.primaryBiomeType.ToString());
    }

    void DisplayAndSetBuildingInProgressToStatsPanel()
    {
        // Hide durability elements and show progress elements
        uipm.ShowUIObject(uipm.buildingStatsPanel.transform.Find("DurabilitySlider").gameObject, false);
        uipm.ShowUIObject(uipm.buildingStatsPanel.transform.Find("DurabilityText").gameObject, false);
        uipm.ShowUIObject(uipm.buildingStatsPanel.transform.Find("ProgressSlider").gameObject, true);
        uipm.ShowUIObject(uipm.buildingStatsPanel.transform.Find("ProgressText").gameObject, true);

        // progress
        GetTextComp(uipm.buildingStatsPanel.transform.Find("ProgressText")).text = Mathf.RoundToInt(selectedBIP.progress).ToString() + "%";
        GetSlider(uipm.buildingStatsPanel.transform.Find("ProgressSlider")).fillAmount = selectedBIP.progress / 100.0f;

        // level
        GetTextComp(uipm.buildingStatsPanel.transform.Find("LevelText")).text = selectedBIP.building.level.ToString();

        // depot icon
        if (selectedBIP.building.depotResource != depotResources.NA)
        {
            switch (selectedBIP.building.depotResource)
            {
                case depotResources.FOOD:
                    uipm.buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = uipm.foodResourceIcon;
                    break;
                case depotResources.ORE:
                    uipm.buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = uipm.oreResourceIcon;
                    break;
                case depotResources.WOOD:
                    uipm.buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = uipm.woodResourceIcon;
                    break;
                case depotResources.ALL:
                    uipm.buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = uipm.allResourceIcon;
                    break;
            }
        }

        // biome
        biomeTile.SetBiomeID(selectedBIP.transform.position);

        GetTextComp(uipm.buildingStatsPanel.transform.Find("BiomeText")).text = uipm.UppercaseFirstAndAfterSpaces(biomeTile.primaryBiomeType.ToString());
    }

    void SetBuildingGraphicPanel(bool isBiP) // Sets the graphic panel details - name and icon
    {
        if (isBiP) // build in progress
        {
            string name;
            name = selectedBIP.building.name;
            name = uipm.UppercaseFirstAndAfterSpaces(name);
            GetTextComp(uipm.buildingGraphicPanel.transform.Find("Name")).text = name;

            uipm.buildingGraphicPanel.transform.Find("Graphic").GetComponent<Image>().sprite = selectedBIP.building.icon;
        } else // completed building
        {
            string name;
            name = selectedCompletedBuilding.building.name;
            name = uipm.UppercaseFirstAndAfterSpaces(name);
            GetTextComp(uipm.buildingGraphicPanel.transform.Find("Name")).text = name;

            uipm.buildingGraphicPanel.transform.Find("Graphic").GetComponent<Image>().sprite = selectedCompletedBuilding.building.icon;
        }
    }

    #endregion

    #region CompletedBuilding

    void ShowCompletedBuildingUI() // Displays elements required for the selected completed building
    {
        // Hide other panels
        //ShowUnitPanels(false);
        //ShowMultipleUnitsPanel(false);
        //ShowResourcePanels(false);
        //ShowBuildingActionPanels(true);

        // hide other panels
        HideAllUIPanels();

        // set graphic panel details
        SetBuildingGraphicPanel(false);

        // show panel
        am.ProcessOpenAnim(uipm.buildingCanvas, true);
        //ShowBuildingPanels(true);
    }

    void ProcessCompletedBuildingDetails() // Displays the elements needed to be updated every frame for the completed building (ie stats in the event of a level up, as well as action buttons)
    {
        // set action buttons
        SetBuildingActionButtons();

        // set stat panel details
        SetBuildingStatsPanel(false);
    }

    #endregion

    #region BuildInProgress

    void ShowBuildingInProgressUI() // Displays elements needed for building in progress when selected
    {
        if (selectedBIP == null) // eventually it would be nice to make this auto focus the completed building - this is being run when the building in progress is selected, but is then completed
        {
            resetUI = true;
            uiMode = UIModes.IDLE;
        }
        else
        {
            // Hide other panels
            //ShowUnitPanels(false);
            //ShowMultipleUnitsPanel(false);
            //ShowResourcePanels(false);
            //ShowBuildingActionPanels(false);

            // hide other panels
            HideAllUIPanels();

            // set graphic panel details
            SetBuildingGraphicPanel(true);

            // show panel
            am.ProcessOpenAnim(uipm.buildingCanvas, true);
            //ShowBuildingPanels(true);
        }
    }

    void ProcessBuildingInProgressDetails() // Displays elements needed for stat panel to be updated every frame (primarily keeping progress updated)
    {
        if (selectedBIP == null) // eventually it would be nice to make this auto focus the completed building - this is being run when the building in progress is selected, but is then completed
        {
            resetUI = true;
            uiMode = UIModes.IDLE;
        }
        else
        {
            // set stat panel details
            SetBuildingStatsPanel(true);
        }
    }

    #endregion

    #region Resources

    void ShowResourcePanels() // Shows/hides resource panels for UI
    {
        //uipm.ShowUIObject(uipm.resourceCanvas, show);
        //uipm.ShowUIObject(uipm.resourceGraphicPanel, show);
        //uipm.ShowUIObject(uipm.resourceStatsPanel, show);

        // hide other panels
        HideAllUIPanels();

        am.ProcessOpenAnim(uipm.resourceCanvas, true);
    }

    void SetResourceUI() // Sets required elements for resource panels
    {
        //ShowUnitPanels(false);
        //ShowMultipleUnitsPanel(false);

        // set graphic panel details
        SetResourceGraphicPanel();

        // set stat panel details
        SetResourceStatsPanel();

        // show panel
        ShowResourcePanels();
    }

    void SetResourceGraphicPanel() // Sets name and icon to resource graphic panel
    {
        GetTextComp(uipm.resourceGraphicPanel.transform.Find("Name")).text = selectedResource.name;

        uipm.resourceGraphicPanel.transform.Find("Graphic").GetComponent<Image>().sprite = selectedResource.icon;
    }

    void SetResourceStatsPanel() // Sets resource type and biome to resource stats panel
    {
        switch (selectedResource.resourceType)
        {
            case ResourceTypes.WOOD:
                uipm.resourceStatsPanel.transform.Find("Graphic").GetComponent<Image>().sprite = uipm.woodResourceIcon;
                break;
            case ResourceTypes.ORE:
                uipm.resourceStatsPanel.transform.Find("Graphic").GetComponent<Image>().sprite = uipm.oreResourceIcon;
                break;
            case ResourceTypes.FOOD:
                uipm.resourceStatsPanel.transform.Find("Graphic").GetComponent<Image>().sprite = uipm.foodResourceIcon;
                break;
        }

        // biome
        biomeTile.SetBiomeID(selectedResource.transform.position);

        GetTextComp(uipm.resourceStatsPanel.transform.Find("BiomeText")).text = uipm.UppercaseFirstAndAfterSpaces(biomeTile.primaryBiomeType.ToString());
    }

    void ProcessResourcesDetails() // Keeps UI updated with amount of resources remaining for selected resource
    {
        GetTextComp(uipm.resourceStatsPanel.transform.Find("RemainingResourcesText")).text = selectedResource.resourcesRemaining + "/" + selectedResource.totalResources;
        GetSlider(uipm.resourceStatsPanel.transform.Find("RemainingResourcesSlider")).fillAmount = (float)selectedResource.resourcesRemaining / (float)selectedResource.totalResources;
    }

    #endregion

    #region UX

    public void ButtonUIProcessing(GameObject ab) // public method used to trigger the fade PingPong effect on the chosen action
    {
        StartCoroutine(ActionIconFade(ab));
    }

    IEnumerator ActionIconFade(GameObject actionButton) // Determines which type of action was clicked to determine when it is no longer active (ie, user cancels the action)
    {
        Image icon = actionButton.transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>();
        Color originColor = icon.color;

        if (actionButton.GetComponent<BuildAction>())
        {
            while (actionMode == ActionModes.BUILD)
            {
                ProcessFade(icon);

                yield return null;
            }
        }
        else if (actionButton.GetComponent<BuildingAction>())
        {
            while (actionMode == ActionModes.BLUEPRINT)
            {
                ProcessFade(icon);

                yield return null;
            }
        }
        else if (actionButton.GetComponent<GatherAction>())
        {
            while (actionMode == ActionModes.GATHER)
            {
                ProcessFade(icon);

                yield return null;
            }
        }

        ResetIconAlpha(originColor, icon);
    }
    
    public void SetToCanvasSpace(GameObject source, RectTransform prefab) // sets the user feedback of choosing a resource to the canvas space so it is viewed from the same angle, regardless of camera rotation
    {
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(source.transform.position);
        Vector2 unitObj_ScreenPosition = new Vector2(
        ((ViewportPosition.x * uipm.UXCanvas.sizeDelta.x) - (uipm.UXCanvas.sizeDelta.x * 0.5f)),
        ((ViewportPosition.y * uipm.UXCanvas.sizeDelta.y) - (uipm.UXCanvas.sizeDelta.y * 0.5f) + gatherBuildUXYDistance));

        prefab.anchoredPosition = unitObj_ScreenPosition;
    }

    public IEnumerator DisplayUX(GameObject source, GameObject UXPrefab, bool build) // Shows UX above designated object to show resource gathered/build progress
    {
        bool feedbackHidden = false;

        Vector3 pos = source.transform.position;
        pos = new Vector3(pos.x, pos.y + gatherBuildUXYDistance, pos.z);

        UXPrefab = Instantiate(UXPrefab, pos, Quaternion.identity, uipm.UXCanvas);

        SetToCanvasSpace(source, UXPrefab.GetComponent<RectTransform>());

        UXPrefab.transform.localScale = new Vector3(gatherBuildUXBaseScale, gatherBuildUXBaseScale, gatherBuildUXBaseScale);
        CanvasGroup cg = UXPrefab.GetComponent<CanvasGroup>();

        RectTransform iconRT;
        if (build)
        {
            iconRT = UXPrefab.transform.Find("UXIcon").GetComponent<RectTransform>();
            if (source.CompareTag("Unit"))
            {
                // Icon is a little too big, so shrinking that slightly
                iconRT.localScale = new Vector3(iconRT.localScale.x - .075f, iconRT.localScale.y - .075f);
            }
        } else
        {
            iconRT = null;
        }
            while (!feedbackHidden)
            {
                float fade = gatherBuildUXFadeFactor * Time.deltaTime;
                float floatSpeed = gatherBuildUXSpeed * Time.deltaTime;

                Vector3 newPos = new Vector3(UXPrefab.GetComponent<RectTransform>().position.x, UXPrefab.GetComponent<RectTransform>().position.y + floatSpeed, UXPrefab.GetComponent<RectTransform>().position.z);

                cg.alpha -= fade;
                UXPrefab.GetComponent<RectTransform>().position = newPos;

                if (build && source != null)
                {
                    if (source.CompareTag("Unit")) // personal build progress
                    {
                        iconRT.Rotate(new Vector3(0, 0, -(personalBuildUXSpinRate * buildUXSpinFactor)));

                    }
                    else if (source.CompareTag("BuildingInProgress")) // total build progress
                    {
                        iconRT.Rotate(new Vector3(0, 0, -(totalBuildUXSpinRate * buildUXSpinFactor)));
                    }
                }

                if (cg.alpha == 0.0f)
                {
                    Destroy(UXPrefab);
                    feedbackHidden = true;
                }

                yield return new WaitForEndOfFrame();
            }      
    }

    private void ProcessFade(Image icon) // Processes PingPong fade effect on the given image
    {
        float newAlpha = Mathf.Lerp(actionButtonFadeMin, 255, Mathf.PingPong((Time.time * actionButtonFadeSpeed), 1));
        Color newColor = icon.color;
        newColor.a = (newAlpha / 255.0f);
        icon.color = newColor;
    }

    public IEnumerator HighlightConfirmedResourceOrBIP(Outline outline) // Shows UX for when a resource/bip is chosen to be worked on by a unit
    {
        outline.OutlineWidth = taskConfirmationOutlineWidth;
        outline.enabled = true;
        yield return new WaitForSeconds(taskConfirmationOutlineDuration);
        outline.enabled = false;
    }

    public void HighlightResourceOrBuilding(Outline outline, bool highlight) // Shows UX for when a resource, CompletedBuilding, or Build In Progress is selected in the UI
    {
        outline.OutlineWidth = taskConfirmationOutlineWidth;
        outline.enabled = highlight;
    }

    void ResetIconAlpha(Color originColor, Image icon) // Sets the icon's color back to it's default color before fade PingPong effect
    {
        icon.color = originColor;
    }

    #endregion

    void HideAllUIPanels() // Simply hides all UI panels if nothing is selected
    {
        //ShowUnitPanels(false);
        //ShowMultipleUnitsPanel(false);
        //ShowResourcePanels(false);
        //ShowBuildingPanels(false);
        //ShowBuildingActionPanels(false);

        am.ProcessOpenAnim(uipm.unitCanvas, false);
        am.ProcessOpenAnim(uipm.resourceCanvas, false);
        am.ProcessOpenAnim(uipm.buildingCanvas, false);

        if (uiMode != UIModes.UNIT)
        {
            ShowMultipleUnitsPanel(false);
        }       
    }

    public VillagerUnit GetVillagerUnit(Unit unit) // Returns Villager Unit from given Unit
    {
        VillagerUnit tryVillager = unit as VillagerUnit;

        return tryVillager;
    }

    TMP_Text GetTextComp (Transform transform) // Returns TextMeshPro_Text component from given transform
    {
        return transform.GetComponent<TMP_Text>();
    }

    Image GetSlider(Transform transform) // Returns Image component from given transform
    {
        return transform.Find("Fill Area/Fill").GetComponent<Image>();
    }
}
