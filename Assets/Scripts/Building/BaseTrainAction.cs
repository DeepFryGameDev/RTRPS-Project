using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTrainAction : BaseBuildingAction
{
    float progress;

    public float GetProgress() { return progress; }
    public void SetProgress(float val) { progress = val; }
}
