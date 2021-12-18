using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavInterface : MonoBehaviour
{
    [Tooltip("Camera's default view angle")]
    [SerializeField] [Range(0, 90)] float cameraAngle;

    [Tooltip("Camera's distance from terrain")]
    [Range(1, 50)] public float cameraDistance; // also used by NavMovement for drag movement

    [Tooltip("How high above 0 the terrain is placed")]
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
