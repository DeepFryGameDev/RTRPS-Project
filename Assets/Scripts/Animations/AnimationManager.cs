using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] Animator unitCanvasAnim;
    [SerializeField] Animator buildingCanvasAnim;
    [SerializeField] Animator resourceCanvasAnim;
    [SerializeField] Animator buildActionPanelAnim;
    [SerializeField] Animator multiUnitPanelAnim;
    [SerializeField] Animator optionsPanelAnim;

    public void ProcessOpenAnim(GameObject obj, bool show)
    {
        if (show)
        {            
            if (!GetIsOpen(obj))
            {
                GetAnimator(obj).SetBool("IsOpen", true);
            }
        } else
        {
            if (GetIsOpen(obj))
            {
                GetAnimator(obj).SetBool("IsOpen", false);
            }
        }
    }

    public void HideObjects()
    {
        GetCanvasGroup(unitCanvasAnim).alpha = 0;
        GetCanvasGroup(unitCanvasAnim).interactable = false;
        GetCanvasGroup(unitCanvasAnim).blocksRaycasts = false;

        GetCanvasGroup(buildingCanvasAnim).alpha = 0;
        GetCanvasGroup(buildingCanvasAnim).interactable = false;
        GetCanvasGroup(buildingCanvasAnim).blocksRaycasts = false;

        GetCanvasGroup(resourceCanvasAnim).alpha = 0;
        GetCanvasGroup(resourceCanvasAnim).interactable = false;
        GetCanvasGroup(resourceCanvasAnim).blocksRaycasts = false;

        GetCanvasGroup(buildActionPanelAnim).alpha = 0;
        GetCanvasGroup(buildActionPanelAnim).interactable = false;
        GetCanvasGroup(buildActionPanelAnim).blocksRaycasts = false;

        GetCanvasGroup(multiUnitPanelAnim).alpha = 0;
        GetCanvasGroup(multiUnitPanelAnim).interactable = false;
        GetCanvasGroup(multiUnitPanelAnim).blocksRaycasts = false;

        GetCanvasGroup(optionsPanelAnim).alpha = 0;
        GetCanvasGroup(optionsPanelAnim).interactable = false;
        GetCanvasGroup(optionsPanelAnim).blocksRaycasts = false;        
    }

    public void ShowUnitCanvas()
    {
        GetCanvasGroup(unitCanvasAnim).alpha = 1;
    }

    public void ShowBuildingCanvas()
    {
        GetCanvasGroup(buildingCanvasAnim).alpha = 1;
    }

    public void ShowResourceCanvas()
    {
        GetCanvasGroup(resourceCanvasAnim).alpha = 1;
    }

    public void ShowBuildActionPanel()
    {

        GetCanvasGroup(buildActionPanelAnim).alpha = 1;
    }

    public void ShowMultiUnitPanel()
    {
        GetCanvasGroup(multiUnitPanelAnim).alpha = 1;
    }

    public void ShowOptionsPanel()
    {
        GetCanvasGroup(optionsPanelAnim).alpha = 1;
    }

    public bool GetIsOpen(GameObject obj)
    {
        return obj.GetComponent<Animator>().GetBool("IsOpen");
    }

    CanvasGroup GetCanvasGroup(Animator anim)
    {
        return anim.GetComponent<CanvasGroup>();
    }

    Animator GetAnimator(GameObject obj)
    {
        return obj.GetComponent<Animator>();
    }
}
