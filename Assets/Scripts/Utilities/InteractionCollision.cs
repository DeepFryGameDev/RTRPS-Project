using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionCollision : MonoBehaviour
{
    [ReadOnly] public bool isAtDest = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Unit") && other.GetComponent<VillagerUnit>() && (other.GetComponent<VillagerUnit>().buildTaskIsActive || other.GetComponent<VillagerUnit>().gatherTaskIsActive))
        {
            isAtDest = true;
            other.GetComponent<VillagerUnit>().isAtDest = true;
        }        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Unit") && other.GetComponent<VillagerUnit>() && (other.GetComponent<VillagerUnit>().buildTaskIsActive || other.GetComponent<VillagerUnit>().gatherTaskIsActive))
        {
            isAtDest = false;
            other.GetComponent<VillagerUnit>().isAtDest = false;
        }
    }
}
