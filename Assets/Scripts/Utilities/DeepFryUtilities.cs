using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeepFryUtilities : MonoBehaviour
{
    PlayerResources pr;

    private void Start()
    {
        pr = FindObjectOfType<PlayerResources>();
    }

    public bool IfPlayerHasAvailableResources(int woodRequired, int oreRequired, int foodRequired, int goldRequired) // changes icon color and sets resourcesAvailable if player has enough resources for the action's requirements
    {
        if (pr.wood >= woodRequired &&
            pr.ore >= oreRequired &&
            pr.food >= foodRequired &&
            pr.gold >= goldRequired)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
