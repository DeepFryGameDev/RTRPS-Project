using System.Collections.Generic;
using UnityEngine;

public class GathererActions : MonoBehaviour
{
    [Tooltip("List of all actions that can be used by the gatherer class")]
    public List<BaseAction> gathererActions = new List<BaseAction>();
}
