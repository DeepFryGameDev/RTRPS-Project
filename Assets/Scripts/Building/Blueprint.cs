using System.Collections.Generic;
using UnityEngine;

// this script is used when placing the blueprint of a building down onto the terrain
public class Blueprint : MonoBehaviour
{
    [HideInInspector] public BaseBuilding building; // set when instantiating the blueprint - this holds the building's parameters for the blueprint

    BuildManager bm; // used to get the 'can build' material to be set to each mesh on the blueprint.  Also used to set if the blueprint is still active or if it's been cancelled.
    PlayerResources pr; // used to modify the player's total resources if the blueprint has been placed
    UIPrefabManager uipm; // used to get the prefabs needed to be adjusted

    RaycastHit hit; // used for following the cursor to determine the blueprint's position

    [HideInInspector] public List<GameObject> listOfRecursiveChildren = new List<GameObject>(); // used to check child gameObjects recursively to obtain all mesh renderers and collider overlap scripts

    void Start()
    {
        bm = FindObjectOfType<BuildManager>();
        pr = FindObjectOfType<PlayerResources>();
        uipm = FindObjectOfType<UIPrefabManager>();
                
        SetPosition(); // used to determine initial position of the blueprint based on mouse position

        SetChildrenAndMat(); // checks all child gameobjects recursively and if they contain a mesh, sets their material to a temporary 'can build' material.  This allows the color to be modified if the blueprint cannot be placed
    }

    void Update()
    {
        SetPosition(); // used to set the position of the blueprint based on mouse position

        CheckForInput(); // used to check if the blueprint should be placed, or action should be cancelled altogether
    }

    private void SetPosition() // sets the position of the blueprint based on mouse position
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
        {
            transform.position = hit.point;
        }
    }

    void CheckForInput() // if left click action is performed, and building is not obstructed by any colliders
    {
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

    void SetBuilding()
    {
        // sets appropriate bools to BuildManager to let it know the blueprint has been set
        bm.blueprintOpen = false;
        bm.blueprintClosed = true;

        // subtracts required resources from the player's total resource pool
        pr.wood -= building.woodRequired;
        pr.ore -= building.oreRequired;
        pr.food -= building.foodRequired;
        pr.gold -= building.goldRequired;

        // instantiates the building in progress
        GameObject newBuild = Instantiate(building.inProgressPrefab, transform.position, transform.rotation, bm.transform);

        // sets the new build in progress 
        newBuild.GetComponent<BuildInProgress>().building = building;

        // begins building process for the new build in progress
        bm.StartBuildingProcess(newBuild);

        // destroys the blueprint from the world
        Destroy(gameObject);
    }

    bool BlueprintObstructingAnything()
    {
        //Confirm there is nothing under where the blueprint will be placed
        //Check if the collider on this object is touching another collider and return based on that

        foreach (GameObject obj in listOfRecursiveChildren)
        {
            if (obj.GetComponent<CollidersOverlappingForBuild>() && obj.GetComponent<CollidersOverlappingForBuild>().isOverlapping)
            {
                return true;
            }
        }

        return false;
    }

    // Thanks to Chris Oates for the solution:
    // https://stackoverflow.com/questions/33437244/find-children-of-children-of-a-gameobject
    void GetRecursiveChildren(GameObject obj) // adds all gameobjects that are children in this object to the 'listOfChildren' list.
    {
        if (null == obj)
            return;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
                continue;
            listOfRecursiveChildren.Add(child.gameObject);
            GetRecursiveChildren(child.gameObject);
        }
    }

    // Checks all child gameobjects and sets temporary 'can build' material so the color on the mesh can be manipulated
    void SetChildrenAndMat()
    {
        GetRecursiveChildren(gameObject);

        foreach (GameObject obj in listOfRecursiveChildren)
        {
            if (obj.GetComponent<MeshRenderer>())
            {
                obj.GetComponent<MeshRenderer>().material = uipm.bluePrintCanBuildMat;
            }
        }
    }
}
