using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiUnitsSelectionButton : MonoBehaviour
{
    public Unit unit;

    UIProcessing uip;
    UnitProcessing up;

    // Start is called before the first frame update
    void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        up = FindObjectOfType<UnitProcessing>();
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
    }
}
