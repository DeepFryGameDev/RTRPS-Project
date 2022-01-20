using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DeepFryCommands : MonoBehaviour
{
    static string debugDashes = " ---------- ";

    [MenuItem("DeepFry/UI/Show/OptionsCanvas")]
    public static void ShowOptionsCanvas()
    {
        DebugMessage("Showing Options Canvas");
        AnimationManager am = FindObjectOfType<AnimationManager>();
        am.ShowOptionsPanel();
    }

    [MenuItem("DeepFry/UI/Show/UnitCanvas")]
    public static void ShowUnitCanvas()
    {
        DebugMessage("Showing Unit Canvas");
        AnimationManager am = FindObjectOfType<AnimationManager>();
        am.ShowUnitCanvas();
    }

    [MenuItem("DeepFry/UI/Show/BuildingCanvas")]
    public static void ShowBuildingCanvas()
    {
        DebugMessage("Showing Building Canvas");
        AnimationManager am = FindObjectOfType<AnimationManager>();
        am.ShowBuildingCanvas();
    }

    [MenuItem("DeepFry/UI/Show/ResourceCanvas")]
    public static void ShowResourceCanvas()
    {
        DebugMessage("Showing Resource Canvas");
        AnimationManager am = FindObjectOfType<AnimationManager>();
        am.ShowResourceCanvas();
    }

    [MenuItem("DeepFry/UI/Show/BuildActionCanvas")]
    public static void ShowBuildActionPanel()
    {
        DebugMessage("Showing Build Action Panel");
        AnimationManager am = FindObjectOfType<AnimationManager>();
        am.ShowBuildActionPanel();
    }

    [MenuItem("DeepFry/UI/Show/MultiUnitCanvas")]
    public static void ShowMultiUnitCanvas()
    {
        DebugMessage("Showing Multi Unit Canvas");
        AnimationManager am = FindObjectOfType<AnimationManager>();
        am.ShowMultiUnitPanel();
    }

    [MenuItem("DeepFry/UI/Hide/AllPanels")]
    public static void HideUIPanels()
    {
        DebugMessage("Hiding UI Panels");
        AnimationManager am = FindObjectOfType<AnimationManager>();
        am.HideObjects();
    }

    [MenuItem("DeepFry/Tiles/Verify/Verify Tile Terrain")]
    public static void VerifyTileTerrain()
    {
        DebugMessage("Verifying terrain is painted correctly");
        int xSize = (int)Terrain.activeTerrain.terrainData.size.x;
        int xMin = -(xSize / 2);
        int xMax = (xSize / 2);

        int zSize = (int)Terrain.activeTerrain.terrainData.size.y;
        int zMin = -(zSize / 2);
        int zMax = (zSize / 2);

        DebugMessage("Verifying Tiles");

        // for testing
        for (int x = xMin; x <= (xMax); x++)
        {
            for (int z = zMin; z <= (zMax); z++)
            {
                float terrainHeight = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));
                Vector3 pos = new Vector3(x, terrainHeight, z);

                if (TerrainSurface.GetMainTexture(pos) == 0)
                {
                    Debug.Log("Texture index for " + pos + " is 0");
                }
            }
        }

        DebugMessage("VerifyTileTerrain Completed successfully!");
    }

    static void DebugMessage(string msg)
    {
        Debug.LogWarning(debugDashes + msg + debugDashes);
    }
}
