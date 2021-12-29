using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GatherAction : MonoBehaviour
{
    [ReadOnly] public Unit unit;
    [ReadOnly] public string action;

    // Start is called before the first frame update
    void Start()
    {
        SetAction();
    }

    public void SetAction()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;   

        if (action.Equals("Gather"))
        {
            entry.callback.AddListener((data) => { Gather((PointerEventData)data); });
        }

        trigger.triggers.Add(entry);
    }

    void Gather(PointerEventData data)
    {
        //Show glow on cursor and keep action button highlighted
    }
}
