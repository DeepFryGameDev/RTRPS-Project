using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeTile : MonoBehaviour
{
    public bool hideGizmos = true;

    public string biomeName;
    public BiomeTypes biomeType;

    [HideInInspector] public float defaultX, defaultZ;

    // BIOME COLORS
    Color plainsColor = Color.green;
    Color plainsHillColor = Color.green;
    Color forestColor = Color.grey;
    Color forestHillColor = Color.grey;
    Color mountainColor = Color.black;
    Color riverColor = Color.blue;
    Color oceanColor = Color.blue;
    Color lakeColor = Color.blue;
    Color desertColor = Color.yellow;
    Color beachColor = Color.yellow;

void OnDrawGizmos()
{
    if (!hideGizmos)
    {
        switch (biomeType)
        {
            case BiomeTypes.BEACH:
                Gizmos.color = beachColor;
                break;
            case BiomeTypes.OCEAN:
                Gizmos.color = oceanColor;
                break;
        }
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
}
