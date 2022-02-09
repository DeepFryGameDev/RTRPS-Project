using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// used for any building in the game not currently in the process of being created
public class CompletedBuilding : MonoBehaviour
{
    // contains the building's parameters
    [HideInInspector] public BaseBuilding building;

    [HideInInspector] public bool isTraining;
    [HideInInspector] public float trainingTimeElapsed;

    [HideInInspector] public List<BaseTrainAction> queuedTrainActions = new List<BaseTrainAction>();
    [HideInInspector] public BaseTrainAction currentTrainAction;

    Coroutine trainingCoroutine;
    Coroutine trainSimTimeCoroutine;

    
    bool trainCompleted, inSim;    

    BiomeTile tile;

    DeepFryUtilities dfu;
    TrainingManager tm;

    void Awake()
    {
        dfu = FindObjectOfType<DeepFryUtilities>();
        tm = FindObjectOfType<TrainingManager>();

        tile = new BiomeTile();
        tile.primaryBiomeType = BiomeTypes.PLAINS; // for testing
    }

    void Update()
    {
        PrepareTraining();
    }

    void PrepareTraining()
    {
        if (queuedTrainActions.Count > 0 && currentTrainAction != queuedTrainActions[0])
        {
            currentTrainAction = queuedTrainActions[0];

            if (dfu.IfPlayerHasAvailableResources(currentTrainAction.woodRequired, currentTrainAction.oreRequired, currentTrainAction.foodRequired, currentTrainAction.goldRequired))
            {
                isTraining = true;
                trainingCoroutine = StartCoroutine(ProcessTraining());
            }
        }
    }

    IEnumerator ProcessTraining()
    {
        while (isTraining)
        {
            if (!inSim)
            {
                Debug.Log("Training - " + currentTrainAction.actionScript);

                trainCompleted = false;
                yield return trainSimTimeCoroutine = StartCoroutine(SimTrainTime()); // simulate training time

                if (trainCompleted)
                {
                    //Debug.Log("Spawn unit");

                    SpawnUnit(GetSpawnPointLocation(), queuedTrainActions[0].trainedUnitPrefab);
                    queuedTrainActions.RemoveAt(0);
                }

                if (queuedTrainActions.Count == 0)
                {
                    isTraining = false;
                }                
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator SimTrainTime() // Simulates the amount of time it takes to gather a resource
    {
        inSim = true;
        trainingTimeElapsed = 0;

        while (trainingTimeElapsed < GetTrainTime())
        {
            trainingTimeElapsed += Time.deltaTime;
            currentTrainAction.SetProgress(trainingTimeElapsed / GetTrainTime()); // may need to be updated

            yield return new WaitForEndOfFrame();
        }

        inSim = false;
        trainCompleted = true;
    }

    public float GetTrainTime()
    {
        float tempRate;

        tempRate = tm.maxBaseTrainTime - (GetBiomeTrainTimeStrength() * tm.baseTrainTimeFactor);

        tempRate *= currentTrainAction.trainDifficulty;

        if (tempRate < tm.minBaseTrainTime)
            return tm.minBaseTrainTime;

        return tempRate;
    }

    float GetBiomeTrainTimeStrength()
    {
        switch (tile.primaryBiomeType)
        {
            case BiomeTypes.PLAINS:
                return currentTrainAction.trainPlainsStrength;
            case BiomeTypes.BEACH:
                return currentTrainAction.trainBeachStrength;
            default:
                return 0;
        }
    }

    public bool ReachedMaxNumberInTrainingQueue()
    {
        return queuedTrainActions.Count >= tm.maxTrainingActions;
    }

    void SpawnUnit(Vector3 location, GameObject unitObj)
    {
        GameObject newUnit = Instantiate(unitObj, location, Quaternion.identity, tm.unitsParent);
        // walk a short distance
        newUnit.GetComponent<NavMeshAgent>().SetDestination(GetWalkPointAfterSpawn(newUnit.transform.position));
    }

    Vector3 GetWalkPointAfterSpawn(Vector3 unitLocation)
    {
        Vector3 loc = new Vector3(0, 0, 0);

        bool found = false;

        while (!found)
        {
            bool hitSomething = false;

            Random.InitState((int)System.DateTime.Now.Ticks);

            loc = new Vector3(unitLocation.x + Random.Range(-tm.maxTrainWalkPointDistance, tm.maxTrainWalkPointDistance),
                unitLocation.y,
                unitLocation.z + Random.Range(-tm.maxTrainWalkPointDistance, tm.maxTrainWalkPointDistance));

            RaycastHit[] hits;
            hits = hits = Physics.RaycastAll(loc, transform.up, 30f);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.CompareTag("CompletedBuilding") || hit.transform.CompareTag("Resource") || hit.transform.CompareTag("BuildingInProgress") || hit.transform.CompareTag("BuildingInProgressChild") ||
                    hit.transform.CompareTag("Blueprint") || hit.transform.CompareTag("BlueprintBuilding") || hit.transform.CompareTag("Building") || hit.transform.CompareTag("UnitInteraction"))
                {
                    //Debug.Log("Hit something: " + hit.transform.gameObject.name + " - unable to walk here - " + loc);
                    hitSomething = true;
                }
            }

            if (!hitSomething)
            {
                found = true;
            }
        }

        return loc;
    }

    Vector3 GetSpawnPointLocation()
    {
        Vector3 loc = new Vector3(0, 0, 0);              

        foreach (Transform child in transform.Find("SpawnPoints"))
        {
            bool ableToSpawn = false;
            bool hitSomething = false;

            RaycastHit[] hits;
            hits = hits = Physics.RaycastAll(child.transform.position, transform.up, 30f);
            foreach (RaycastHit hit in hits)
            {
                hitSomething = true;
                //Debug.Log("Checking hit from " + child.gameObject.name + " on " + hit.transform.gameObject.name);

                if (hit.transform.CompareTag("CompletedBuilding") || hit.transform.CompareTag("Resource") || hit.transform.CompareTag("BuildingInProgress") || hit.transform.CompareTag("BuildingInProgressChild") ||
                    hit.transform.CompareTag("Blueprint") || hit.transform.CompareTag("BlueprintBuilding") || hit.transform.CompareTag("Building"))
                {
                    //Debug.Log("Hit something on " + child.gameObject.name + " - unable to spawn here.");
                    ableToSpawn = false;
                    break;
                } else
                {
                    ableToSpawn = true;

                    break;
                }
            }

            if (ableToSpawn || !hitSomething)
            {
                return child.transform.position;
            }
                           
        }

        return loc;
    }
}
