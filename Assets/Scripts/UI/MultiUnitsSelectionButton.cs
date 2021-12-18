using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiUnitsSelectionButton : MonoBehaviour
{
    public Unit unit;

    UIProcessing uip;
    UnitProcessing up;

    bool isClicked = false, doubleClicked = false;
    int frameCount;

    // Start is called before the first frame update
    void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        up = FindObjectOfType<UnitProcessing>();
        frameCount = 0;
    }

    IEnumerator CheckForDoubleClick()
    {
        doubleClicked = false;

        while (frameCount < uip.selectedUnitDoubleClickFrameBuffer && !doubleClicked)
        {
            frameCount++;
            yield return new WaitForEndOfFrame();
        }

        frameCount = 0;
        isClicked = false;
    }

    public void OnCursorEnter()
    {
        unit.GetComponent<Outline>().OutlineWidth = uip.hoveredUnitsOutlineWidth;
    }

    public void OnCursorExit()
    {
        unit.GetComponent<Outline>().OutlineWidth = up.highlightWidth;
    }

    public void OnCursorClick()
    {
        // update UI based on this unit
        uip.SetCurrentUnit(unit);
        uip.resetUI = true;

        if (frameCount < uip.selectedUnitDoubleClickFrameBuffer && isClicked)
        {
            doubleClicked = true;
            Debug.Log("double clicked on unit " + unit.gameObject.name);
        }

        if (!isClicked)
        {
            isClicked = true;
            StartCoroutine(CheckForDoubleClick());
        }
    }
}
