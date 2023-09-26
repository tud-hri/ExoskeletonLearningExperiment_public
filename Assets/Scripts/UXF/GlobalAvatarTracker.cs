using UnityEngine;

namespace UXF
{
    /// <summary>
    /// Attach this component to a gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record position/rotation of the feet at each frame.
    /// </summary>
    public class GlobalAvatarTracker : Tracker
    {
        [Header("Tracked Objects")]
        [SerializeField] Transform rightFootTransform;
        [SerializeField] Transform leftFootTransform;
        [SerializeField] Transform hipFakeTransform;
        [SerializeField] Transform hipTransform;

        /// <summary>
        /// Sets measurementDescriptor and customHeader to appropriate values
        /// </summary>
        protected override void SetupDescriptorAndHeader()
        {
            measurementDescriptor = "global_avatar";

            customHeader = new string[]
            {
                "right_pos_x",
                "right_pos_y",
                "right_pos_z",
                "right_rot_x",
                "right_rot_y",
                "right_rot_z",
                "left_pos_x",
                "left_pos_y",
                "left_pos_z",
                "left_rot_x",
                "left_rot_y",
                "left_rot_z",
                "hip_pos_x",
                "hip_pos_y",
                "hip_pos_z",
                "hip_rot_x",
                "hip_rot_y",
                "hip_rot_z",
                "hip_fake_pos_x",
                "hip_fake_pos_y",
                "hip_fake_pos_z",
                "hip_fake_rot_x",
                "hip_fake_rot_y",
                "hip_fake_rot_z",
            };
        }

        /// <summary>
        /// Returns current position and rotation values
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            // get position and rotation
            Vector3 rightPos    = rightFootTransform.position;
            Vector3 rightRot    = rightFootTransform.eulerAngles;
            Vector3 leftPos     = leftFootTransform.position;
            Vector3 leftRot     = leftFootTransform.eulerAngles;
            Vector3 hipPos      = hipTransform.position;
            Vector3 hipRot      = hipTransform.eulerAngles;
            Vector3 hipFakePos  = hipFakeTransform.position;
            Vector3 hipFakeRot  = hipFakeTransform.eulerAngles;

            // return position, rotation (x, y, z) as an array
            var values = new UXFDataRow()
            {
                ("right_pos_x", rightPos.x),
                ("right_pos_y", rightPos.y),
                ("right_pos_z", rightPos.z),
                ("right_rot_x", rightRot.x),
                ("right_rot_y", rightRot.y),
                ("right_rot_z", rightRot.z),
                ("left_pos_x", leftPos.x),
                ("left_pos_y", leftPos.y),
                ("left_pos_z", leftPos.z),
                ("left_rot_x", leftRot.x),
                ("left_rot_y", leftRot.y),
                ("left_rot_z", leftRot.z),
                ("hip_pos_x", hipPos.x),
                ("hip_pos_y", hipPos.y),
                ("hip_pos_z", hipPos.z),
                ("hip_rot_x", hipRot.x),
                ("hip_rot_y", hipRot.y),
                ("hip_rot_z", hipRot.z),
                ("hip_fake_pos_x", hipFakePos.x),
                ("hip_fake_pos_y", hipFakePos.y),
                ("hip_fake_pos_z", hipFakePos.z),
                ("hip_fake_rot_x", hipFakeRot.x),
                ("hip_fake_rot_y", hipFakeRot.y),
                ("hip_fake_rot_z", hipFakeRot.z),
            };

            return values;
        }
    }
}
