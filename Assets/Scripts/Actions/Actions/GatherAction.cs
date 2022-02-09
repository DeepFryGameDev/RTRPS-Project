using UnityEngine;
using UnityEngine.EventSystems;

// This script is added as a component onto generated gather action buttons

public class GatherAction : MonoBehaviour
{
    [HideInInspector] public Unit unit; // unit that is performing the action
    [HideInInspector] public BaseAction action; // action containing parameters to be used when performing this action

    GatherManager gm; // used to tell UIP when gather action has been clicked
    UIProcessing uip; // used for updating UI to show when action has been chosen

    void Start()
    {
        gm = FindObjectOfType<GatherManager>();
        uip = FindObjectOfType<UIProcessing>();

        SetAction(); // sets the button's action on PointerClick to ActionButtonPressed()
    }

    private void Update()
    {
        if (Input.GetKeyDown(action.shortcutKey)) // the same action that is set to PointerClick event in Start() is also performed when pressing the shortcut key
        {
            ActionButtonPressed();
        }
    }

    void ActionButtonPressed() // updates UI Processing to know that the gather action has been requested
    {
        //Show glow on cursor and keep action button highlighted
        if (!uip.actionButtonClicked)
        {
            uip.actionButtonClicked = true;

            uip.ButtonUIProcessing(this.gameObject);

            Invoke(action.actionScript, 0);
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

        entry.callback.AddListener((data) => { ActionButtonPressed((PointerEventData)data); });
        
        trigger.triggers.Add(entry);
    }

    #region ActionScripts

    void Gather()
    {
        uip.actionMode = ActionModes.GATHER;
    }

    #endregion
}
