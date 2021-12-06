using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipProcessing : MonoBehaviour
{
    [SerializeField] GameObject BiomeTooltip;
    [SerializeField] GameObject SelectedUnitTooltip;
    [SerializeField] TMP_Text biomeTooltip_biomeNameText;
    [SerializeField] TMP_Text biomeTooltip_primaryTypeText;
    [SerializeField] TMP_Text biomeTooltip_secondaryTypeText;
    [SerializeField] TMP_Text selectedUnitTooltip_NameText;
    [SerializeField] TMP_Text selectedUnitTooltip_BiomeText;
    [SerializeField] GameObject unitsParent;

    BiomeTile biomeHovered;

    Unit unitHovered;

    Transform camTransform;

    // Start is called before the first frame update
    void Start()
    {
        ShowTooltip(BiomeTooltip, false);
        ShowTooltip(SelectedUnitTooltip, false);

        camTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForUnitDeselect();

        SetUnitTooltip();
        SetBiomeTooltip();

        BiomeTooltipProcessing();
        UnitTooltipProcessing();
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
            unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Count > 0 && Input.GetKeyDown(KeyCode.Mouse0)
            )
        {
            unitsParent.GetComponent<SelectedUnitProcessing>().selectedUnits.Clear();
        }
    }

    void SetUnitTooltip()
    {
        RaycastHit[] hits;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hits = Physics.RaycastAll(ray, 1000);

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("Unit"))
            {
                unitHovered = hit.transform.GetComponent<Unit>();

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    SelectUnit();
                }
            }
        }
    }

    void SetBiomeTooltip()
    {
        RaycastHit[] hits;
        Ray ray = new Ray(camTransform.position, camTransform.forward);
        hits = Physics.RaycastAll(ray, 1000);

        bool biomeFound = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("BiomeTile"))
            {
                biomeFound = true;
                biomeHovered = hit.transform.GetComponent<BiomeTile>();
            }
        }

        if (!biomeFound)
        {
            biomeHovered = null;
            ShowTooltip(BiomeTooltip, false);
        }
    }

    private void BiomeTooltipProcessing()
    {
        if (biomeHovered != null)
        {
            ShowTooltip(BiomeTooltip, true);
            biomeTooltip_biomeNameText.text = biomeHovered.biomeName;
            biomeTooltip_primaryTypeText.text = getPrimaryBiomeType(biomeHovered.biomeType);
            biomeTooltip_secondaryTypeText.text = getSecondaryBiomeType(biomeHovered.biomeType);
        }
    }

    void UnitTooltipProcessing()
    {
        VillagerUnit villagerUnit = unitHovered as VillagerUnit;

        if (villagerUnit != null)
        {

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
