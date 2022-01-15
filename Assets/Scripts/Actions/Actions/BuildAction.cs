using UnityEngine;
using UnityEngine.EventSystems;

// This script is added as a component onto generated build action buttons

public class BuildAction : MonoBehaviour
{
    [HideInInspector] public Unit unit; // unit that is performing the action
    [HideInInspector] public BaseAction action; // action containing parameters to be used when performing this action

    UIProcessing uip; // used for updating UI to show when action has been chosen
    BuildManager bm; // used for updating buildActionClicked

    void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        bm = FindObjectOfType<BuildManager>();
        
        SetAction(); // sets the button's action on PointerClick to ActionButtonPressed()
    }

    private void Update()
    {
        if (Input.GetKeyDown(action.shortcutKey)) // the same action that is set to PointerClick event in Start() is also performed when pressing the shortcut key
        {
            ActionButtonPressed();
        }
    }

    void ActionButtonPressed() // updates UI Processing to know that the build action has been requested
    {
        //Show glow on cursor and keep action button highlighted
        if (!uip.actionButtonClicked)
        {
            uip.actionButtonClicked = true;
            bm.buildActionClicked = true;
            uip.ButtonUIProcessing(this.gameObject);
        }            
    }

    void ActionButtonPressed(PointerEventData data) // used for setting PointerClick event
    {
        ActionButtonPressed();      
    }

    void SetAction() // sets PointerClick event system for the button in UI to perform this action when clicked
    {
        EventTrigger trigger = GetComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;

        if (action.actionScript.Equals("Build"))
        {
            entry.callback.AddListener((data) => { ActionButtonPressed((PointerEventData)data); });
        }

        trigger.triggers.Add(entry);
    }

}
