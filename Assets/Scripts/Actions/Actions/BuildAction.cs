using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildAction : MonoBehaviour
{
    [ReadOnly] public Unit unit;
    [ReadOnly] public string action;

    BuildManager bm;

    // Start is called before the first frame update
    void Start()
    {
        bm = FindObjectOfType<BuildManager>();

        SetAction();
    }

    public void SetAction()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;

        if (action.Equals("Build"))
        {
            entry.callback.AddListener((data) => { Build((PointerEventData)data); });
        }

        trigger.triggers.Add(entry);
    }

    void Build(PointerEventData data)
    {
        //Display build menu, the options here will proceed to:
        //Instantiate(bm.depotTest);
    }
}
