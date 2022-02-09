using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This script is added as a component onto generated build action buttons

public class BuildingAction : MonoBehaviour
{
    [HideInInspector] public BaseAction action; // action containing parameters to be used when performing this action

    BaseBuildingAction baseBuildingAction;

    Color baseColor;
    Image icon;
    bool actionAvailable;

    UIProcessing uip; // used for updating UI to show when action has been chosen
    TrainingManager tm;
    DeepFryUtilities dfu;
    PlayerResources pr;

    void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        tm = FindObjectOfType<TrainingManager>();
        dfu = FindObjectOfType<DeepFryUtilities>();
        pr = FindObjectOfType<PlayerResources>();
        
        SetAction(); // sets the button's action on PointerClick to ActionButtonPressed()

        baseBuildingAction = GetBaseBuildingAction();

        icon = transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>();
        baseColor = icon.color;
    }

    private void Update()
    {
        if (Input.GetKeyDown(action.shortcutKey) && actionAvailable) // the same action that is set to PointerClick event in Start() is also performed when pressing the shortcut key
        {
            ActionButtonPressed();
        }

        SetIconColorAndAvailability();
    }

    void SetIconColorAndAvailability()
    {        
        if (dfu.IfPlayerHasAvailableResources(baseBuildingAction.woodRequired, baseBuildingAction.oreRequired, baseBuildingAction.foodRequired, baseBuildingAction.goldRequired) &&
            (uip.selectedCompletedBuilding != null && !uip.selectedCompletedBuilding.ReachedMaxNumberInTrainingQueue())) // color normal and set available
        {
            actionAvailable = true;
            icon.color = baseColor;
        } else // color red and set unavailable
        {
            actionAvailable = false;
            icon.color = uip.resourcesUnavailableForActionColor;
        }
    }

    void ActionButtonPressed() // updates UI Processing to know that the build action has been requested
    {
        // possibly show some UX feedback to show button was clicked
        Invoke(action.actionScript, 0);
    }

    void ActionButtonPressed(PointerEventData data) // used for setting PointerClick event
    {
        if (actionAvailable)
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

    BaseBuildingAction GetBaseBuildingAction()
    {
        BaseBuildingAction tryAction = action as BaseBuildingAction;

        return tryAction;
    }

    #region ActionScripts

    private void TrainVillager()
    {
        BaseTrainAction newBTA = tm.GetTrainActionFromBuildingAction(baseBuildingAction);

        //Debug.Log("Adding BTA - " + newBTA.name);
        uip.selectedCompletedBuilding.queuedTrainActions.Add(newBTA);

        pr.RemoveResources(baseBuildingAction.woodRequired, baseBuildingAction.oreRequired, baseBuildingAction.foodRequired, baseBuildingAction.goldRequired);
    }

    #endregion

}
