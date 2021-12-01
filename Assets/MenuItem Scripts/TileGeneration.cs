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
        int xSize = (int)GameObject.Find("CameraNavigation").GetComponent<NavMovement>().terrainMap.terrainData.size.x;
        int xMin = -(xSize / 2);
        int xMax = (xSize / 2);

        int zSize = (int)GameObject.Find("CameraNavigation").GetComponent<NavMovement>().terrainMap.terrainData.size.y;
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
        for (int x = xMin; x <= (xMin + 25); x++)
        {
            for (int z = zMin; z <= (zMin + 25); z++)
            {
                float terrainHeight = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));
                Debug.Log("Placing tile at " + x + ", " + terrainHeight + ", " + z);
                Object tile = Instantiate(tilePrefab, new Vector3(x, terrainHeight, z), Quaternion.identity, parentForTiles);
                tile.name = "Tile (" + x + ", " + z + ")";
            }
        }

        DebugMessage("GenerateTiles Completed successfully!");
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

    static void DebugMessage(string msg)
    {
        Debug.LogWarning(debugDashes + msg + debugDashes);
    }
}
