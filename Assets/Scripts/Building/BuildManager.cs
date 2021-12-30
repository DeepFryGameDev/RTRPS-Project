using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public GameObject depotTest;

    UIProcessing uip;
    UnitMovement um;

    private void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        um = FindObjectOfType<UnitMovement>();
    }

    private void Update()
    {      
        if (uip.buildActionClicked)
        {
            ProcessActionClicked();
        }

        CheckIfActionNoLongerClicked();
    }

    void ProcessActionClicked()
    {
        //CheckIfActionNoLongerClicked();
    }

    private void CheckIfActionNoLongerClicked()
    {
        if (uip.buildActionClicked && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Escape)))
        {
            uip.buildActionClicked = false;
        }
    }
}
