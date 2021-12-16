using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GatherManager : MonoBehaviour
{
    [Tooltip("Duration in seconds of outline when clicking a resource")]
    [SerializeField] float resourceConfirmationOutlineDuration;
    [Tooltip("Width of outline when clicking a resource")]
    [SerializeField] [Range(1, 10)] float resourceConfirmationOutlineWidth;
    [Tooltip("Set to resource gather UX feedback prefab in UI")]
    [SerializeField] GameObject resourceGatherUX;
    [Tooltip("Distance of pixels above unit that UX will appear")]
    [SerializeField] float resourceGatherUXYDistance;
    [Tooltip("Default Scale of UX Feedback")]
    [SerializeField] float resourceGatherUXBaseScale;
    [Tooltip("Speed of UX feedback on screen moving upwards along y position")]
    [SerializeField] float resourceGatherUXSpeed;
    [Tooltip("How quickly the UX Feedback fades")]
    [SerializeField] float resourceGatherUXFadeFactor;

    UIProcessing uip;

    private void Start()
    {
        uip = FindObjectOfType<UIProcessing>();
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

        gatherUX = Instantiate(gatherUX, pos, Quaternion.identity, transform.Find("Canvas"));
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

    void SetToCanvasSpace(GameObject source, RectTransform prefab)
    {
        RectTransform CanvasRect = transform.Find("Canvas").GetComponent<RectTransform>();

        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(source.transform.position);
        Vector2 unitObj_ScreenPosition = new Vector2(
        ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
        ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f) + resourceGatherUXYDistance));

        prefab.anchoredPosition = unitObj_ScreenPosition;
    }
}
