using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    protected void TooltipProcessing()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction, Color.blue);

        if (Physics.Raycast(ray, out hit, 1000) && hit.transform.gameObject.CompareTag("Unit"))
        {
            Debug.Log("Unit found: " + hit.transform.gameObject.name);
        }
    }
}
