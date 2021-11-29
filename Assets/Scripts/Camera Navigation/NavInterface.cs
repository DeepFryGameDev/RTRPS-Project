using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavInterface : MonoBehaviour
{
    [SerializeField] bool debugging;

    [SerializeField] [Range(0, 90)] float cameraAngle;
    
    [Range(1, 25)] public float cameraDistance; // also used by NavMovement for drag movement

    void OnValidate()
    {
        Camera.main.transform.eulerAngles = new Vector3(cameraAngle, 0f, 0f);
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, cameraDistance, Camera.main.transform.position.z);
    }
}
