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

    [MenuItem("DeepFry/Tiles/Generate/All Tiles")]
    public static void GenerateTiles()
    {
        DebugMessage("Running Tile Generation");
        int xSize = (int)Terrain.activeTerrain.terrainData.size.x;
        int xMin = -(xSize / 2);
        int xMax = (xSize / 2);

        int zSize = (int)Terrain.activeTerrain.terrainData.size.y;
        int zMin = -(zSize / 2);
        int zMax = (zSize / 2);

        DebugMessage("Loading parent 'BiomeTiles' for tiles");
        Transform parentForTiles = GameObject.Find("BiomeTiles").transform;
        if (parentForTiles == null)
        {
            throw new FileNotFoundException("Parent object not found - please check configuration");
        } else
        {
            DebugMessage("Parent found!");
        }

        DebugMessage("Loading prefab from 'Resources/Tile/Tile");
        Object tilePrefab = Resources.Load("Tile/Tile");

        if (tilePrefab == null)
        {
            throw new FileNotFoundException("Prefab not found - please check configuration");
        } else
        {
            DebugMessage("Tile prefab loaded!");
        }

        DebugMessage("Generating Tiles");

        /*for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                Debug.Log("Place tile at " + x + ", " + terrainheight + ", " + y);
            }
        }*/

        // for testing
        for (int x = xMin; x <= (xMin + 100); x++)
        {
            for (int z = zMin; z <= (zMin + 100); z++)
            {
                float terrainHeight = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));
                Vector3 pos = new Vector3(x, terrainHeight, z);
                Debug.Log("Placing tile at " + pos);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, parentForTiles) as GameObject;
                Debug.Log(tile.name);
                tile.name = "Tile (" + x + ", " + z + ")";
                tile.GetComponent<BiomeTile>().defaultX = x;
                tile.GetComponent<BiomeTile>().defaultZ = z;

                //Set biome type
                switch (TerrainSurface.GetMainTexture(pos))
                {
                    case 3:
                        Debug.Log(pos + " - Plains/Great Plains");
                        tile.GetComponent<BiomeTile>().biomeType = BiomeTypes.PLAINS;
                        tile.GetComponent<BiomeTile>().biomeName = "Great Plains";
                        break;
                    case 6:
                        Debug.Log(pos + " - Beach/Newport Beach");
                        tile.GetComponent<BiomeTile>().biomeType = BiomeTypes.BEACH;
                        tile.GetComponent<BiomeTile>().biomeName = "Newport Beach";
                        break;
                    case 8:
                        Debug.Log(pos + " - Ocean/Pacific Ocean");
                        tile.GetComponent<BiomeTile>().biomeType = BiomeTypes.OCEAN;
                        tile.GetComponent<BiomeTile>().biomeName = "Pacific Ocean";
                        break;
                }
            }
        }

        DebugMessage("GenerateTiles Completed successfully!");
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

        DebugMessage("Loading parent 'BiomeTiles' for tiles");
        Transform parentForTiles = GameObject.Find("BiomeTiles").transform;
        if (parentForTiles == null)
        {
            throw new FileNotFoundException("Parent object not found - please check configuration");
        }
        else
        {
            DebugMessage("Parent found!");
        }        

        DebugMessage("Verifying Tiles");

        // for testing
        for (int x = xMin; x <= (xMin + 100); x++)
        {
            for (int z = zMin; z <= (zMin + 100); z++)
            {
                float terrainHeight = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));
                Vector3 pos = new Vector3(x, terrainHeight, z);

                if (TerrainSurface.GetMainTexture(pos) != 3 && TerrainSurface.GetMainTexture(pos) != 6 && TerrainSurface.GetMainTexture(pos) != 8)
                {
                    Debug.Log("Texture index for " + pos + " - " + TerrainSurface.GetMainTexture(pos));
                }                
            }
        }

        DebugMessage("VerifyTileTerrain Completed successfully!");
    }

    [MenuItem("DeepFry/Tiles/Delete/All Tiles")]
    public static void DeleteAllTiles()
    {
        DebugMessage("Deleting all tiles");
        DebugMessage("Loading parent 'BiomeTiles' for tiles");
        Transform parentForTiles = GameObject.Find("BiomeTiles").transform;
        if (parentForTiles == null)
        {
            throw new FileNotFoundException("Parent object not found - please check configuration");
        }
        else
        {
            DebugMessage("Parent found! Deleting all tiles now.");
        }

        var tempList = parentForTiles.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            DestroyImmediate(child.gameObject);
        }

        DebugMessage("DeleteAllTiles Completed successfully!");
    }

    [MenuItem("DeepFry/Tiles/Gizmos/ShowTileGizmos")]
    public static void ShowTileGizmos()
    {
        DebugMessage("Showing tile gizmos");
        DebugMessage("Loading parent 'BiomeTiles' for tiles");
        Transform parentForTiles = GameObject.Find("BiomeTiles").transform;
        if (parentForTiles == null)
        {
            throw new FileNotFoundException("Parent object not found - please check configuration");
        }
        else
        {
            DebugMessage("Parent found! Showing all tiles now.");
        }

        foreach (Transform child in parentForTiles)
        {
            child.GetComponent<BiomeTile>().hideGizmos = false;
        }
    }

    [MenuItem("DeepFry/Tiles/Gizmos/HideTileGizmos")]
    public static void HideTileGizmos()
    {
        DebugMessage("Hiding tile gizmos");
        DebugMessage("Loading parent 'BiomeTiles' for tiles");
        Transform parentForTiles = GameObject.Find("BiomeTiles").transform;
        if (parentForTiles == null)
        {
            throw new FileNotFoundException("Parent object not found - please check configuration");
        }
        else
        {
            DebugMessage("Parent found! Hiding all tiles now.");
        }

        foreach (Transform child in parentForTiles)
        {
            child.GetComponent<BiomeTile>().hideGizmos = true;
        }
    }

    [MenuItem("DeepFry/Tiles/Update/UpdateTileHeight")]
    public static void UpdateTileHeight()
    {
        DebugMessage("Updating all tile height");
        DebugMessage("Loading parent 'BiomeTiles' for tiles");
        Transform parentForTiles = GameObject.Find("BiomeTiles").transform;
        if (parentForTiles == null)
        {
            throw new FileNotFoundException("Parent object not found - please check configuration");
        }
        else
        {
            DebugMessage("Parent found! Updating tile height.");
        }

        foreach (Transform child in parentForTiles)
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(new Vector3(child.GetComponent<BiomeTile>().transform.position.x, 0, child.GetComponent<BiomeTile>().transform.position.z));

            child.GetComponent<BiomeTile>().transform.position = new Vector3(
                child.GetComponent<BiomeTile>().transform.position.x,
                terrainHeight,
                child.GetComponent<BiomeTile>().transform.position.z);
        }

        DebugMessage("UpdateTileHeight Completed successfully!");
    }

    [MenuItem("DeepFry/Tiles/Update/Realign Tiles")]
    public static void RealignTiles()
    {
        DebugMessage("Realigning all tiles");
        DebugMessage("Loading parent 'BiomeTiles' for tiles");
        Transform parentForTiles = GameObject.Find("BiomeTiles").transform;
        if (parentForTiles == null)
        {
            throw new FileNotFoundException("Parent object not found - please check configuration");
        }
        else
        {
            DebugMessage("Parent found! Realigning tiles.");
        }

        foreach (Transform child in parentForTiles)
        {
            /*float terrainHeight = Terrain.activeTerrain.SampleHeight(new Vector3(child.GetComponent<BiomeTile>().defaultX, 0, child.GetComponent<BiomeTile>().defaultZ));

            child.GetComponent<BiomeTile>().transform.position = new Vector3(
                child.GetComponent<BiomeTile>().defaultX,
                terrainHeight,
                child.GetComponent<BiomeTile>().defaultZ);*/
            Debug.Log(child.GetComponent<BiomeTile>().defaultX + ", " + child.GetComponent<BiomeTile>().defaultZ);            
        }

        DebugMessage("RealignTiles Completed successfully!");
    }  

    [MenuItem("DeepFry/Tiles/Select/SelectAllOceanTiles")]
    public static void SelectAllOceanTiles()
    {
        DebugMessage("Selecting all ocean tiles");
        DebugMessage("Loading parent 'BiomeTiles' for tiles");
        Transform parentForTiles = GameObject.Find("BiomeTiles").transform;
        if (parentForTiles == null)
        {
            throw new FileNotFoundException("Parent object not found - please check configuration");
        }
        else
        {
            DebugMessage("Parent found! Selecting ocean tiles.");
        }

        List<GameObject> tiles = new List<GameObject>();

        foreach (Transform child in parentForTiles)
        {
            if (child.GetComponent<BiomeTile>().biomeType == BiomeTypes.OCEAN)
            {
                tiles.Add(child.gameObject);
            }
        }

        GameObject[] tilesArray = tiles.ToArray();
        Selection.objects = tilesArray;
    }

    static void DebugMessage(string msg)
    {
        Debug.LogWarning(debugDashes + msg + debugDashes);
    }
}
