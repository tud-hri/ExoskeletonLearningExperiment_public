using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogAdditionalDetails : MonoBehaviour
{
    private UnityDataReplay.ReplayerTypes.StepReplayManager stepReplayManager;

    // Start is called before the first frame update
    void Start()
    {
        stepReplayManager = FindObjectOfType<UnityDataReplay.ReplayerTypes.StepReplayManager>();
    }

    public void LogPlayerHeight()
    {
        if (UXF.Session.instance.participantDetails.ContainsKey("playerheight"))
        {
            UXF.Session.instance.participantDetails["playerheight"] = stepReplayManager.UserHeight;
            UXF.Session.instance.participantDetails["optimal_stepsize"] = stepReplayManager.OptimalStepLength;
        }
        else
        {
            UXF.Session.instance.participantDetails.Add("playerheight", stepReplayManager.UserHeight);
            UXF.Session.instance.participantDetails.Add("optimal_stepsize", stepReplayManager.OptimalStepLength);
        }
    }
}
