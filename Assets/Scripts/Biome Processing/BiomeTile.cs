using UnityEngine;

// used to determine what biome the camera is looking at, or unit/building is currently on
public class BiomeTile
{
    public int biomeID; // ID used to determine biome.  This is set by the texture index painted on the terrain
    public BiomeTypes primaryBiomeType; // primary biome to be used for stat calculations and bonuses
    public BiomeTypes secondaryBiomeType; // not being used yet
    public string name; // actual name of the region, ie: "Sherwood Forest"

    public BiomeTile() // when constructed, default biomeID to 0 so there are no null exceptions.  Also sets default biome
    {
        biomeID = 0;

        SetBiome();
    }

    public void SetBiomeID(Vector3 position) // sets biome ID based on the position and terrain height.  The ID is gathered from the dominant texture painted at this position
    {
        float terrainHeight = Terrain.activeTerrain.SampleHeight(new Vector3(position.x, 0, position.z));
        Vector3 pos = new Vector3(position.x, terrainHeight, position.z);

        biomeID = TerrainSurface.GetMainTexture(pos);

        SetBiome();
    }

    void SetBiome() // sets Tile details based on the Biome ID
    {
        switch (biomeID)
        {
            case 0: // only ran once when setting up the tile, otherwise this indicates a problem on the terrain that there is no dominant texture painted, as the texture in index 0 is not to be used.
                name = "NULL";
                primaryBiomeType = BiomeTypes.PLAINS;
                break;
            case 1:
                name = "Rocky Mountains";
                primaryBiomeType = BiomeTypes.MOUNTAIN;
                break;
            case 2:
                name = "Black Marsh";
                primaryBiomeType = BiomeTypes.SWAMP;
                break;
            case 3:
                name = "Great Plains";
                primaryBiomeType = BiomeTypes.PLAINS;
                break;
            case 5:
                name = "Pacific Ocean";
                primaryBiomeType = BiomeTypes.OCEAN;
                break;
            case 7:
                name = "Rocky Mountains";
                primaryBiomeType = BiomeTypes.MOUNTAIN;
                break;
            case 6:
                name = "Newport Beach";
                primaryBiomeType = BiomeTypes.BEACH;
                break;
            case 8:
                name = "Pacific Ocean";
                primaryBiomeType = BiomeTypes.OCEAN;
                break;

            default:
                Debug.Log("In biome ID: " + biomeID); // used to determine which biome the game is referencing, if not already configured
                name = "NULL";
                primaryBiomeType = BiomeTypes.PLAINS;
                break;
        }
    }
}
