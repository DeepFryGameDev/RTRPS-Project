using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* icon attribution 
 * -Buildings-
 * School: Icons made by "https://www.flaticon.com/free-icons/school" - Freepik from "https://www.flaticon.com/"
 * Depot: Depot icon by Icons8 - "https://icons8.com/icon/NknZcbPw1Nxs/depot"
 * 
 * -Building Actions-
 * Villager: Icons made by "https://www.flaticon.com/free-icons/medieval" max.icons from "https://www.flaticon.com/"
 * 
 * -Build-
 * Personal build progress: Icons made by "https://www.flaticon.com/free-icons/work" - Freepik from "https://www.flaticon.com/"
 * Total build progress: Icons made by "https://www.flaticon.com/free-icons/gear" - Freepik from "https://www.flaticon.com/"
 *
 *  -Resources-
 * Rock: Icons made by "https://www.flaticon.com/authors/icongeek26" - Icongeek26 from "https://www.flaticon.com/" 
 * Tree: Icons made by "https://www.freepik.com" - Freepik from "https://www.flaticon.com/"
 * Farm: Icons made by "https://www.flaticon.com/authors/photo3idea-studio" - photo3idea_studio from "https://www.flaticon.com/"
 */

public class IconManager : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
