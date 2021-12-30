using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GatherPhases
{
    SEEKINGRESOURCE,
    GATHERING,
    MOVETODEPOT,
    DEPOSITING
}

public class GatherManager : MonoBehaviour
{
    [Tooltip("Duration in seconds of outline when clicking a resource")]
    [SerializeField] float resourceConfirmationOutlineDuration;
    [Tooltip("Width of outline when clicking a resource")]
    [SerializeField] [Range(1, 10)] float resourceConfirmationOutlineWidth;
    [Tooltip("Set to resource gather UX feedback prefab in UI")]
    [SerializeField] GameObject resourceGatherUX;
    [Tooltip("Set to Gather Canvas in UI")]
    [SerializeField] RectTransform GatherCanvas;
    [Tooltip("Distance of pixels above unit that UX will appear")]
    [SerializeField] float resourceGatherUXYDistance;
    [Tooltip("Default Scale of UX Feedback")]
    [SerializeField] float resourceGatherUXBaseScale;
    [Tooltip("Speed of UX feedback on screen moving upwards along y position")]
    [SerializeField] float resourceGatherUXSpeed;
    [Tooltip("How quickly the UX Feedback fades")]
    [SerializeField] float resourceGatherUXFadeFactor;    

    UIProcessing uip;
    UnitMovement um;

    private void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
        um = FindObjectOfType<UnitMovement>();
    }

    private void Update()
    {
        if (uip.gatherActionClicked)
        {
            ProcessActionClicked();
        }

        CheckIfActionNoLongerClicked();
    }

    void ProcessActionClicked()
    {        
        CheckIfResourceIsHovered();
    }

    private void CheckIfActionNoLongerClicked()
    {
        /*if (uip.gatherActionClicked && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Escape)))
        {
            uip.gatherActionClicked = false;
        }*/

        if (uip.gatherActionClicked && Input.GetKeyDown(KeyCode.Escape))
        {
            uip.gatherActionClicked = false;
        }
    }

    private void CheckIfResourceIsHovered()
    {
        // loop through all resources and check if they are highlighted.  if yes, remove highlight
        Resource[] allResources = FindObjectsOfType<Resource>();
        foreach (Resource res in allResources)
        {
            if (res.GetComponent<Outline>() && res.GetComponent<Outline>().enabled == true)
            {
                HighlightResource(res, false);
            }
        }

        RaycastHit[] hits;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        hits = Physics.RaycastAll(ray, 1000);

        // Checking if mouse cursor is over a resource
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("Resource"))
            {
                //check for each resource type/unit type
                // check if should be able to gather
                bool canGather = false;

                foreach (Unit unit in uip.selectedUnits)
                {
                    if (hit.transform.GetComponent<Resource>().resourceType == ResourceTypes.WOOD &&
                        (((VillagerUnit)unit).villagerClass == villagerClasses.VILLAGER ||
                        ((VillagerUnit)unit).villagerClass == villagerClasses.GATHERER ||
                        ((VillagerUnit)unit).villagerClass == villagerClasses.LUMBERJACK))
                    {
                        canGather = true;
                    }

                    if (hit.transform.GetComponent<Resource>().resourceType == ResourceTypes.ORE &&
                        (((VillagerUnit)unit).villagerClass == villagerClasses.VILLAGER ||
                        ((VillagerUnit)unit).villagerClass == villagerClasses.GATHERER ||
                        ((VillagerUnit)unit).villagerClass == villagerClasses.MINER))
                    {
                        canGather = true;
                    }

                    if (hit.transform.GetComponent<Resource>().resourceType == ResourceTypes.FOOD &&
                        (((VillagerUnit)unit).villagerClass == villagerClasses.VILLAGER ||
                        ((VillagerUnit)unit).villagerClass == villagerClasses.GATHERER ||
                        ((VillagerUnit)unit).villagerClass == villagerClasses.FARMER))
                    {
                        canGather = true;
                    }
                }

                if (canGather)
                {
                    HighlightResource(hit.transform.GetComponent<Resource>(), true);

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        um.StartGathering(hit.transform.GetComponent<Resource>());
                    }
                }
            }
        }
    }

    public IEnumerator ShowResourceGatherUX(GameObject source, ResourceTypes resourceType, int gathered, bool plus)
    {
        bool feedbackHidden = false;
        Vector3 pos = source.transform.position;
        pos = new Vector3(pos.x, pos.y + resourceGatherUXYDistance, pos.z);

        // set icon
        Sprite icon = null;

        switch (resourceType)
        {
            case ResourceTypes.WOOD:
                icon = uip.woodResourceIcon;
                break;
            case ResourceTypes.ORE:
                icon = uip.oreResourceIcon;
                break;
            case ResourceTypes.FOOD:
                icon = uip.foodResourceIcon;
                break;
        }

        string sign = string.Empty;
        if (plus)
        {
            sign = "+";
        } else
        {
            sign = "-";
        }


        // prepare GatherUX
        GameObject gatherUX = resourceGatherUX;
        gatherUX.transform.Find("ResourceGatheredText").GetComponent<TMP_Text>().text = sign + gathered.ToString();
        gatherUX.transform.Find("ResourceGatheredIcon").GetComponent<Image>().sprite = icon;

        gatherUX = Instantiate(gatherUX, pos, Quaternion.identity, GatherCanvas);
        SetToCanvasSpace(source.gameObject, gatherUX.GetComponent<RectTransform>());

        gatherUX.transform.localScale = new Vector3(resourceGatherUXBaseScale, resourceGatherUXBaseScale, resourceGatherUXBaseScale);
        CanvasGroup cg = gatherUX.GetComponent<CanvasGroup>();

        while (!feedbackHidden)
        {
            float fade = resourceGatherUXFadeFactor * Time.deltaTime;
            float floatSpeed = resourceGatherUXSpeed * Time.deltaTime;

            Vector3 newPos = new Vector3(gatherUX.GetComponent<RectTransform>().position.x, gatherUX.GetComponent<RectTransform>().position.y + floatSpeed, gatherUX.GetComponent<RectTransform>().position.z);

            cg.alpha -= fade;
            gatherUX.GetComponent<RectTransform>().position = newPos;

            if (cg.alpha == 0.0f)
            {
                Destroy(gatherUX);
                feedbackHidden = true;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator HighlightConfirmedResource(Resource resource)
    {
        resource.GetComponent<Outline>().OutlineWidth = resourceConfirmationOutlineWidth;
        resource.GetComponent<Outline>().enabled = true;
        yield return new WaitForSeconds(resourceConfirmationOutlineDuration);
        resource.GetComponent<Outline>().enabled = false;
    }

    public void HighlightResource(Resource resource, bool highlight)
    {
        resource.GetComponent<Outline>().OutlineWidth = resourceConfirmationOutlineWidth;
        resource.GetComponent<Outline>().enabled = highlight;
    }

    void SetToCanvasSpace(GameObject source, RectTransform prefab)
    {
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(source.transform.position);
        Vector2 unitObj_ScreenPosition = new Vector2(
        ((ViewportPosition.x * GatherCanvas.sizeDelta.x) - (GatherCanvas.sizeDelta.x * 0.5f)),
        ((ViewportPosition.y * GatherCanvas.sizeDelta.y) - (GatherCanvas.sizeDelta.y * 0.5f) + resourceGatherUXYDistance));

        prefab.anchoredPosition = unitObj_ScreenPosition;
    }
}
