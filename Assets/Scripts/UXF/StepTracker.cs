using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    public class StepTracker : Tracker
    {
        // Step params
        private float step_length;
        private float step_score;
        private string stepping_leg;
        private float current_optimal_step_length;

        public void DoStep(Step step)
        {
            step_length = step.StepLength; // [m]
            stepping_leg = step.SteppingLeg.ToString();
            step_score = step.Score;
            current_optimal_step_length = step.CurrentOptimalStepLength; // [m]

            if (Recording) // Session.instance.InTrial
                RecordRow();
        }
        
        protected override void SetupDescriptorAndHeader()
        {
            measurementDescriptor = "step_data";

            customHeader = new string[]
            {
                // step params
                "step_length",
                "stepping_leg",
                "step_score",
                "current_optimal_step_length",
            };
        }

        protected override UXFDataRow GetCurrentValues()
        {

            var values = new UXFDataRow()
            {
                // Step params
                ("step_length",step_length),
                ("stepping_leg",stepping_leg),
                ("step_score",step_score),
                ("current_optimal_step_length",current_optimal_step_length),
            };

            return values;
        }

    }
}
