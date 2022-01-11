using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

[System.Serializable]
public class TileGeneration : MonoBehaviour
{
    static string debugDashes = " ---------- ";

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
