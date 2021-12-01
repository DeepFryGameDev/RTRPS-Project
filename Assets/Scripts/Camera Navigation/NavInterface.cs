using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavInterface : MonoBehaviour
{
    [SerializeField] bool debugging;

    [SerializeField] [Range(0, 90)] float cameraAngle;
    
    [Range(1, 25)] public float cameraDistance; // also used by NavMovement for drag movement
    public float terrainHeight; // also used by NavMovement for zooming

    void OnValidate()
    {
        Camera.main.transform.eulerAngles = new Vector3(cameraAngle, 0f, 0f);
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, terrainHeight + cameraDistance, Camera.main.transform.position.z);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Semicolon) && Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 20;

            Vector3 outPos = Camera.main.ScreenToWorldPoint(mousePos);

            Debug.Log("World Position: " + Mathf.RoundToInt(outPos.x) + ", " + Mathf.RoundToInt(outPos.z));
        }
    }
}
