using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    public class ThrustTracker : Tracker
    {
        // Thrust params
        private float peak_acc;
        private float used_min;
        private float used_max;
        private float scaled_val;

        private float thrust_distance;
        private float trunk_input;
        private float walker_input;
        private bool walker_collision;

        public void DoThrust(HipThrust hipThrust)
        {
            peak_acc = hipThrust.thrustInput;
            used_min = hipThrust.usedMin;
            used_max = hipThrust.usedMax;
            scaled_val = hipThrust.ScaledVal;

            thrust_distance = hipThrust.thrustDistance;
            trunk_input = hipThrust.trunkScaledInput;
            walker_input = hipThrust.scaledWalkerInput;
            walker_collision = hipThrust.LimitedByWalker;
            
            if (Recording) // Session.instance.InTrial
                RecordRow();
        }

        protected override void SetupDescriptorAndHeader()
        {
            measurementDescriptor = "thrust_data";

            customHeader = new string[]
            {
                // thrust params
                "peak_acc",
                "used_min",
                "used_max",
                "scaled_val",

                "thrust_distance",
                "trunk_input",
                "walker_input",
                
                "walker_collision"
            };
        }

        protected override UXFDataRow GetCurrentValues()
        {

            var values = new UXFDataRow()
            {
                // Thrust params
                ("peak_acc",peak_acc),
                ("used_min",used_min),
                ("used_max",used_max),
                ("scaled_val",scaled_val),

                ("thrust_distance",thrust_distance),
                ("trunk_input",trunk_input),
                ("walker_input",walker_input),
                
                ("walker_collision",walker_collision),
            };

            return values;
        }

    }
}
