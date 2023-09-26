using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class HipCalibration : MonoBehaviour
{

    [SerializeField] VRIK vrik;
    [SerializeField] GameObject CameraRig;
    [SerializeField] WeightShiftingInput weightShiftingInput;
    [SerializeField] Vector3 additionalOffset;


    // Calibration to match the position between the user (Real World) and Avatar (Virtual World)
    public void CalibrateHip()
    {
        Vector3 diff = vrik.solver.spine.pelvisTarget.position - weightShiftingInput.GetMiddlePoint(); // Distance between pelvis (user) and Feet Middle point (avatar)
        diff.y = 0;
        diff += additionalOffset; // Extra offset to increase accuracy
        CameraRig.transform.position -= diff;
    }
}
