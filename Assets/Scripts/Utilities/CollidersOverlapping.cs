using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidersOverlapping : MonoBehaviour
{
    [HideInInspector] public bool isOverlapping;

    bool isTouchingUnit, isTouchingBuilding, isTouchingResource;
    BuildManager bm;

    private void Start()
    {
        bm = FindObjectOfType<BuildManager>();
    }

    private void Update()
    {
        UpdateOverlapping();
    }

    private void UpdateOverlapping()
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

    private void ChangeOverlapMat(bool overlapping)
    {
        foreach (GameObject obj in GetBlueprintParent().GetComponent<Blueprint>().listOfChildren)
        {
            if (obj.GetComponent<MeshRenderer>())
            {
                if (overlapping)
                {
                    obj.GetComponent<MeshRenderer>().material = bm.bluePrintCannotBuildMat;
                } else
                {
                    obj.GetComponent<MeshRenderer>().material = bm.bluePrintCanBuildMat;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Unit"))
        {
            isTouchingUnit = true;
        }
        
        if (other.CompareTag("Building"))
        {
            isTouchingBuilding = true;
        }

        if (other.CompareTag("Resource"))
        {
            isTouchingResource = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Unit"))
        {
            isTouchingUnit = false;
        }

        if (other.CompareTag("Building"))
        {
            isTouchingBuilding = false;
        }

        if (other.CompareTag("Resource"))
        {
            isTouchingResource = false;
        }
    }

    Transform GetBlueprintParent()
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
