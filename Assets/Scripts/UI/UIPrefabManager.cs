using UnityEngine;

// This script contains public variables to store all UI elements to be used/manipulated by other scripts, as well as methods for making small changes to UI objects
public class UIPrefabManager : MonoBehaviour
{
    [Tooltip("Set to unit canvas in UI")]
    public GameObject unitCanvas;
    [Tooltip("Set to resource canvas in UI")]
    public GameObject resourceCanvas;
    [Tooltip("Set to resource spacer in top bar in UI")]
    public GameObject resourceUISpacer;
    [Tooltip("Set to building canvas in UI")]
    public GameObject buildingCanvas;
    [Tooltip("Set to unit graphic panel in UI")]
    public GameObject unitGraphicPanel;
    [Tooltip("Set to unit stats panel in UI")]
    public GameObject unitStatsPanel;
    [Tooltip("Set to unit action panel in UI")]
    public GameObject unitActionPanel;
    [Tooltip("Set to resource graphic panel in UI")]
    public GameObject resourceGraphicPanel;
    [Tooltip("Set to resource stats panel in UI")]
    public GameObject resourceStatsPanel;
    [Tooltip("Set to building graphic panel in UI")]
    public GameObject buildingGraphicPanel;
    [Tooltip("Set to building stats panel in UI")]
    public GameObject buildingStatsPanel;
    [Tooltip("Set to building action panel in UI")]
    public GameObject buildingActionPanel;
    [Tooltip("Set to building action queue panel in UI")]
    public GameObject buildingActionQueuePanel;
    [Tooltip("Set to multiple units panel in UI")]
    public GameObject multiUnitsPanel;
    [Tooltip("Set to options in UI")]
    public GameObject optionsPanel;

    [Tooltip("Set to multiple units 'button' prefab")]
    public GameObject multiUnitsButton;
    [Tooltip("Set to multi units spacer")]
    public Transform multiUnitsSpacer;

    [Tooltip("Set to unit action skill spacer")]
    public Transform unitActionSpacer;
    [Tooltip("Set to building action skill spacer")]
    public Transform buildingActionSpacer;
    [Tooltip("Set to building action queue spacer")]
    public Transform buildingActionQueueSpacer;


    [Tooltip("Set to Biome Tooltip to use in UI")]
    public GameObject BiomeTooltipPanel;

    [Tooltip("Set to UX Canvas in UI")]
    public RectTransform UXCanvas;
    [Tooltip("Set to gather UX feedback prefab in UI")]
    public GameObject gatherBuildUX;

    [Tooltip("Set to Canvas that contains build actions")]
    public GameObject buildActionPanel;
    [Tooltip("Material for blueprint if it is able to be built")]
    public Material bluePrintCanBuildMat;
    [Tooltip("Material for blueprint if it is unable to be built")]
    public Material bluePrintCannotBuildMat;

    [Tooltip("Set to action button prefab")]
    public GameObject actionButton;
    [Tooltip("Set to action queue button prefab")]
    public GameObject actionQueueButton;

    /// <summary>
    /// Displays/Hides the given object by changing the "Active" value in the hierarchy
    /// </summary>
    /// <param name="panel">Panel in UI to modify</param>
    /// <param name="show">Panel's "active" value</param>
    public void ShowUIObject(GameObject panel, bool show)
    {
        panel.SetActive(show);
    }

    /// <summary>
    /// Sets provided string to set each word to start with a capital letter
    /// </summary>
    /// <param name="stringToChange">String to update</param>
    public string UppercaseFirstAndAfterSpaces(string stringToChange)
    {
        char tempChar = '\0';

        if (string.IsNullOrEmpty(stringToChange))
        {
            return string.Empty;
        }

        char[] a = stringToChange.ToCharArray();
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
