using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCalibration : MonoBehaviour
{
    [Space]
    [Header("   Camera Orientation Calibration ")]
    [Space]
    [Header("camera view aligned with the horizon.")]
    [Header("This script assumes you are keeping your ")]
    [Space(25)]
    [SerializeField] Valve.VR.SteamVR_TrackedObject pointAtTracker;
    [SerializeField] Transform cameraTransform;

    public void Calibrate() // This assumes that you are holding your camera straight wrt the horizon.
    {
        cameraTransform.LookAt(pointAtTracker.transform);
        var rot = cameraTransform.rotation.eulerAngles;
        rot.z = 0;
        cameraTransform.rotation = Quaternion.Euler(rot);
    }
}
