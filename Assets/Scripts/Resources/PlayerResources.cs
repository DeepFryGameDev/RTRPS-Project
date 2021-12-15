using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public int wood;
    public int ore;
    public int food;
    public int gold;

    GameObject resourceUIParent;

    // Start is called before the first frame update
    void Start()
    {
        resourceUIParent = GameObject.Find("UI/Canvas/TopBar/ResourceSpacer");
    }

    // Update is called once per frame
    void Update()
    {
        ShowResourceCount();
    }

    private void ShowResourceCount()
    {
        resourceUIParent.transform.Find("Lumber/Count").GetComponent<TMP_Text>().text = wood.ToString();
        resourceUIParent.transform.Find("Ore/Count").GetComponent<TMP_Text>().text = ore.ToString();
        resourceUIParent.transform.Find("Food/Count").GetComponent<TMP_Text>().text = food.ToString();
        resourceUIParent.transform.Find("Gold/Count").GetComponent<TMP_Text>().text = gold.ToString();
    }
}
