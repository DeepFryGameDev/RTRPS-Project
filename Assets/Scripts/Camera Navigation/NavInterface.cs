using UnityEngine;

// This script is used primarily to set the default camera angle and height
public class NavInterface : MonoBehaviour
{
    [Tooltip("Camera's default view angle on the X axis")]
    [SerializeField] [Range(0, 90)] float cameraAngle;

    [Tooltip("Camera's distance from terrain")]
    [Range(1, 50)] public float defaultCameraDistance; // also used by NavMovement for drag movement

    [Tooltip("How high above 0 the terrain is placed - set to the height located in 'Set Height Controls' setting.")]
    public float terrainHeight; // also used by NavMovement for zooming

    void OnValidate()
    {
        Camera.main.transform.eulerAngles = new Vector3(cameraAngle, 0f, 0f); // sets default camera angle
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, terrainHeight + defaultCameraDistance, Camera.main.transform.position.z); // sets default camera Y (height above terrain) position
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Semicolon) && Input.GetKeyDown(KeyCode.Mouse0)) // used for troubleshooting only.  Outputs the world's x,z position when holding semicolon and left clicking.
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 20;

            Vector3 outPos = Camera.main.ScreenToWorldPoint(mousePos);

            Debug.Log("World Position: " + Mathf.RoundToInt(outPos.x) + ", " + Mathf.RoundToInt(outPos.z));
        }
    }
}
