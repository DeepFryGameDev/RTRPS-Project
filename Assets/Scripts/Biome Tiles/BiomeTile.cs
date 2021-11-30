using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeTile : MonoBehaviour
{
    public bool hideGizmos = true;

    void OnDrawGizmos()
    {
        if (!hideGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
