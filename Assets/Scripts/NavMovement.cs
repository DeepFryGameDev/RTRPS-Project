using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMovement : MonoBehaviour
{
    [SerializeField] bool debugging;

    [SerializeField] [Range(1f, 150f)] float panBorderSize;
    [SerializeField] [Range(1f, 50f)] float panFactor;
    [SerializeField] [Range(1f, 100f)] float panSpeedOnKeyPress;
    [SerializeField] [Range(1f, 100f)] float scrollSpeed;

    [SerializeField] Terrain terrainMap;
    [SerializeField] Camera mainCamera;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        Vector3 pos = mainCamera.transform.position;

        //up
        if (Input.GetAxis("Vertical") > 0)
        {
            pos.z += panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Vertical")) * Time.deltaTime;
        } else if (Input.mousePosition.y >= Screen.height - panBorderSize && Input.mousePosition.y <= Screen.height)
        {
            float position = (Input.mousePosition.y - (Screen.height - panBorderSize));
            float panSpeed = panFactor * (position / panBorderSize);

            pos.z += panSpeed * Time.deltaTime;
        }

        //down
        if (Input.GetAxis("Vertical") < 0)
        {
            pos.z -= panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Vertical")) * Time.deltaTime;
        }
        else if (Input.mousePosition.y <= panBorderSize && (Input.mousePosition.y - panBorderSize) >= -panBorderSize)
        {
            float position = (Input.mousePosition.y - panBorderSize);
            float panSpeed = panFactor * Mathf.Abs(-position / -panBorderSize);

            pos.z -= panSpeed * Time.deltaTime;
        }

        //right
        if (Input.GetAxis("Horizontal") > 0)
        {
            pos.x += panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Horizontal")) * Time.deltaTime;
        }
        else if (Input.mousePosition.x >= Screen.width - panBorderSize && Input.mousePosition.x <= Screen.width)
        {
            float position = (Input.mousePosition.x - (Screen.width - panBorderSize));
            float panSpeed = panFactor * (position / panBorderSize);

            pos.x += panSpeed * Time.deltaTime;
        }
        
        //left
        if (Input.GetAxis("Horizontal") < 0)
        {
            pos.x -= panSpeedOnKeyPress * Mathf.Abs(Input.GetAxis("Horizontal")) * Time.deltaTime;
        }
        else if (Input.mousePosition.x <= panBorderSize && (Input.mousePosition.x - panBorderSize) >= -panBorderSize)
        {
            float position = (Input.mousePosition.x - panBorderSize);
            float panSpeed = panFactor * Mathf.Abs(-position / -panBorderSize);

            pos.x -= panSpeed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, -(terrainMap.terrainData.size.x/2), (terrainMap.terrainData.size.x/2));
        pos.z = Mathf.Clamp(pos.z, -(terrainMap.terrainData.size.z/2), (terrainMap.terrainData.size.z/2));

        mainCamera.transform.position = pos;
    }

    void ZoomCamera()
    {
        Vector3 pos = mainCamera.transform.position;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * (scrollSpeed * 10) * Time.deltaTime;

        mainCamera.transform.position = pos;
    }
}
