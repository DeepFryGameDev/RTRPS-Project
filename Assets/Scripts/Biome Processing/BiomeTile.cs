using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeTile
{
    public int biomeID;
    public BiomeTypes primaryBiomeType;
    public BiomeTypes secondaryBiomeType;
    public string name;

    public BiomeTile()
    {
        biomeID = 0;

        SetBiome();
    }

    public void SetBiomeID(Vector3 position)
    {
        float terrainHeight = Terrain.activeTerrain.SampleHeight(new Vector3(position.x, 0, position.z));
        Vector3 pos = new Vector3(position.x, terrainHeight, position.z);

        biomeID = TerrainSurface.GetMainTexture(pos);

        SetBiome();
    }

    void SetBiome()
    {
        switch (biomeID)
        {
            case 3:
                name = "Great Plains";
                primaryBiomeType = BiomeTypes.PLAINS;
                break;
            case 6:
                name = "Sahara Desert";
                primaryBiomeType = BiomeTypes.BEACH;
                break;

            default:
                name = "NULL";
                primaryBiomeType = BiomeTypes.PLAINS;
                break;
        }
    }
}
