using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

[RequireComponent(typeof(SteamVR_Behaviour_Pose))]
public class VRCalibrationWand : MonoBehaviour
{
    public UnityEvent<VRCalibrationWand> OnTriggerPress = new UnityEvent<VRCalibrationWand>();
    public UnityEvent<VRCalibrationWand> OnGripPress = new UnityEvent<VRCalibrationWand>();
    
    SteamVR_Behaviour_Pose controller;

    private void Start()
    {
        controller = GetComponent<SteamVR_Behaviour_Pose>();
    }

    private void Update()
    {
        if (SteamVR_Input.GetStateDown("GrabPinch",controller.inputSource))
        {
            Debug.Log("Pinching");
            OnTriggerPress.Invoke(this);
        }

        if (SteamVR_Input.GetStateDown("GrabGrip", controller.inputSource))
        {
            Debug.Log("Gripping");
            OnGripPress.Invoke(this);
        }
    }

    /// <summary>
    /// Returns scale required to match sourcePosition to trackedObject, given that the selected point by 
    /// trackedObject is on the opposite side of the origin from which you want to scale. This can in part 
    /// be overcome by using the scaleFactor to scale different axes with different values.
    /// </summary>
    /// <returns>Returns scale based on the passed values.</returns>
    public Vector3 GetScaleValue(Vector3 sourcePosition, Vector3 scaleFactor, bool scaleX = true, bool scaleY = true, bool scaleZ = true)
    {
        if (controller == null) Debug.LogError("Make sure to assign a tracked object which is used as the Calibration Wand!");

        Vector3 scale = Vector3.one;
        if (scaleX) scale.x = (controller.transform.position.x / sourcePosition.x) * scaleFactor.x;
        if (scaleY) scale.y = (controller.transform.position.y / sourcePosition.y) * scaleFactor.y;
        if (scaleZ) scale.z = (controller.transform.position.z / sourcePosition.z) * scaleFactor.z;


        return scale;
    }

    public Vector3 GetScaleValue(Vector3 sourcePosition, bool scaleX = true, bool scaleY = true, bool scaleZ = true)
    {
        return GetScaleValue(sourcePosition,new Vector3(1,1,1),scaleX,scaleY,scaleZ);
    }
}
