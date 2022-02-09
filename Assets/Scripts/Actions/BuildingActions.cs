using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingActions : MonoBehaviour
{
    [Tooltip("List of all actions that can be used by buildings")]
    public List<BaseBuildingAction> buildingActions = new List<BaseBuildingAction>();
}
