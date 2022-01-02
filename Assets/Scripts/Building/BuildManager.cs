using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BuildPhases
{
    MOVETOBUILDING,
    PROCESSBUILD
}

public class BuildManager : MonoBehaviour
{
    public float minBuildTime;
    public float maxBuildTime;    
    public float buildTimeAgilityFactor;
    public float buildTimeIntelligenceFactor;
    public float buildTimeWillpowerFactor;

    public float minBuildPerProg;
    public float maxBuildPerProg;
    public float buildPerProgFactor;
    public float buildPerProgStrengthFactor;
    public float buildPerProgIntelligenceFactor;

    public float minBuildTotProg;
    public float maxBuildTotProg;
    public float buildTotProgFactor;
    public float buildTotProgWillpowerFactor;
    public float buildTotProgStaminaFactor;

    public GameObject buildCanvas;
    public GameObject buildingButtonPrefab;
    public float buildingCanvasButtonAdjustment;
    public Material bluePrintCanBuildMat;
    public Material bluePrintCannotBuildMat;

    public List<BaseBuilding> buildings = new List<BaseBuilding>();

    UIProcessing uip;
    UnitMovement um;
    List<BaseBuilding> availableBuildings = new List<BaseBuilding>();
    GameObject actionSpacer;

    float defaultCanvasHeight;
    bool panelShown;

    [HideInInspector] public BaseBuilding chosenBuilding;
    [HideInInspector] public bool blueprintOpen, blueprintClosed;

    private void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        um = FindObjectOfType<UnitMovement>();

        actionSpacer = buildCanvas.transform.Find("BuildActionPanel/BuildSpacer").gameObject;

        defaultCanvasHeight = buildCanvas.transform.Find("BuildActionPanel").GetComponent<RectTransform>().rect.height;

        ShowBuildPanel(false);
    }

    private void Update()
    {      
        if (uip.buildActionClicked)
        {
            ProcessActionClicked();
        }

        CheckIfActionNoLongerClicked();
    }

    void ProcessActionClicked()
    {
        if (!panelShown)
        {
            ShowBuildPanel(true);
        }

        if (uip.buildingActionClicked && blueprintClosed)
        {
            blueprintClosed = false;
            blueprintOpen = false;
            uip.buildingActionClicked = false;
            uip.buildActionClicked = false;

            uip.actionButtonClicked = false;

            ShowBuildPanel(false);
        }

        if (uip.buildingActionClicked && !blueprintOpen)
        {
            blueprintOpen = true;
            GameObject blueprint = Instantiate(chosenBuilding.blueprintPrefab, transform);
            blueprint.GetComponent<Blueprint>().building = chosenBuilding;
        }
    }

    private void CheckIfActionNoLongerClicked()
    {
        if (uip.buildingActionClicked && Input.GetKeyDown(KeyCode.Escape))
        {
            uip.buildingActionClicked = false;

        } else if (uip.buildActionClicked && Input.GetKeyDown(KeyCode.Escape))
        {
            uip.buildActionClicked = false;
            panelShown = false;
            ShowBuildPanel(false);
        }
    }

    public void StartBuildingProcess(GameObject newBuild)
    {
        foreach (Unit unit in uip.selectedUnits)
        {
            if (uip.GetVillagerUnit(unit) && 
                (uip.GetVillagerUnit(unit).villagerClass == villagerClasses.VILLAGER || uip.GetVillagerUnit(unit).villagerClass == villagerClasses.BUILDER)
                )
            {
                uip.GetVillagerUnit(unit).PrepareBuilding(newBuild);
            }
        }
    }

    public void FinishBuildingProcess(BuildInProgress bip)
    {
        Debug.Log("Building process complete, finalizing bip and instantiating completed building...");

        // show UX feedback for completion

        // Instantiate bip.building.completed building
        Instantiate(bip.building.completePrefab, bip.transform.position, bip.transform.rotation, transform);

        // Destroy bip gameobject
        Destroy(bip.gameObject);
    }

    void ShowBuildPanel(bool show)
    {
       float alpha;
       if (show)
        {
            SetAvailableBuildings();

            SetActionButtons();

            // adjust height of panel based on available buildings
            int canvasHeightFactor = Mathf.FloorToInt(availableBuildings.Count / 3);
            float adjHeight = defaultCanvasHeight + (buildingCanvasButtonAdjustment * (float)canvasHeightFactor);

            RectTransform rt = buildCanvas.transform.Find("BuildActionPanel").GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.rect.width, adjHeight);

            alpha = 1;
            panelShown = true;
        } else
        {
            alpha = 0;
            panelShown = false;
        }

        buildCanvas.GetComponent<CanvasGroup>().alpha = alpha;
    }

    private void SetActionButtons()
    {
        foreach (Transform child in actionSpacer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (BaseBuilding building in availableBuildings)
        {
            // prepare action button
            GameObject buildActionGO = GameObject.Instantiate(buildingButtonPrefab) as GameObject;
            buildActionGO.transform.SetParent(actionSpacer.transform, false);

            // Set Icon
            buildActionGO.transform.Find("SkillIconFrame/SkillIcon").GetComponent<Image>().sprite = building.icon;

            // Set Name
            buildActionGO.transform.Find("SkillName").GetComponent<TMP_Text>().text = building.name;

            // Set Shortcut
            buildActionGO.transform.Find("ShortcutKeyFrame/ShortcutKey").GetComponent<TMP_Text>().text = building.shortcutKey.ToString();

            // Set Action
            BuildingAction ba = buildActionGO.AddComponent(typeof(BuildingAction)) as BuildingAction;
            ba.building = building;

            // Set BuildingAction CheckResources
            //ba.CheckResources();

            // Set Unit
            ba.unit = uip.selectedUnits[0];
        }
    }

    void SetAvailableBuildings()
    {
        availableBuildings.Clear();

        //for now, just add the test building
        availableBuildings.Add(buildings[0]);
    }
}
