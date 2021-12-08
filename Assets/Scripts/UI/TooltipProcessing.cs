using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipProcessing : MonoBehaviour
{
    [SerializeField] GameObject BiomeTooltip;

    BiomeTile biomeHovered;

    Unit unitHovered;

    Transform camTransform;

    // Start is called before the first frame update
    void Start()
    {
        ShowTooltip(BiomeTooltip, false);

        camTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        SetBiomeTooltip();

        BiomeTooltipProcessing();
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
            BiomeTooltip.transform.Find("Name").GetComponent<TMP_Text>().text = biomeHovered.biomeName;
            BiomeTooltip.transform.Find("Type").GetComponent<TMP_Text>().text = getPrimaryBiomeType(biomeHovered.biomeType);
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
}
