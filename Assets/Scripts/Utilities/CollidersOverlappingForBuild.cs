using UnityEngine;

// Used to detect if colliders are overlapping for blueprint 
public class CollidersOverlappingForBuild : MonoBehaviour
{
    [HideInInspector] public bool isOverlapping; // primary bool used to detect overlapping

    bool isTouchingUnit, isTouchingBuilding, isTouchingResource; // used to detect multiple objects and which type is being overlapped (may need to be updated)

    UIPrefabManager uipm; // used to retrieve the 'can' and 'cannot' build materials for the blueprint.  This will set the blueprint to another color if it is being hovered over another object

    private void Start()
    {
        uipm = FindObjectOfType<UIPrefabManager>();
    }

    private void Update()
    {
        UpdateOverlapping();
    }

    private void UpdateOverlapping() // Sets variables upon overlap
    {
        if (isTouchingUnit || isTouchingBuilding || isTouchingResource)
        {
            isOverlapping = true;
        }
        else
        {
            isOverlapping = false;
        }

        ChangeOverlapMat(isOverlapping);
    }

    private void ChangeOverlapMat(bool overlapping) // Changes material of blueprint components upon overlap
    {
        foreach (GameObject obj in GetBlueprintParent().GetComponent<Blueprint>().listOfRecursiveChildren)
        {
            if (obj.GetComponent<MeshRenderer>())
            {
                if (overlapping)
                {
                    obj.GetComponent<MeshRenderer>().material = uipm.bluePrintCannotBuildMat;
                } else
                {
                    obj.GetComponent<MeshRenderer>().material = uipm.bluePrintCanBuildMat;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other) // If blueprint colliders are touching any of object type, the corresponding bool is returned true
    {
        if (other.CompareTag("Unit"))
        {
            isTouchingUnit = true;
        }
        
        if (other.CompareTag("BlueprintBuilding"))
        {
            isTouchingBuilding = true;
        }

        if (other.CompareTag("Resource"))
        {
            isTouchingResource = true;
        }
    }

    void OnTriggerExit(Collider other) // Sets the corresponding bool to false if they are no longer touching colliders
    {
        if (other.CompareTag("Unit"))
        {
            isTouchingUnit = false;
        }

        if (other.CompareTag("BlueprintBuilding"))
        {
            isTouchingBuilding = false;
        }

        if (other.CompareTag("Resource"))
        {
            isTouchingResource = false;
        }
    }

    Transform GetBlueprintParent() // Returns the blueprint at the top level of the object - this will need to be updated as this is currently a hacky workaround
    {
        Transform temp = transform;

        if (temp.GetComponent<Blueprint>())
        {
            return temp;
        }

        if (!temp.GetComponent<Blueprint>())
        {
            temp = temp.parent;

            if (!temp.GetComponent<Blueprint>())
            {
                temp = temp.parent;

                if (!temp.GetComponent<Blueprint>())
                {
                    temp = temp.parent;

                    if (!temp.GetComponent<Blueprint>())
                    {
                        temp = temp.parent;
                    }
                }
            }
        }

        if (temp == transform)
        {
            return null;
        } else
        {
            return temp;
        }
    }
}
