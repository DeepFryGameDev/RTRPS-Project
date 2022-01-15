using System.Collections;
using UnityEngine;

// This script handles the processing when a unit in the multi-selection panel has been clicked
public class MultiUnitsSelectionButton : MonoBehaviour
{
    public Unit unit;

    UIProcessing uip;
    NavMovement nm;

    bool isClicked = false, doubleClicked = false;
    int frameCount;

    // Start is called before the first frame update
    void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        nm = FindObjectOfType<NavMovement>();
        
        frameCount = 0;
    }

    public void OnCursorEnter()
    {
        unit.GetComponent<Outline>().OutlineWidth = uip.hoveredUnitsOutlineWidth;
    }

    public void OnCursorExit()
    {        
        if (uip.selectedUnit != unit)
        {
            unit.GetComponent<Outline>().OutlineWidth = uip.defaultOutlineWidth;
        }
    }

    public void OnCursorClick() // update UI based on this unit
    {      
        // make all other selected units have default outline
        foreach (Unit unit in uip.selectedUnits)
        {
            unit.GetComponent<Outline>().OutlineWidth = uip.defaultOutlineWidth;
        }

        // set this one to hoveredUnitsOutlineWidth
        unit.GetComponent<Outline>().OutlineWidth = uip.hoveredUnitsOutlineWidth;

        uip.SetCurrentUnit(unit);
        uip.resetUI = true;

        if (frameCount < uip.selectedObjectDoubleClickFrameBuffer && isClicked)
        {
            doubleClicked = true;
            nm.FocusSelection(unit.gameObject);
        }

        if (!isClicked)
        {
            isClicked = true;
            StartCoroutine(CheckForDoubleClick());
        }
    }

    IEnumerator CheckForDoubleClick()
    {
        doubleClicked = false;

        while (frameCount <= uip.selectedObjectDoubleClickFrameBuffer && !doubleClicked)
        {
            frameCount++;
            yield return new WaitForEndOfFrame();
        }

        frameCount = 0;
        isClicked = false;
    }
}
