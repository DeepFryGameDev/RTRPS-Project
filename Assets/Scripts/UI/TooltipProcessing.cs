using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipProcessing : MonoBehaviour
{
    [Tooltip("Set to Biome Tooltip to use in UI")]
    [SerializeField] GameObject BiomeTooltip;

    BiomeTile biomeHovered;

    Unit unitHovered;

    Transform camTransform;

    // Start is called before the first frame update
    void Start()
    {
        biomeHovered = new BiomeTile();
        ShowTooltip(BiomeTooltip, false);

        camTransform = Camera.main.transform;

        ShowTooltip(BiomeTooltip, true);
    }

    // Update is called once per frame
    void Update()
    {
        SetBiomeTooltip();

        BiomeTooltipProcessing();        
    }

    void SetBiomeTooltip()
    {
        biomeHovered.SetBiomeID(GetCameraTerrainIntersectionPosition());
    }

    private void BiomeTooltipProcessing()
    {        
        BiomeTooltip.transform.Find("Name").GetComponent<TMP_Text>().text = UppercaseFirstAndAfterSpaces(biomeHovered.name);
        BiomeTooltip.transform.Find("Type").GetComponent<TMP_Text>().text = UppercaseFirstAndAfterSpaces(biomeHovered.primaryBiomeType.ToString());
    }

    Vector3 GetCameraTerrainIntersectionPosition()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hitInfo, 1000.0f))
        {
            return hitInfo.point;
        }

        return new Vector3(0, 0, 0);
    }

    void ShowTooltip(GameObject tooltip, bool show)
    {
        tooltip.SetActive(show);
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
}
