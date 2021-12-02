using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipProcessing : MonoBehaviour
{
    [SerializeField] GameObject BiomeTooltip;
    [SerializeField] TMP_Text biomeNameText;
    [SerializeField] TMP_Text primaryTypeText;
    [SerializeField] TMP_Text secondaryTypeText;

    // Start is called before the first frame update
    void Start()
    {
        ShowTooltip(false);
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction, Color.blue);

        if (Physics.Raycast(ray, out hit, 1000) && hit.transform.gameObject.CompareTag("BiomeTile"))
        {
            biomeNameText.text = hit.transform.gameObject.GetComponent<BiomeTile>().biomeName;
            primaryTypeText.text = getPrimaryBiomeType(hit.transform.gameObject.GetComponent<BiomeTile>().biomeType);
            secondaryTypeText.text = getSecondaryBiomeType(hit.transform.gameObject.GetComponent<BiomeTile>().biomeType);
            ShowTooltip(true);
            //Debug.Log("hitting " + hit.transform.name);
        } else
        {
            ShowTooltip(false);

            /*Debug.Log("hitting " + ray.origin);

            string test = hit.transform.gameObject.name;
            if (test == null || test == string.Empty)
            {
                Debug.Log("hitting nothing");
            } else
            {
                Debug.Log(hit.transform.gameObject.name);
            }*/
        }
    }

    void ShowTooltip(bool show)
    {
        BiomeTooltip.SetActive(show);
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
