using TMPro;
using UnityEngine;

// used for handling the biome tooltip in UI in accordance with where the camera is looking
public class BiomeTooltipProcessing : MonoBehaviour
{
    UIPrefabManager uipm; // used to modify the Biome Tooltip panel

    BiomeTile biomeHovered; // used to check the biome that the camera is aimed at

    Transform camTransform; // set to transform of the main camera

    void Start()
    {
        uipm = FindObjectOfType<UIPrefabManager>();

        biomeHovered = new BiomeTile();
        camTransform = Camera.main.transform;

        uipm.ShowUIObject(uipm.BiomeTooltipPanel, true); // displays biome tooltip panel
    }

    void Update()
    {
        SetBiomeTooltip(); // sets the BiomeID of the biomeHovered based on index of dominant texture painted on terrain where the center of the camera is pointing

        TooltipProcessing(); // updates the biome tooltip UI panel text to the appropriate values for the biome      
    }

    // sets the BiomeID of the biomeHovered based on index of dominant texture painted on terrain where the center of the camera is pointing
    void SetBiomeTooltip()
    {
        biomeHovered.SetBiomeID(GetCameraTerrainIntersectionPosition());
    }

    // updates the biome tooltip UI panel text to the appropriate values for the biome 
    void TooltipProcessing()
    {        
        uipm.BiomeTooltipPanel.transform.Find("Name").GetComponent<TMP_Text>().text = uipm.UppercaseFirstAndAfterSpaces(biomeHovered.name);
        uipm.BiomeTooltipPanel.transform.Find("Type").GetComponent<TMP_Text>().text = uipm.UppercaseFirstAndAfterSpaces(biomeHovered.primaryBiomeType.ToString());
    }

    // returns the position where the center of where the camera is looking intersects with the terrain
    Vector3 GetCameraTerrainIntersectionPosition()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hitInfo, 1000.0f))
        {
            return hitInfo.point;
        }

        return new Vector3(0, 0, 0);
    }
}
