using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipProcessing : MonoBehaviour
{
    [SerializeField] GameObject BiomeTooltip;
    [SerializeField] GameObject HoveredUnitTooltip;
    [SerializeField] GameObject SelectedUnitTooltip;
    [SerializeField] TMP_Text biomeTooltip_biomeNameText;
    [SerializeField] TMP_Text biomeTooltip_primaryTypeText;
    [SerializeField] TMP_Text biomeTooltip_secondaryTypeText;
    [SerializeField] TMP_Text hoveredUnitTooltip_NameText;
    [SerializeField] TMP_Text hoveredUnitTooltip_BiomeText;
    [SerializeField] TMP_Text selectedUnitTooltip_NameText;
    [SerializeField] TMP_Text selectedUnitTooltip_BiomeText;
    [SerializeField] GameObject unitsParent;

    enum tooltipModes
    {
        IDLE,
        BIOME,
        UNIT,
        UNITSELECTEDONLY,
        UNITSELECTEDUNITHOVERED,
        UNITSELECTEDBIOMEHOVERED
    }

    tooltipModes tooltipMode;

    BiomeTile biomeHovered;
    Unit unitHovered;

    // Start is called before the first frame update
    void Start()
    {
        ShowTooltip(BiomeTooltip, false);
        ShowTooltip(HoveredUnitTooltip, false);
        ShowTooltip(SelectedUnitTooltip, false);
    }

    // Update is called once per frame
    void Update()
    {
        CheckForUnitDeselect();

        SetTooltip();
        BiomeTooltipProcessing();
        UnitTooltipProcessing();
        ShowHideTooltips();
    }

    void SelectUnit()
    {
        unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Clear();

        unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Add(unitHovered);
        selectedUnitTooltip_BiomeText.text = getPrimaryBiomeType(biomeHovered.biomeType);
        VillagerUnit villagerUnit = unitHovered as VillagerUnit;

        if (villagerUnit != null)
        {
            selectedUnitTooltip_NameText.text = villagerUnit.villagerClass.ToString();
        }
    }

    void CheckForUnitDeselect()
    {
        if (unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Count > 0 && Input.GetKeyDown(KeyCode.Escape) ||
            tooltipMode == tooltipModes.UNITSELECTEDBIOMEHOVERED && unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Count > 0 && Input.GetKeyDown(KeyCode.Mouse0)
            )
        {
            unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Clear();
        }
    }

    void SetTooltip()
    {
        bool hittingBiomeTile = false;
        bool hittingUnit = false;

        RaycastHit[] hits;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hits = Physics.RaycastAll(ray, 1000);

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("BiomeTile"))
            {
                hittingBiomeTile = true;
                biomeHovered = hit.transform.GetComponent<BiomeTile>();
            }
            if (hit.transform.gameObject.CompareTag("Unit"))
            {
                hittingUnit = true;
                unitHovered = hit.transform.GetComponent<Unit>();

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    SelectUnit();
                }
            }
        }

        if (hittingBiomeTile && !hittingUnit && unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Count == 0)
        {
            tooltipMode = tooltipModes.BIOME;
        }
        else if (hittingBiomeTile && !hittingUnit && unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Count > 0)
        {
            tooltipMode = tooltipModes.UNITSELECTEDBIOMEHOVERED;
        }
        else if (hittingUnit && !unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Contains(unitHovered))
        {
            tooltipMode = tooltipModes.UNIT;
        }
        else if (!hittingBiomeTile && !hittingUnit && unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Count > 0)
        {
            tooltipMode = tooltipModes.UNITSELECTEDONLY;
        }
        else if (hittingUnit && unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Count > 0 && !unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Contains(unitHovered))
        {
            tooltipMode = tooltipModes.UNITSELECTEDUNITHOVERED;
        }
        else if (hittingUnit && unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Count > 0 && unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Contains(unitHovered))
        {
            tooltipMode = tooltipModes.UNITSELECTEDBIOMEHOVERED;
        }
        else
        {
            tooltipMode = tooltipModes.IDLE;
        }
    }

    private void BiomeTooltipProcessing()
    {
        if (tooltipMode == tooltipModes.BIOME || tooltipMode == tooltipModes.UNITSELECTEDBIOMEHOVERED)
        {
            biomeTooltip_biomeNameText.text = biomeHovered.biomeName;
            biomeTooltip_primaryTypeText.text = getPrimaryBiomeType(biomeHovered.biomeType);
            biomeTooltip_secondaryTypeText.text = getSecondaryBiomeType(biomeHovered.biomeType);
        }
    }

    void UnitTooltipProcessing()
    {
        if (tooltipMode == tooltipModes.UNIT || tooltipMode == tooltipModes.UNITSELECTEDUNITHOVERED)
        {
            hoveredUnitTooltip_BiomeText.text = biomeHovered.biomeType.ToString();

            VillagerUnit villagerUnit = unitHovered as VillagerUnit;

            if (villagerUnit != null)
            {
                hoveredUnitTooltip_NameText.text = villagerUnit.villagerClass.ToString();
            }
        }
    }

    void ShowHideTooltips()
    {
        //Debug.Log(tooltipMode);

        switch (tooltipMode)
        {
            case tooltipModes.IDLE:
                ShowTooltip(BiomeTooltip, false);
                ShowTooltip(HoveredUnitTooltip, false);
                ShowTooltip(SelectedUnitTooltip, false);
                break;
            case tooltipModes.BIOME:
                ShowTooltip(BiomeTooltip, true);
                ShowTooltip(HoveredUnitTooltip, false);
                ShowTooltip(SelectedUnitTooltip, false);
                break;
            case tooltipModes.UNIT:
                ShowTooltip(BiomeTooltip, false);
                ShowTooltip(HoveredUnitTooltip, true);
                //ShowTooltip(SelectedUnitTooltip, false);
                break;
            case tooltipModes.UNITSELECTEDONLY:
                ShowTooltip(BiomeTooltip, false);
                //ShowTooltip(HoveredUnitTooltip, true);
                ShowTooltip(SelectedUnitTooltip, true);
                break;
            case tooltipModes.UNITSELECTEDBIOMEHOVERED:
                ShowTooltip(BiomeTooltip, true);
                ShowTooltip(HoveredUnitTooltip, false);
                ShowTooltip(SelectedUnitTooltip, true);
                break;
        }
    }

    void ShowTooltip(GameObject tooltip, bool show)
    {
        tooltip.SetActive(show);
    }

    string getPrimaryBiomeType(BiomeTypes biomeType)
    {
        switch (biomeType)
        {
            case BiomeTypes.PLAINS:
                return "Plains";
            case BiomeTypes.PLAINSHILL:
                return "Plains";
            case BiomeTypes.OCEAN:
                return "Ocean";
            case BiomeTypes.BEACH:
                return "Beach";
            default:
                return string.Empty;
        }        
    }

    string getSecondaryBiomeType(BiomeTypes biomeType)
    {
        switch (biomeType)
        {
            case BiomeTypes.PLAINSHILL:
                return "Hill";
            default:
                return string.Empty;
        }
    }
}
