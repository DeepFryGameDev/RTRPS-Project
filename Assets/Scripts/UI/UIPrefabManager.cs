using UnityEngine;

/* icon attribution 
 * 
 * Personal build progress icon: Icons made by "https://www.flaticon.com/free-icons/work" - Freepik from "https://www.flaticon.com/"
 * Total build progress icon: Icons made by "https://www.flaticon.com/free-icons/gear" - Freepik from "https://www.flaticon.com/"
 */

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
    [Tooltip("Set to multiple units panel in UI")]
    public GameObject multiUnitsPanel;
    [Tooltip("Set to multiple units 'button' prefab")]
    public GameObject multiUnitsButton;
    [Tooltip("Set to action action skill spacer")]
    public GameObject actionSpacer;
    [Tooltip("Set to action button prefab")]
    public GameObject actionButton;

    [Tooltip("Set to Biome Tooltip to use in UI")]
    public GameObject BiomeTooltipPanel;

    [Tooltip("Set to UX Canvas in UI")]
    public RectTransform UXCanvas;
    [Tooltip("Set to gather UX feedback prefab in UI")]
    public GameObject gatherBuildUX;

    [Tooltip("Set to Canvas that contains build actions")]
    public GameObject buildActionCanvas;
    [Tooltip("Material for blueprint if it is able to be built")]
    public Material bluePrintCanBuildMat;
    [Tooltip("Material for blueprint if it is unable to be built")]
    public Material bluePrintCannotBuildMat;

    [Tooltip("Set to icon used for personal build progress in UI")]
    public Sprite personalProgressIcon;
    [Tooltip("Set to icon used for total build progress in UI")]
    public Sprite totalProgressIcon;

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
