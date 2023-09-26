using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
    /// <summary>
    /// Attach this component to a gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record position/rotation of the tracked VR devices at each frame.
    /// </summary>
    public class LocalDevicesTracker : Tracker
    {
        [Header("Tracked Objects")]
        [SerializeField] private SceneStateManager sceneStateManager;
        [SerializeField] private Transform hipTransform;
        [SerializeField] private Transform walkerTransform;

        /// <summary>
        /// Sets measurementDescriptor and customHeader to appropriate values
        /// </summary>
        protected override void SetupDescriptorAndHeader()
        {
            measurementDescriptor = "local_devices";

            customHeader = new string[]
            {
                    "hmd_pos_x",
                    "hmd_pos_y",
                    "hmd_pos_z",
                    "hmd_rot_x",
                    "hmd_rot_y",
                    "hmd_rot_z",
                    "hip_pos_x",
                    "hip_pos_y",
                    "hip_pos_z",
                    "hip_rot_x",
                    "hip_rot_y",
                    "hip_rot_z",
                    "walker_pos_x",
                    "walker_pos_y",
                    "walker_pos_z",
                    "walker_rot_x",
                    "walker_rot_y",
                    "walker_rot_z"
            };
        }

        /// <summary>
        /// Returns current position and rotation values
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            // get position and rotation
            var cameraTransform = sceneStateManager.FirstPersonCamera.transform;
            Vector3 hmdPos      = cameraTransform.localPosition;
            Vector3 hmdRot      = cameraTransform.localEulerAngles;
            Vector3 hipPos      = hipTransform.localPosition;
            Vector3 hipRot      = hipTransform.localEulerAngles;
            Vector3 walkerPos   = walkerTransform.localPosition;
            Vector3 walkerRot   = walkerTransform.localEulerAngles;

            // return position, rotation (x, y, z) as an array
            var values = new UXFDataRow()
                {
                    ("hmd_pos_x", hmdPos.x),
                    ("hmd_pos_y", hmdPos.y),
                    ("hmd_pos_z", hmdPos.z),
                    ("hmd_rot_x", hmdRot.x),
                    ("hmd_rot_y", hmdRot.y),
                    ("hmd_rot_z", hmdRot.z),
                    ("hip_pos_x", hipPos.x),
                    ("hip_pos_y", hipPos.y),
                    ("hip_pos_z", hipPos.z),
                    ("hip_rot_x", hipRot.x),
                    ("hip_rot_y", hipRot.y),
                    ("hip_rot_z", hipRot.z),
                    ("walker_pos_x", walkerPos.x),
                    ("walker_pos_y", walkerPos.y),
                    ("walker_pos_z", walkerPos.z),
                    ("walker_rot_x", walkerRot.x),
                    ("walker_rot_y", walkerRot.y),
                    ("walker_rot_z", walkerRot.z)
                };

            return values;
        }
    }

}