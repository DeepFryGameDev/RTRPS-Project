using UnityEngine;

// used for any building in the game not currently in the process of being created
public class CompletedBuilding : MonoBehaviour
{
    // contains the building's parameters
    [HideInInspector] public BaseBuilding building;
}
