using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildAction : MonoBehaviour
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
            Build();
        }
    }

    public void SetAction()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;

        if (action.actionScript.Equals("Build"))
        {
            entry.callback.AddListener((data) => { Build((PointerEventData)data); });
        }

        trigger.triggers.Add(entry);
    }

    void Build(PointerEventData data)
    {
        Build();      
    }

    void Build()
    {
        //Show glow on cursor and keep action button highlighted
        if (!uip.actionButtonClicked)
        {
            uip.actionButtonClicked = true;
            uip.buildActionClicked = true;
            uip.ButtonUIProcessing(this.gameObject);
        }            
    }
}
