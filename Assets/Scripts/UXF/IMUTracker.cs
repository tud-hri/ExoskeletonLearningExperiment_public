using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    /// <summary>
    /// Attach this component to a gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record IMU values recorded in each frame.
    /// </summary>
    public class IMUTracker : Tracker
    {
        [Header("Tracked Objects")]
        [SerializeField] private HipThrustInput hipThrustInput;

        /// <summary>
        /// Sets measurementDescriptor and customHeader to appropriate values
        /// </summary>
        protected override void SetupDescriptorAndHeader()
        {
            measurementDescriptor = "vr_transform";

            customHeader = new string[]
            {
                    "acc_x",
                    "acc_y",
                    "acc_z",
                    "acc_raw_x",
                    "acc_raw_y",
                    "acc_raw_z"
            };
        }

        /// <summary>
        /// Returns current IMU values
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            // get acc values
            var acc = hipThrustInput.SendAcc();
            var accRaw = hipThrustInput.SendAccRaw();

            var values = new UXFDataRow()
                {
                    ("acc_x", acc.x),
                    ("acc_y", acc.y),
                    ("acc_z", acc.z),
                    ("acc_raw_x", accRaw.x),
                    ("acc_raw_y", accRaw.y),
                    ("acc_raw_z", accRaw.z),
                };

            return values;
        }
    }

}