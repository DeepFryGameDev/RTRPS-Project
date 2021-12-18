using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProcessing : MonoBehaviour
{
    [Tooltip("Set this to value for UI Layer - default is 5")]
    public int UILayer;

    public int selectedUnitsMax = 20;
    public float selectedUnitsUIEdgeSpace = 10;
    [Range(1, 10)] public float hoveredUnitsOutlineWidth = 8;
    [Range(1, 50)] public int selectedUnitDoubleClickFrameBuffer = 10;

    [Tooltip("Set to unit graphic panel in UI")]
    [SerializeField] GameObject unitGraphicPanel;
    [Tooltip("Set to unit stats panel in UI")]
    [SerializeField] GameObject unitStatsPanel;
    [Tooltip("Set to unit action panel in UI")]
    [SerializeField] GameObject unitActionPanel;
    [Tooltip("Set to multiple units panel in UI")]
    [SerializeField] GameObject multiUnitsPanel;
    [Tooltip("Set to multiple units 'button' prefab")]
    [SerializeField] GameObject multiUnitsButton;

    [Tooltip("Set to icon used for wood resource in UI")]
    public Sprite woodResourceIcon;
    [Tooltip("Set to icon used for ore resource in UI")]
    public Sprite oreResourceIcon;
    [Tooltip("Set to icon used for food resource in UI")]
    public Sprite foodResourceIcon;
    [Tooltip("Set to icon used for gold resource in UI")]
    public Sprite goldResourceIcon;

    [HideInInspector] public bool resetUI;
    bool uiCleared = false;

    List<Unit> selectedUnits;
    Unit currentUnit;
    float multiUnitsPanelSpacing;

    // Start is called before the first frame update
    void Start()
    {
        selectedUnits = FindObjectOfType<SelectedUnitProcessing>().selectedUnits;

        transform.Find("Canvas/TopBar/ResourceSpacer/Lumber/Icon").GetComponent<Image>().sprite = woodResourceIcon;
        transform.Find("Canvas/TopBar/ResourceSpacer/Ore/Icon").GetComponent<Image>().sprite = oreResourceIcon;
        transform.Find("Canvas/TopBar/ResourceSpacer/Food/Icon").GetComponent<Image>().sprite = foodResourceIcon;
        transform.Find("Canvas/TopBar/ResourceSpacer/Gold/Icon").GetComponent<Image>().sprite = goldResourceIcon;

        HorizontalLayoutGroup horizontalLayoutGroup = multiUnitsPanel.transform.Find("MultiUnitsSpacer").GetComponent<HorizontalLayoutGroup>();
        multiUnitsPanelSpacing = horizontalLayoutGroup.spacing;
    }

    private void Update()
    {
        UpdateUI();
    }

    public void SetCurrentUnit(Unit unitToSet)
    {
        currentUnit = unitToSet;
    }

    void UpdateUI()
    {
        if (resetUI && selectedUnits.Count == 1) // Only need to be run once
        {
            resetUI = false;
            uiCleared = false;

            // set graphic panel details
            SetGraphicPanel();

            // show panel
            ShowPanels(true);

            //Make sure multiple units panel is gone
            ShowMultipleUnitsPanel(false);
        }

        if (resetUI && selectedUnits.Count > 1) // Only need to be run once
        {
            resetUI = false;
            uiCleared = false;

            // set graphic panel details
            SetGraphicPanel();

            // also show multiple units panel
            if (!multiUnitsPanel.activeInHierarchy)
            {
                GenerateMultipleUnitsPanel();

                // show panels
                ShowMultipleUnitsPanel(true);
                ShowPanels(true);
            }            
        }

        if (selectedUnits.Count >= 1) // for things that need to be consistently updated
        {
            // set stat panel details
            SetStatsPanel();

            // set action panel details
            SetActionPanel();

            // set biome
            SetBiome();
        }

        if (!uiCleared && selectedUnits.Count == 0)
        {
            uiCleared = true;
            ShowPanels(false);
            ShowMultipleUnitsPanel(false);
        }
    }

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

    void SetGraphicPanel()
    {
        string name = string.Empty;

        if (GetVillagerUnit(currentUnit))
        {
            name = GetVillagerUnit(currentUnit).villagerClass.ToString();
            name = UppercaseFirst(name);
        }

        GetTextComp(unitGraphicPanel.transform.Find("Name")).text = name;

        unitGraphicPanel.transform.Find("Graphic").GetComponent<Image>().sprite = currentUnit.GetFaceGraphic();
    }

    void SetStatsPanel()
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
            } else
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
        } else
        {
            Debug.LogError(currentUnit.gameObject.name + " is not a villager unit!");
        }
    }

    void SetActionPanel()
    {
        if (GetVillagerUnit(currentUnit))
        {
            if (GetVillagerUnit(currentUnit).gatherTaskIsActive || GetVillagerUnit(currentUnit).resourcesHolding > 0)
            {
                // Update the Progress Bar for Gathering
                GetTextComp(unitActionPanel.transform.Find("ActionText")).text = GetVillagerUnit(currentUnit).resourcesHolding + "/" + GetVillagerUnit(currentUnit).GetCarryLimit();
                GetSlider(unitActionPanel.transform.Find("ActionSlider")).fillAmount = (float)GetVillagerUnit(currentUnit).resourcesHolding / GetVillagerUnit(currentUnit).GetCarryLimit();

                if (!unitActionPanel.transform.Find("ActionSlider").gameObject.activeInHierarchy)
                {
                    unitActionPanel.transform.Find("ActionSlider").gameObject.SetActive(true);
                    unitActionPanel.transform.Find("ActionText").gameObject.SetActive(true);
                }
            } else
            {
                if (unitActionPanel.transform.Find("ActionSlider").gameObject.activeInHierarchy)
                {
                    unitActionPanel.transform.Find("ActionSlider").gameObject.SetActive(false);
                    unitActionPanel.transform.Find("ActionText").gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogError(currentUnit.gameObject.name + " is not a villager unit!");
        }
    }

    void SetBiome()
    {
        currentUnit.GetCurrentBiomeTile();

        if (currentUnit.GetBiome())
        {
            GetTextComp(unitStatsPanel.transform.Find("BiomeText")).text = UppercaseFirst(currentUnit.GetBiome().biomeType.ToString());
        } else
        {
            GetTextComp(unitStatsPanel.transform.Find("BiomeText")).text = string.Empty;
        }        
    }

    public Unit GetUnitSelected(int ID)
    {
        Debug.Log("returning unit: " + selectedUnits[ID].gameObject.name);
        return selectedUnits[ID];
    }

    public void ShowPanels(bool show)
    {
        unitGraphicPanel.SetActive(show);
        unitActionPanel.SetActive(show);
        unitStatsPanel.SetActive(show);
    }

    public void ShowMultipleUnitsPanel(bool show)
    {
        multiUnitsPanel.SetActive(show);
    }

    VillagerUnit GetVillagerUnit(Unit unit)
    {
        VillagerUnit tryVillager = unit as VillagerUnit;

        return tryVillager;
    }

    string UppercaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }

        char[] a = s.ToCharArray();
        for (int i = 0; i < a.Length; i++)
        {
            a[i] = char.ToLower(a[i]);
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
