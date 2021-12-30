using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GatherAction : MonoBehaviour
{
    [ReadOnly] public Unit unit;
    [ReadOnly] public BaseAction action;

    UIProcessing uip;

    // Start is called before the first frame update
    void Start()
    {
        SetAction();
        uip = FindObjectOfType<UIProcessing>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(action.shortcutKey))
        {
            Gather();
        }
    }

    public void SetAction()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;   

        if (action.actionScript.Equals("Gather"))
        {
            entry.callback.AddListener((data) => { Gather((PointerEventData)data); });
        }

        trigger.triggers.Add(entry);
    }

    void Gather(PointerEventData data)
    {
        Gather();
    }

    void Gather()
    {
        //Show glow on cursor and keep action button highlighted

        uip.actionButtonClicked = true;
        uip.gatherActionClicked = true;
        uip.ButtonUIProcessing(this.gameObject);
    }
}
