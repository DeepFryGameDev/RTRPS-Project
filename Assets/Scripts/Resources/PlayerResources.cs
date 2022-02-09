using TMPro;
using UnityEngine;

// This script houses the total number of resources for the player
public class PlayerResources : MonoBehaviour
{
    public int wood;
    public int ore;
    public int food;
    public int gold;

    UIPrefabManager uipm; // used to manipulate the player's resource count in the top bar UI panel

    private void Start()
    {
        uipm = FindObjectOfType<UIPrefabManager>();
    }

    void Update()
    {
        ShowResourceCount();
    }

    void ShowResourceCount() // shows total resource count in top bar UI panel
    {
        uipm.resourceUISpacer.transform.Find("Lumber/Count").GetComponent<TMP_Text>().text = wood.ToString();
        uipm.resourceUISpacer.transform.Find("Ore/Count").GetComponent<TMP_Text>().text = ore.ToString();
        uipm.resourceUISpacer.transform.Find("Food/Count").GetComponent<TMP_Text>().text = food.ToString();
        uipm.resourceUISpacer.transform.Find("Gold/Count").GetComponent<TMP_Text>().text = gold.ToString();
    }

    public void RemoveResources(int wood, int ore, int food, int gold)
    {
        this.wood -= wood;
        this.ore -= ore;
        this.food -= food;
        this.gold -= gold;
    }
}
