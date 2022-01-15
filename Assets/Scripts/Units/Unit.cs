using UnityEngine;
using UnityEngine.AI;

// Houses all unit parameters and needed objects
public class Unit : MonoBehaviour
{
    public BaseUnit baseUnit = new BaseUnit(); // contains all unit stats and face graphics

    [HideInInspector] public NavMeshAgent agent; // set to the unit's NavMeshAgent for movement manipulation

    protected UnitProcessing up; // used for processing unit stat calculations
    protected UIProcessing uip; // used for processing default UI details
    protected UnitMovement um; // used for processing movement with NavMeshAgent  

    public Unit() // Constructor for initializing required vars when unit is created
    {
        baseUnit.SetEXP(0);
        baseUnit.SetLevel(1);
    }

    protected virtual void SetUnitProcessingVars() // Initializes default variables after unit is created
    {
        baseUnit.SetExpToNextLevel(Mathf.RoundToInt(baseUnit.GetLevel() * up.toNextLevelFactor));        
    }

    protected void UnitAwake() // Procedures to be called when unit is placed into the world
    {
        GetComponent<Outline>().OutlineWidth = uip.defaultOutlineWidth;
        GetComponent<Outline>().enabled = false;

        agent = GetComponent<NavMeshAgent>();
    }

    public float GetMoveSpeed() // Sets unit's movement speed for their NavMesh Agent
    {
        return (um.moveSpeedBaseline + (baseUnit.GetAgility() * um.moveSpeedAgilityFactor) + (baseUnit.GetMovement() * um.moveSpeedFactor));
    }
}
