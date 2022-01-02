using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildInProgress : MonoBehaviour
{
    [ReadOnly] public float progress;
    [ReadOnly] public List<Unit> unitsInteracting;

    [HideInInspector] public BaseBuilding building;

    List<MeshRenderer> bipRenderers;
    Material tempMat;
    List<GameObject> listOfChildren = new List<GameObject>();

    private void Start()
    {
        bipRenderers = GetRenderers();
        tempMat = new Material(Shader.Find("Standard"));

        foreach (MeshRenderer mr in bipRenderers)
        {
            tempMat.CopyPropertiesFromMaterial(mr.material);
            Color blankColor = new Color(mr.material.color.r, mr.material.color.g, mr.material.color.b, 0.0f);
            mr.material.color = blankColor;
        }
    }

    List<MeshRenderer> GetRenderers()
    {
        List<MeshRenderer> temp = new List<MeshRenderer>();
        GetRecursiveChildren(gameObject);

        foreach (GameObject obj in listOfChildren) 
        {
            if (obj.GetComponent<MeshRenderer>())
            {
                temp.Add(obj.GetComponent<MeshRenderer>());
            }
        }

        return temp;
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

    public void IncreaseProgress(float prog)
    {
        progress += prog;

        // update prefab alpha in world to reflect progress
        foreach (MeshRenderer mr in bipRenderers)
        {
            UpdateAlpha(mr.material);
        }
    }

    void UpdateAlpha(Material mat)
    {
        Color newColor = mat.color;
        float newAlpha = progress / 100.0f;

        newColor.a = newAlpha;
        mat.color = newColor;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, building.interactionBounds);
    }
}
