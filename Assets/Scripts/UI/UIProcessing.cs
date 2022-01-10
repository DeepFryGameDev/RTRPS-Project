using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UIModes {
    IDLE,
    UNIT,
    BUILDINGINPROG,
    BUILDING,
    RESOURCE
}
public class UIProcessing : MonoBehaviour
{
    [Tooltip("Set this to value for UI Layer - default is 5")]
    public int UILayer;

    public int selectedUnitsMax = 20;
    public float selectedUnitsUIEdgeSpace = 10;
    [Range(1, 10)] public float hoveredUnitsOutlineWidth = 8;
    [Range(1, 50)] public int selectedUnitDoubleClickFrameBuffer = 10;

    [Tooltip("Set to unit canvas in UI")]
    [SerializeField] GameObject unitCanvas;
    [Tooltip("Set to resource canvas in UI")]
    [SerializeField] GameObject resourceCanvas;
    [Tooltip("Set to building canvas in UI")]
    [SerializeField] GameObject buildingCanvas;
    [Tooltip("Set to unit graphic panel in UI")]
    [SerializeField] GameObject unitGraphicPanel;
    [Tooltip("Set to unit stats panel in UI")]
    [SerializeField] GameObject unitStatsPanel;
    [Tooltip("Set to unit action panel in UI")]
    [SerializeField] GameObject unitActionPanel;
    [Tooltip("Set to resource graphic panel in UI")]
    [SerializeField] GameObject resourceGraphicPanel;
    [Tooltip("Set to resource stats panel in UI")]
    [SerializeField] GameObject resourceStatsPanel;
    [Tooltip("Set to building graphic panel in UI")]
    [SerializeField] GameObject buildingGraphicPanel;
    [Tooltip("Set to building stats panel in UI")]
    [SerializeField] GameObject buildingStatsPanel;
    [Tooltip("Set to building action panel in UI")]
    [SerializeField] GameObject buildingActionPanel;
    [Tooltip("Set to multiple units panel in UI")]
    [SerializeField] GameObject multiUnitsPanel;
    [Tooltip("Set to multiple units 'button' prefab")]
    [SerializeField] GameObject multiUnitsButton;
    [Tooltip("Set to action action skill spacer")]
    [SerializeField] GameObject actionSpacer;
    [Tooltip("Set to action button prefab")]
    [SerializeField] GameObject actionButton;
    [Tooltip("Speed of action button fade in/out when clicked")]
    public float actionButtonFadeSpeed;
    [Tooltip("How far to fade out when action button is clicked")]
    [Range(0, 255)] public float actionButtonFadeMin;
    [Tooltip("Duration in seconds of outline when clicking a resource")]
    [SerializeField] float taskConfirmationOutlineDuration;
    [Tooltip("Width of outline when clicking a resource")]
    [SerializeField] [Range(1, 10)] float taskConfirmationOutlineWidth;

    [Tooltip("Set to icon used for wood resource in UI")]
    public Sprite woodResourceIcon;
    [Tooltip("Set to icon used for ore resource in UI")]
    public Sprite oreResourceIcon;
    [Tooltip("Set to icon used for food resource in UI")]
    public Sprite foodResourceIcon;
    [Tooltip("Set to icon used for all resources in UI")]
    public Sprite allResourceIcon;
    [Tooltip("Set to icon used for gold resource in UI")]
    public Sprite goldResourceIcon;

    [ReadOnly] public UIModes uiMode;

    [ReadOnly] public bool actionButtonClicked, gatherActionClicked, buildActionClicked, buildingActionClicked, actionButtonFadeBreak;

    [HideInInspector] public bool resetUI;

    [HideInInspector] public List<Unit> selectedUnits;
    Unit currentUnit;
    float multiUnitsPanelSpacing;

    [HideInInspector] public BuildInProgress currentBip;
    [HideInInspector] public CompletedBuilding currentBuilding;

    [HideInInspector] public Resource currentResource;

    GathererActions gathererActions;
    BuilderActions builderActions;



    // Start is called before the first frame update
    void Start()
    {
        selectedUnits = FindObjectOfType<SelectionProcessing>().selectedUnits;

        transform.Find("MainCanvas/TopBar/ResourceSpacer/Lumber/Icon").GetComponent<Image>().sprite = woodResourceIcon;
        transform.Find("MainCanvas/TopBar/ResourceSpacer/Ore/Icon").GetComponent<Image>().sprite = oreResourceIcon;
        transform.Find("MainCanvas/TopBar/ResourceSpacer/Food/Icon").GetComponent<Image>().sprite = foodResourceIcon;
        transform.Find("MainCanvas/TopBar/ResourceSpacer/Gold/Icon").GetComponent<Image>().sprite = goldResourceIcon;

        HorizontalLayoutGroup horizontalLayoutGroup = multiUnitsPanel.transform.Find("MultiUnitsSpacer").GetComponent<HorizontalLayoutGroup>();

        gathererActions = FindObjectOfType<GathererActions>();
        builderActions = FindObjectOfType<BuilderActions>();

        multiUnitsPanelSpacing = horizontalLayoutGroup.spacing;

        uiMode = UIModes.IDLE;
    }

    private void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (resetUI) // for things to run once
        {
            resetUI = false;

            switch (uiMode)
            {
                case UIModes.IDLE:
                    ShowUnitPanels(false);
                    ShowMultipleUnitsPanel(false);
                    ShowResourcePanels(false);
                    ShowBuildingPanels(false);
                    ShowBuildingActionPanels(false);

                    break;

                case UIModes.UNIT:
                    ShowResourcePanels(false);
                    ShowBuildingPanels(false);
                    ShowBuildingActionPanels(false);

                    // set graphic panel details
                    SetUnitGraphicPanel();

                    // set action buttons
                    SetUnitActionButtons();

                    // show panel
                    ShowUnitPanels(true);

                    //Make sure multiple units panel is gone
                    ShowMultipleUnitsPanel(false);

                    if (selectedUnits.Count > 1)
                    {
                        // also show multiple units panel
                        if (!multiUnitsPanel.activeInHierarchy)
                        {
                            GenerateMultipleUnitsPanel();

                            // show panels
                            ShowMultipleUnitsPanel(true);
                        }
                    }

                    break;

                case UIModes.BUILDINGINPROG:
                    ShowUnitPanels(false);
                    ShowMultipleUnitsPanel(false);
                    ShowResourcePanels(false);
                    ShowBuildingActionPanels(false);

                    // set graphic panel details
                    SetBuildingGraphicPanel(true);

                    // show panel
                    ShowBuildingPanels(true);

                    break;

                case UIModes.BUILDING:
                    ShowUnitPanels(false);
                    ShowMultipleUnitsPanel(false);
                    ShowResourcePanels(false);
                    ShowBuildingActionPanels(true);

                    // set graphic panel details
                    SetBuildingGraphicPanel(false);

                    // show panel
                    ShowBuildingPanels(true);

                    break;

                case UIModes.RESOURCE:
                    ShowUnitPanels(false);
                    ShowMultipleUnitsPanel(false);

                    // set graphic panel details
                    SetResourceGraphicPanel();

                    // set stat panel details
                    SetResourceStatsPanel();

                    // show panel
                    ShowResourcePanels(true);

                    break;
            }
        }

        // to keep updated every frame
        switch (uiMode)
        {
            case UIModes.IDLE:

                break;

            case UIModes.UNIT:
                // set stat panel details
                SetUnitStatsPanel();

                // set action panel details
                SetUnitActionPanel();

                break;

            case UIModes.BUILDINGINPROG:
                // set stat panel details
                SetBuildingStatsPanel(true);

                break;

            case UIModes.BUILDING:
                // set action buttons
                SetBuildingActionButtons();

                // set stat panel details
                SetBuildingStatsPanel(false);

                break;

            case UIModes.RESOURCE:
                // update resources remaining in UI
                UpdateResourceRemainingDetails();
                break;
        }
    }

    #region Buildings

    private void ShowBuildingPanels(bool show)
    {
        buildingCanvas.SetActive(show);
        buildingGraphicPanel.SetActive(show);
        buildingStatsPanel.SetActive(show);
    }

    void ShowBuildingActionPanels(bool show)
    {
        if (show && currentBuilding.building.actions.Count > 0)
        {
            buildingActionPanel.SetActive(true);
        } else
        {
            buildingActionPanel.SetActive(false);
        }
    }

    private void SetBuildingActionButtons()
    {
        if (currentBuilding.building.actions.Count > 0)
        {
            foreach (BaseAction action in currentBuilding.building.actions)
            {
                // set action buttons in panel
            }
        }
    }


    private void SetBuildingStatsPanel(bool isBiP)
    {
        if (isBiP) // build in progress
        {
            buildingStatsPanel.transform.Find("DurabilitySlider").gameObject.SetActive(false);
            buildingStatsPanel.transform.Find("DurabilityText").gameObject.SetActive(false);
            buildingStatsPanel.transform.Find("ProgressSlider").gameObject.SetActive(true);
            buildingStatsPanel.transform.Find("ProgressText").gameObject.SetActive(true);

            // progress
            GetTextComp(buildingStatsPanel.transform.Find("ProgressText")).text = Mathf.RoundToInt(currentBip.progress).ToString() + "%";
            GetSlider(buildingStatsPanel.transform.Find("ProgressSlider")).fillAmount = currentBip.progress / 100.0f;

            // level
            GetTextComp(buildingStatsPanel.transform.Find("LevelText")).text = currentBip.building.level.ToString();

            // depot icon
            if (currentBip.building.depotResource != depotResources.NA)
            {
                switch (currentBip.building.depotResource)
                {
                    case depotResources.FOOD:
                        buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = foodResourceIcon;
                        break;
                    case depotResources.ORE:
                        buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = oreResourceIcon;
                        break;
                    case depotResources.WOOD:
                        buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = woodResourceIcon;
                        break;
                    case depotResources.ALL:
                        buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = allResourceIcon;
                        break;
                }
            }

            // biome
            // nothing yet, going to be revamped
        }
        else // completed building
        {
            buildingStatsPanel.transform.Find("DurabilitySlider").gameObject.SetActive(true);
            buildingStatsPanel.transform.Find("DurabilityText").gameObject.SetActive(true);
            buildingStatsPanel.transform.Find("ProgressSlider").gameObject.SetActive(false);
            buildingStatsPanel.transform.Find("ProgressText").gameObject.SetActive(false);

            // durability
            GetTextComp(buildingStatsPanel.transform.Find("DurabilityText")).text = currentBuilding.building.currentDurability.ToString() + "/" + currentBuilding.building.maxDurability.ToString();
            GetSlider(buildingStatsPanel.transform.Find("DurabilitySlider")).fillAmount = currentBuilding.building.currentDurability / currentBuilding.building.maxDurability;
            
            // level
            GetTextComp(buildingStatsPanel.transform.Find("LevelText")).text = currentBuilding.building.level.ToString();

            // depot icon
            if (currentBuilding.building.depotResource != depotResources.NA)
            {
                switch (currentBuilding.building.depotResource)
                {
                    case depotResources.FOOD:
                        buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = foodResourceIcon;
                        break;
                    case depotResources.ORE:
                        buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = oreResourceIcon;
                        break;
                    case depotResources.WOOD:
                        buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = woodResourceIcon;
                        break;
                    case depotResources.ALL:
                        buildingStatsPanel.transform.Find("DepotResourceIcon").GetComponent<Image>().sprite = allResourceIcon;
                        break;
                }
            }

            // biome
            // nothing yet, going to be revamped
        }
    }

    private void SetBuildingGraphicPanel(bool isBiP)
    {
        if (isBiP) // build in progress
        {
            string name;
            name = currentBip.building.name;
            name = UppercaseFirstAndAfterSpaces(name);
            GetTextComp(buildingGraphicPanel.transform.Find("Name")).text = name;

            buildingGraphicPanel.transform.Find("Graphic").GetComponent<Image>().sprite = currentBip.building.icon;
        } else // completed building
        {
            string name;
            name = currentBuilding.building.name;
            name = UppercaseFirstAndAfterSpaces(name);
            GetTextComp(buildingGraphicPanel.transform.Find("Name")).text = name;

            buildingGraphicPanel.transform.Find("Graphic").GetComponent<Image>().sprite = currentBuilding.building.icon;
        }
    }

    #endregion

    #region Units
    void GenerateMultipleUnitsPanel()
    {
        int selectedUnitsCount = selectedUnits.Count;
        float prefabWidth = multiUnitsButton.GetComponent<RectTransform>().rect.width;

        Transform spacer = multiUnitsPanel.transform.Find("MultiUnitsSpacer");

        // set width of rect transform of multiUnitsPanel
        // edge space + (units spacing * unit count) + (count * prefab width)
        float newWidth = (selectedUnitsUIEdgeSpace + (multiUnitsPanelSpacing * selectedUnitsCount) + (prefabWidth * selectedUnitsCount));

        RectTransform rt = multiUnitsPanel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(newWidth, rt.rect.height);

        // clear old units in spacer
        foreach (Transform child in spacer)
        {
            Destroy(child.gameObject);
        }

        // generate buttons and set them as children of spacer
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            GameObject newButton = Instantiate(multiUnitsButton, spacer);
            newButton.name = i + ") UnitMultiSelectButton";
            newButton.GetComponent<Image>().sprite = selectedUnits[i].GetFaceGraphic();
            newButton.GetComponent<MultiUnitsSelectionButton>().unit = selectedUnits[i];
        }
    }

    void SetUnitGraphicPanel()
    {
        string name = string.Empty;

        if (GetVillagerUnit(currentUnit))
        {
            name = GetVillagerUnit(currentUnit).villagerClass.ToString();
            name = UppercaseFirstAndAfterSpaces(name);
        }

        GetTextComp(unitGraphicPanel.transform.Find("Name")).text = name;

        unitGraphicPanel.transform.Find("Graphic").GetComponent<Image>().sprite = currentUnit.GetFaceGraphic();
    }

    void SetUnitStatsPanel()
    {
        if (GetVillagerUnit(currentUnit))
        {
            // Level
            GetTextComp(unitStatsPanel.transform.Find("LevelText")).text = currentUnit.GetLevel().ToString();

            // exp
            GetTextComp(unitStatsPanel.transform.Find("EXPText")).text = currentUnit.GetEXP() + "/" + currentUnit.GetExpToNextLevel();
            GetSlider(unitStatsPanel.transform.Find("EXPSlider")).fillAmount = (float)currentUnit.GetEXP() / (float)currentUnit.GetExpToNextLevel();

            // HP
            GetTextComp(unitStatsPanel.transform.Find("HPText")).text = currentUnit.GetHP() + "/" + currentUnit.GetMaxHP();
            GetSlider(unitStatsPanel.transform.Find("HPSlider")).fillAmount = (float)currentUnit.GetHP() / (float)currentUnit.GetMaxHP();

            // MP
            if (currentUnit.usesEnergy)
            {
                unitStatsPanel.transform.Find("MPIconFrame").gameObject.SetActive(false);
                unitStatsPanel.transform.Find("MPSlider").gameObject.SetActive(false);
                unitStatsPanel.transform.Find("MPText").gameObject.SetActive(false);

                unitStatsPanel.transform.Find("EnergyIconFrame").gameObject.SetActive(true);
                unitStatsPanel.transform.Find("EnergySlider").gameObject.SetActive(true);
                unitStatsPanel.transform.Find("EnergyText").gameObject.SetActive(true);

                GetTextComp(unitStatsPanel.transform.Find("EnergyText")).text = currentUnit.GetMP() + "/" + currentUnit.GetMaxMP();
                GetSlider(unitStatsPanel.transform.Find("EnergySlider")).fillAmount = (float)currentUnit.GetMP() / (float)currentUnit.GetMaxMP();
            }
            else
            {
                unitStatsPanel.transform.Find("MPIconFrame").gameObject.SetActive(true);
                unitStatsPanel.transform.Find("MPSlider").gameObject.SetActive(true);
                unitStatsPanel.transform.Find("MPText").gameObject.SetActive(true);

                unitStatsPanel.transform.Find("EnergyIconFrame").gameObject.SetActive(false);
                unitStatsPanel.transform.Find("EnergySlider").gameObject.SetActive(false);
                unitStatsPanel.transform.Find("EnergyText").gameObject.SetActive(false);

                GetTextComp(unitStatsPanel.transform.Find("MPText")).text = currentUnit.GetMP() + "/" + currentUnit.GetMaxMP();
                GetSlider(unitStatsPanel.transform.Find("MPSlider")).fillAmount = (float)currentUnit.GetMP() / (float)currentUnit.GetMaxMP();
            }

            // Strength
            GetTextComp(unitStatsPanel.transform.Find("StatIconSpacer/Strength/StrengthText")).text = currentUnit.GetStrength().ToString();

            // Stamina
            GetTextComp(unitStatsPanel.transform.Find("StatIconSpacer/Stamina/StaminaText")).text = currentUnit.GetStamina().ToString();

            // Agility
            GetTextComp(unitStatsPanel.transform.Find("StatIconSpacer/Agility/AgilityText")).text = currentUnit.GetAgility().ToString();

            // Luck
            GetTextComp(unitStatsPanel.transform.Find("StatIconSpacer/Luck/LuckText")).text = currentUnit.GetLuck().ToString();

            // Intelligence
            GetTextComp(unitStatsPanel.transform.Find("StatIconSpacer/Intelligence/IntelligenceText")).text = currentUnit.GetIntelligence().ToString();

            // Willpower
            GetTextComp(unitStatsPanel.transform.Find("StatIconSpacer/Willpower/WillpowerText")).text = currentUnit.GetWillpower().ToString();

            // Movement
            GetTextComp(unitStatsPanel.transform.Find("StatIconSpacer/Movement/MovementText")).text = currentUnit.GetMovement().ToString();

            // Biome
            currentUnit.GetCurrentBiomeTile();

            if (currentUnit.GetBiome())
            {
                GetTextComp(unitStatsPanel.transform.Find("BiomeText")).text = UppercaseFirstAndAfterSpaces(currentUnit.GetBiome().biomeType.ToString());
            }
            else
            {
                GetTextComp(unitStatsPanel.transform.Find("BiomeText")).text = string.Empty;
            }
        }
        else
        {
            Debug.LogError(currentUnit.gameObject.name + " is not a villager unit!");
        }
    }

    void SetUnitActionPanel()
    {
        if (GetVillagerUnit(currentUnit))
        {
            if ((GetVillagerUnit(currentUnit).villagerClass == villagerClasses.FARMER ||
                GetVillagerUnit(currentUnit).villagerClass == villagerClasses.LUMBERJACK ||
                GetVillagerUnit(currentUnit).villagerClass == villagerClasses.MINER ||
                GetVillagerUnit(currentUnit).villagerClass == villagerClasses.VILLAGER) &&
                !GetVillagerUnit(currentUnit).buildTaskIsActive)
            {
                // Update gathering bar if gathering class
                if ((GetVillagerUnit(currentUnit).gatherTaskIsActive || GetVillagerUnit(currentUnit).resourcesHolding > 0) && !GetVillagerUnit(currentUnit).buildTaskIsActive)
                {
                    GetTextComp(unitActionPanel.transform.Find("ActionText")).text = GetVillagerUnit(currentUnit).resourcesHolding + "/" + GetVillagerUnit(currentUnit).GetCarryLimit();

                    GetSlider(unitActionPanel.transform.Find("ActionSlider")).fillAmount = (float)GetVillagerUnit(currentUnit).resourcesHolding / GetVillagerUnit(currentUnit).GetCarryLimit();
                    GetSlider(unitActionPanel.transform.Find("ActionTimeSlider")).fillAmount = (float)GetVillagerUnit(currentUnit).gatherTimeElapsed / GetVillagerUnit(currentUnit).GetGatherTime();

                    if (!unitActionPanel.transform.Find("ActionSlider").gameObject.activeInHierarchy)
                    {
                        unitActionPanel.transform.Find("ActionSlider").gameObject.SetActive(true);
                        unitActionPanel.transform.Find("ActionText").gameObject.SetActive(true);
                    }
                    if (!unitActionPanel.transform.Find("ActionTimeSlider").gameObject.activeInHierarchy)
                    {
                        unitActionPanel.transform.Find("ActionTimeSlider").gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (unitActionPanel.transform.Find("ActionSlider").gameObject.activeInHierarchy)
                    {
                        unitActionPanel.transform.Find("ActionSlider").gameObject.SetActive(false);
                        unitActionPanel.transform.Find("ActionText").gameObject.SetActive(false);
                        unitActionPanel.transform.Find("ActionTimeSlider").gameObject.SetActive(false);
                    }
                }
            }

            if ((GetVillagerUnit(currentUnit).villagerClass == villagerClasses.BUILDER ||
                      GetVillagerUnit(currentUnit).villagerClass == villagerClasses.VILLAGER) &&
                      !GetVillagerUnit(currentUnit).gatherTaskIsActive)
            {
                if (GetVillagerUnit(currentUnit).buildTaskIsActive)
                {
                    GetTextComp(unitActionPanel.transform.Find("ActionText")).text = GetVillagerUnit(currentUnit).personalBuildProgress + "%";

                    GetSlider(unitActionPanel.transform.Find("ActionSlider")).fillAmount = GetVillagerUnit(currentUnit).personalBuildProgress / 100;
                    GetSlider(unitActionPanel.transform.Find("ActionTimeSlider")).fillAmount = GetVillagerUnit(currentUnit).buildTimeElapsed / GetVillagerUnit(currentUnit).GetBuildTime();

                    if (!unitActionPanel.transform.Find("ActionSlider").gameObject.activeInHierarchy)
                    {
                        unitActionPanel.transform.Find("ActionSlider").gameObject.SetActive(true);
                        unitActionPanel.transform.Find("ActionText").gameObject.SetActive(true);
                    }
                    if (!unitActionPanel.transform.Find("ActionTimeSlider").gameObject.activeInHierarchy)
                    {
                        unitActionPanel.transform.Find("ActionTimeSlider").gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (unitActionPanel.transform.Find("ActionSlider").gameObject.activeInHierarchy)
                    {
                        unitActionPanel.transform.Find("ActionSlider").gameObject.SetActive(false);
                        unitActionPanel.transform.Find("ActionText").gameObject.SetActive(false);
                        unitActionPanel.transform.Find("ActionTimeSlider").gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            Debug.LogError(currentUnit.gameObject.name + " is not a villager unit!");
        }
    }

    private void SetUnitActionButtons()
    {
        foreach (Transform child in actionSpacer.transform)
        {
            Destroy(child.gameObject);
        }
        if (GetVillagerUnit(currentUnit))
        {
            if (GetVillagerUnit(currentUnit).villagerClass == villagerClasses.BUILDER ||
                GetVillagerUnit(currentUnit).villagerClass == villagerClasses.VILLAGER)
            {
                // for each builder action in BuilderActions

                foreach (BaseAction action in builderActions.builderActions)
                {
                    if (currentUnit.GetLevel() >= action.levelRequired)
                    {
                        // Add action button for 'Build'

                        // prepare action button
                        GameObject buildActionGO = GameObject.Instantiate(actionButton) as GameObject;
                        buildActionGO.transform.SetParent(actionSpacer.transform, false);

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
                        ba.unit = currentUnit;
                    }
                }
            }

            if (GetVillagerUnit(currentUnit).villagerClass == villagerClasses.FARMER ||
                    GetVillagerUnit(currentUnit).villagerClass == villagerClasses.LUMBERJACK ||
                    GetVillagerUnit(currentUnit).villagerClass == villagerClasses.MINER ||
                    GetVillagerUnit(currentUnit).villagerClass == villagerClasses.VILLAGER)
            {

                // for each gatherer action in GathererActions
                foreach (BaseAction action in gathererActions.gathererActions)
                {
                    if (currentUnit.GetLevel() >= action.levelRequired)
                    {
                        // Add action button for 'Gather'

                        // prepare action button
                        GameObject gatherActionGO = GameObject.Instantiate(actionButton) as GameObject;
                        gatherActionGO.transform.SetParent(actionSpacer.transform, false);

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
                        ga.unit = currentUnit;
                    }
                }

            }
        }
    }

    public void SetCurrentUnit(Unit unitToSet)
    {
        currentUnit = unitToSet;
    }

    public Unit GetUnitSelected(int ID)
    {
        Debug.Log("returning unit: " + selectedUnits[ID].gameObject.name);
        return selectedUnits[ID];
    }

    public void ShowUnitPanels(bool show)
    {
        unitCanvas.SetActive(show);
        unitGraphicPanel.SetActive(show);
        unitActionPanel.SetActive(show);
        unitStatsPanel.SetActive(show);
    }

    public void ShowMultipleUnitsPanel(bool show)
    {
        multiUnitsPanel.SetActive(show);
    }
    #endregion

    #region Resources

    void SetResourceGraphicPanel()
    {
        GetTextComp(resourceGraphicPanel.transform.Find("Name")).text = currentResource.name;

        resourceGraphicPanel.transform.Find("Graphic").GetComponent<Image>().sprite = currentResource.icon;
    }

    void SetResourceStatsPanel()
    {
        switch (currentResource.resourceType)
        {
            case ResourceTypes.WOOD:
                resourceStatsPanel.transform.Find("Graphic").GetComponent<Image>().sprite = woodResourceIcon;
                break;
            case ResourceTypes.ORE:
                resourceStatsPanel.transform.Find("Graphic").GetComponent<Image>().sprite = oreResourceIcon;
                break;
            case ResourceTypes.FOOD:
                resourceStatsPanel.transform.Find("Graphic").GetComponent<Image>().sprite = foodResourceIcon;
                break;
        }

        // set biome
    }

    void UpdateResourceRemainingDetails()
    {
        GetTextComp(resourceStatsPanel.transform.Find("RemainingResourcesText")).text = currentResource.resourcesRemaining + "/" + currentResource.totalResources;
        GetSlider(resourceStatsPanel.transform.Find("RemainingResourcesSlider")).fillAmount = (float)currentResource.resourcesRemaining / (float)currentResource.totalResources;
    }

    public void ShowResourcePanels(bool show)
    {
        resourceCanvas.SetActive(show);
        resourceGraphicPanel.SetActive(show);
        resourceStatsPanel.SetActive(show);
    }

    #endregion

    public void ButtonUIProcessing(GameObject ab)
    {
        StartCoroutine(ActionIconFade(ab));
    }

    IEnumerator ActionIconFade(GameObject actionButton)
    {
        Image icon = actionButton.transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>();
        Color originColor = icon.color;

        if (actionButton.GetComponent<BuildAction>())
        {
            while (buildActionClicked)
            {
                ProcessFade(icon);

                yield return null;
            }
        } else if (actionButton.GetComponent<BuildingAction>())
        {
            while (buildingActionClicked)
            {
                ProcessFade(icon);

                yield return null;
            }
        } else if (actionButton.GetComponent<GatherAction>())
        {
            while (gatherActionClicked)
            {
                ProcessFade(icon);

                yield return null;
            }
        }

        ResetIconAlpha(originColor, icon);
    }

    private void ProcessFade(Image icon)
    {
        float newAlpha = Mathf.Lerp(actionButtonFadeMin, 255, Mathf.PingPong((Time.time * actionButtonFadeSpeed), 1));
        Color newColor = icon.color;
        newColor.a = (newAlpha / 255.0f);
        icon.color = newColor;
    }

    public IEnumerator HighlightConfirmedResource(Outline outline)
    {
        outline.OutlineWidth = taskConfirmationOutlineWidth;
        outline.enabled = true;
        yield return new WaitForSeconds(taskConfirmationOutlineDuration);
        outline.enabled = false;
    }

    public void HighlightResource(Outline outline, bool highlight)
    {
        outline.OutlineWidth = taskConfirmationOutlineWidth;
        outline.enabled = highlight;
    }

    /*IEnumerator ActionIconFade(GameObject actionButton)
    {
        Image icon = actionButton.transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>();
        Color originColor = icon.color;

        while (actionButtonClicked)
        {
            if (!actionButtonFadeBreak)
            {
                float newAlpha = Mathf.Lerp(actionButtonFadeMin, 255, Mathf.PingPong((Time.time * actionButtonFadeSpeed), 1));
                Color newColor = icon.color;
                newColor.a = (newAlpha / 255.0f);
                icon.color = newColor;
            } else
            {
                break;
            }

            yield return null;
        }

        actionButtonFadeBreak = false;
        ResetIconAlpha(originColor, icon);
    }*/

    void ResetIconAlpha(Color originColor, Image icon)
    {
        icon.color = originColor;
    }

    public VillagerUnit GetVillagerUnit(Unit unit)
    {
        VillagerUnit tryVillager = unit as VillagerUnit;

        return tryVillager;
    }

    string UppercaseFirstAndAfterSpaces(string s)
    {
        char tempChar = '\0';

        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }

        char[] a = s.ToCharArray();
        for (int i = 0; i < a.Length; i++)
        {
            a[i] = char.ToLower(a[i]);

            if (i != 0)
            {
                if (tempChar != '\0' && tempChar == ' ')
                {
                    a[i] = char.ToUpper(a[i]);
                }

                tempChar = a[i];
            }
        }

        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }

    TMP_Text GetTextComp (Transform transform)
    {
        return transform.GetComponent<TMP_Text>();
    }

    Image GetSlider(Transform transform)
    {
        return transform.Find("Fill Area/Fill").GetComponent<Image>();
    }
}
