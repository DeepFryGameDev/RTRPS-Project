using System.Collections.Generic;
using UnityEngine;

public class BuilderActions : MonoBehaviour
{
    [Tooltip("List of all actions that can be used by the builder class")]
    public List<BaseAction> builderActions = new List<BaseAction>();
}
