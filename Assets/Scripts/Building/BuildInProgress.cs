using System.Collections.Generic;
using UnityEngine;

public class BuildInProgress : MonoBehaviour
{
    [Tooltip("Progress of the build.")]
    [HideInInspector] public float progress; // Once this reaches 100%, the build is completed, this is destroyed, and CompletedBuilding is generated.
    [Tooltip("Units participating in the build in progress.")]
    [HideInInspector] public List<Unit> unitsInteracting; // list of units participating in the build process for this building

    [HideInInspector] public BaseBuilding building; // contains the build parameters for the build in progress
    [HideInInspector] public bool destroyed; // used when detecting if the build has been destroyed before completion

    List<MeshRenderer> bipRenderers; // list of mesh renderers on all child objects - this is used to update the alpha of the mesh depending on progress
    Material tempMat; // temporary material used to replace all mesh renderer materials so that alpha can be adjusted
    List<GameObject> listOfRecursiveChildren = new List<GameObject>(); // used to check child gameObjects recursively to obtain all mesh renderers

    private void Start()
    {
        bipRenderers = GetRenderers(); // initializes list of renderers on all child objects
        tempMat = new Material(Shader.Find("Standard")); // sets temporary material to standard shader so it can be modified

        foreach (MeshRenderer mr in bipRenderers) // sets all mesh materials to temporary material and initializes alpah to 0
        {
            tempMat.CopyPropertiesFromMaterial(mr.material);
            Color blankColor = new Color(mr.material.color.r, mr.material.color.g, mr.material.color.b, 0.0f);
            mr.material.color = blankColor;
        }
    }

    // increases progress of the build based on 'prog' input.  This is generated from stat calculations in the VillagerUnit script
    public void IncreaseProgress(float prog)
    {
        progress += prog;

        // update prefab alpha in world to reflect progress
        foreach (MeshRenderer mr in bipRenderers)
        {
            UpdateAlpha(mr.material);
        }
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

    // Checks all child gameobjects and returns all MeshRenderers
    List<MeshRenderer> GetRenderers()
    {
        List<MeshRenderer> temp = new List<MeshRenderer>();
        GetRecursiveChildren(gameObject);

        foreach (GameObject obj in listOfRecursiveChildren)
        {
            if (obj.GetComponent<MeshRenderer>())
            {
                temp.Add(obj.GetComponent<MeshRenderer>());
            }
        }

        return temp;
    }

    // updates color of the material based on progress
    void UpdateAlpha(Material mat)
    {
        Color newColor = mat.color;
        float newAlpha = progress / 100.0f;

        newColor.a = newAlpha;
        mat.color = newColor;
    }
}
