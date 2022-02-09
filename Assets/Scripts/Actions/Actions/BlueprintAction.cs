using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This script is added as a component onto generated building action buttons

public class BlueprintAction : MonoBehaviour
{    
    [HideInInspector] public Unit unit; // unit that is performing the action
    [HideInInspector] public BaseBuilding building; // building that is related to the action being performed

    UIProcessing uip; // used for updating UI to show when action has been chosen
    BuildManager bm; // used to set the appropriate building to the buildManager
    PlayerResources pr; // used to check if player has enough resources to process the build action
    DeepFryUtilities dfu;

    bool resourcesAvailable; // set to true when player has enough resources available to process the building action, otherwise it is false
    Color resourcesAvailableColor; // set on Awake() to the icon's default color to be used if player has enough resources available to process the build action
    Image icon; // set on Start() to the action's icon so that color can be modified upon resource availability

    private void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        bm = FindObjectOfType<BuildManager>();
        pr = FindObjectOfType<PlayerResources>();
        dfu = FindObjectOfType<DeepFryUtilities>();

        icon = transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>();
        resourcesAvailableColor = icon.color;

        SetAction(); // sets the button's action on PointerClick to ActionButtonPressed()
    }

    private void Update()
    {
        if (Input.GetKeyDown(building.shortcutKey)) // the same action that is set to PointerClick event in Start() is also performed when pressing the shortcut key
        {
            ActionButtonPressed();
        }

        CheckResources(); // verifies the player has enough resources to perform the action
    }

    void ActionButtonPressed() // sets BuildManager's chosenBuilding to this building, and updates UI Processing to know that the building action has been requested
    {
        if (resourcesAvailable)
        {
            //Show glow on cursor and keep action button highlighted
            if (uip.actionMode == ActionModes.BUILD)
            {
                bm.chosenBuilding = building;

                uip.actionMode = ActionModes.BLUEPRINT;

                uip.ButtonUIProcessing(this.gameObject);
            }
        }
    }

    void CheckResources() // changes icon color and sets resourcesAvailable if player has enough resources for the action's requirements
    {
        if (dfu.IfPlayerHasAvailableResources(building.woodRequired, building.oreRequired, building.foodRequired, building.goldRequired))
        {
            // able to use
            icon.color = resourcesAvailableColor;
            resourcesAvailable = true;
        }
        else
        {
            // unable to use
            icon.color = uip.resourcesUnavailableForActionColor;
            resourcesAvailable = false;
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
}
