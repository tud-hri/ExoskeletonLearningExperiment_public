using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    /// <summary>
    /// Attach this component to a gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record the exo learning task data each frame.
    /// </summary>
    public class TaskDataTracker : Tracker
    {
        [Header("Tracked Objects")]
        [SerializeField] private TrunkInclinationInput trunkInclinationInput;
        [SerializeField] private HipThrustInput hipThrustInput;
        [SerializeField] private WeightShiftDetection weightShiftDetection;
        [SerializeField] private UXF_TaskManager ufx_TaskManager;

        protected override void SetupDescriptorAndHeader()
        {
            measurementDescriptor = "task_data";

            customHeader = new string[]
            {
                "trunk_inclination",
                "trigger",
                //"leading_leg",
                "score_weight_shifting",
                "steps", 
                "distance"
            };
        }

        protected override UXFDataRow GetCurrentValues()
        {
            //float leadingLeg = weightShiftingFeedback.leadingLeg / Mathf.Abs(weightShiftingFeedback.leadingLeg);      // Positive means right leg is leading leg

            var values = new UXFDataRow()
                {
                    ("trunk_inclination", trunkInclinationInput.GetAngleRaw()),
                    ("trigger", hipThrustInput.IsTriggered()),
                    //("leading_leg", leadingLeg), 
                    ("score_weight_shifting", weightShiftDetection.Correct), // Weight Shifting (binary)
                    ("steps", ufx_TaskManager.NumberSteps()),
                    ("distance", ufx_TaskManager.Distance())
                };

            return values;
        }
    }

}