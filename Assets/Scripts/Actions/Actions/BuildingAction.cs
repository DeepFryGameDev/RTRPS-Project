using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingAction : MonoBehaviour
{
    [ReadOnly] public Unit unit;
    [ReadOnly] public BaseBuilding building;

    UIProcessing uip;
    BuildManager bm;
    PlayerResources pr;

    bool resourcesAvailable;
    Color resourcesAvailableColor;
    Color resourcesUnavailableColor = Color.red;
    Image icon;

    // Start is called before the first frame update
    void Start()
    {
        SetAction();
        uip = FindObjectOfType<UIProcessing>();
        bm = FindObjectOfType<BuildManager>();  
    }

    private void Awake()
    {
        pr = FindObjectOfType<PlayerResources>();
        icon = transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>();
        resourcesAvailableColor = icon.color;
    }

    private void Update()
    {
        if (Input.GetKeyDown(building.shortcutKey))
        {
            StartBuilding();
        }

        CheckResources();
    }

    public void CheckResources()
    {
        /*if (pr == null)
        {
            pr = FindObjectOfType<PlayerResources>();
            icon = transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>();
            resourcesAvailableColor = icon.color;
        }*/

        if (pr.gold >= building.goldRequired &&
            pr.food >= building.foodRequired &&
            pr.ore >= building.oreRequired &&
            pr.wood >= building.woodRequired)
        {
            // able to use
            icon.color = resourcesAvailableColor;
            resourcesAvailable = true;
        } else
        {
            // unable to use
            icon.color = resourcesUnavailableColor;
            resourcesAvailable = false;
        }
    }

    public void SetAction()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;

        if (building.name.Equals("Test Depot"))
        {
            entry.callback.AddListener((data) => { StartBuilding((PointerEventData)data); });
        }

        trigger.triggers.Add(entry);
    }

    void StartBuilding(PointerEventData data)
    {
        StartBuilding();
    }

    void StartBuilding()
    {
        if (resourcesAvailable)
        {
            //Show glow on cursor and keep action button highlighted
            if (!uip.buildingActionClicked)
            {
                bm.chosenBuilding = building;

                uip.buildingActionClicked = true;
                uip.ButtonUIProcessing(this.gameObject);
            }
        }
    }
}
