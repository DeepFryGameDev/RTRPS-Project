using UnityEngine;

// This script handles keeping the camera positioned with the object in the center of the camera's view
public class CameraFocus : MonoBehaviour
{
    public Unit focusUnit; // As units are the only object that can move, this is set to the unit to be focused so camera can move with them

    NavInterface ni; // Used to get the camera's default camera distance from the terrain

    private void Start()
    {
        ni = FindObjectOfType<NavInterface>();
    }

    void Update()
    {
        if (focusUnit) // If camera should be focusing on a unit, camera will follow unit's position as it moves
        {
            Vector3 pos = focusUnit.transform.position;
            pos.y = pos.y + ni.defaultCameraDistance;
            transform.position = pos;
        }
    }        
}
