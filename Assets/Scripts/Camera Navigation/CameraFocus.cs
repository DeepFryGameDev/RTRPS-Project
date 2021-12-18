using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{
    public Unit focusUnit;

    NavInterface ni;

    private void Start()
    {
        ni = FindObjectOfType<NavInterface>();
    }

    void Update()
    {
        if (focusUnit)
        {
            Vector3 pos = focusUnit.transform.position;
            pos.y = pos.y + ni.cameraDistance;
            transform.position = pos;
        }
    }        
}
