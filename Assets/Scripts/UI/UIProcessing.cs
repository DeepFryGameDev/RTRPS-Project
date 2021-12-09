using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProcessing : MonoBehaviour
{
    [Tooltip("Set this to value for UI Layer - default is 5")]
    public int UILayer;

    [Tooltip("Set to unit graphic panel in UI")]
    [SerializeField] GameObject unitGraphicPanel;
    [Tooltip("Set to unit stats panel in UI")]
    [SerializeField] GameObject unitStatsPanel;
    [Tooltip("Set to unit action panel in UI")]
    [SerializeField] GameObject unitActionPanel;

    List<Unit> selectedUnits;
    Unit currentUnit;

    // Start is called before the first frame update
    void Start()
    {
        selectedUnits = GameObject.Find("Units").GetComponent<SelectedUnitProcessing>().selectedUnits;
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedUnits.Count == 1) // only one unit selected
        {
            currentUnit = selectedUnits[0];
            // set graphic panel details
            SetGraphicPanel();

            // set stat panel details
            SetStatsPanel();

            // set action panel details

            // set biome
            SetBiome();

            // show panel
            ShowPanels(true);

        } else if (selectedUnits.Count > 1) // multiple units selected
        {

        } else // no units selected, hide panels
        {
            ShowPanels(false);
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
    }

    void SetStatsPanel()
    {
        if (GetVillagerUnit(currentUnit))
        {
            // Level
            GetTextComp(unitStatsPanel.transform.Find("LevelText")).text = currentUnit.GetLevel().ToString();

            // exp
            GetTextComp(unitStatsPanel.transform.Find("EXPText")).text = currentUnit.GetEXP() + "/" + currentUnit.GetExpToNextLevel();
            GetSlider(unitStatsPanel.transform.Find("EXPSlider")).fillAmount = currentUnit.GetEXP() / currentUnit.GetExpToNextLevel();

            // HP
            GetTextComp(unitStatsPanel.transform.Find("HPText")).text = currentUnit.GetHP() + "/" + currentUnit.GetMaxHP();
            GetSlider(unitStatsPanel.transform.Find("HPSlider")).fillAmount = currentUnit.GetHP() / currentUnit.GetMaxHP();

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
                GetSlider(unitStatsPanel.transform.Find("EnergySlider")).fillAmount = currentUnit.GetMP() / currentUnit.GetMaxMP(); 
            } else
            {
                unitStatsPanel.transform.Find("MPIconFrame").gameObject.SetActive(true);
                unitStatsPanel.transform.Find("MPSlider").gameObject.SetActive(true);
                unitStatsPanel.transform.Find("MPText").gameObject.SetActive(true);

                unitStatsPanel.transform.Find("EnergyIconFrame").gameObject.SetActive(false);
                unitStatsPanel.transform.Find("EnergySlider").gameObject.SetActive(false);
                unitStatsPanel.transform.Find("EnergyText").gameObject.SetActive(false);

                GetTextComp(unitStatsPanel.transform.Find("MPText")).text = currentUnit.GetMP() + "/" + currentUnit.GetMaxMP();
                GetSlider(unitStatsPanel.transform.Find("MPSlider")).fillAmount = currentUnit.GetMP() / currentUnit.GetMaxMP();
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
        }
    }

    void SetBiome()
    {
        if (currentUnit.GetBiome())
        {
            GetTextComp(unitStatsPanel.transform.Find("BiomeText")).text = UppercaseFirst(currentUnit.GetBiome().biomeType.ToString());
        } else
        {
            GetTextComp(unitStatsPanel.transform.Find("BiomeText")).text = string.Empty;
        }        
    }

    void ShowPanels(bool show)
    {
        unitGraphicPanel.SetActive(show);
        unitActionPanel.SetActive(show);
        unitStatsPanel.SetActive(show);
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
