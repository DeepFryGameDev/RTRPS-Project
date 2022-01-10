using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : MonoBehaviour
{
    [HideInInspector] public BaseBuilding building;

    RaycastHit hit;
    Vector3 movePoint;

    BuildManager bm;
    PlayerResources pr;
    [HideInInspector] public List<GameObject> listOfChildren = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        bm = FindObjectOfType<BuildManager>();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
        {
            transform.position = hit.point;
        }

        pr = FindObjectOfType<PlayerResources>();
        SetChildrenAndMat();
    }

    private void SetChildrenAndMat()
    {
        GetRecursiveChildren(gameObject);
        
        foreach (GameObject obj in listOfChildren)
        {
            if (obj.GetComponent<MeshRenderer>())
            {
                obj.GetComponent<MeshRenderer>().material = bm.bluePrintCanBuildMat;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
        {
            transform.position = hit.point;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!BlueprintObstructingAnything())
                SetBuilding();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bm.blueprintOpen = false;
            Destroy(gameObject);
        }
    }

    bool BlueprintObstructingAnything()
    {
        //Confirm there is nothing under where the blueprint will be placed
        //Check if the collider on this object is touching another collider and return based on that

        foreach (GameObject obj in listOfChildren)
        {
            if (obj.GetComponent<CollidersOverlappingForBuild>() && obj.GetComponent<CollidersOverlappingForBuild>().isOverlapping)
            {
                return true;
            }
        }

        return false;
    }

    private void GetRecursiveChildren(GameObject obj)
    {
        if (null == obj)
            return;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
                continue;
            //child.gameobject contains the current child you can do whatever you want like add it to an array
            listOfChildren.Add(child.gameObject);
            GetRecursiveChildren(child.gameObject);
        }
    }

    private void SetBuilding()
    {
        bm.blueprintOpen = false;
        bm.blueprintClosed = true;

        GameObject newBuild = Instantiate(building.inProgressPrefab, transform.position, transform.rotation, bm.transform);

        pr.wood -= building.woodRequired;
        pr.ore -= building.oreRequired;
        pr.food -= building.foodRequired;
        pr.gold -= building.goldRequired;

        newBuild.GetComponent<BuildInProgress>().building = building;

        bm.StartBuildingProcess(newBuild);

        Destroy(gameObject);
    }
}
