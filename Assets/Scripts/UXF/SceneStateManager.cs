using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneStateManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> visualCues;
    [SerializeField] private Camera firstPersonCamera;
    [SerializeField] private Camera lateralCamera;
    [SerializeField] private Camera backCamera;

    public Camera ActiveCamera { get; private set; }
    public Camera FirstPersonCamera => firstPersonCamera;

    public void SetVisualCues(bool state)
    {
        foreach (var cue in visualCues) cue.SetActive(state);
    }
    
    public void SetPerspective(Perspective perspective)
    {
        switch (perspective)
        {
            case Perspective.FirstPerson:
                ActiveCamera = firstPersonCamera;
                firstPersonCamera.enabled = true;
                lateralCamera.enabled = false;
                backCamera.enabled = false;
                break;
            case Perspective.Lateral:
                ActiveCamera = lateralCamera;
                firstPersonCamera.enabled = false;
                lateralCamera.enabled = true;
                backCamera.enabled = false;
                break;
            case Perspective.Back: 
                ActiveCamera = backCamera;
                firstPersonCamera.enabled = false;
                lateralCamera.enabled = false;
                backCamera.enabled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(perspective), perspective, null);
        }
    }

    public Perspective StringToPerspective(string key)
    {
        return key switch
        {
            "FirstPerson" => Perspective.FirstPerson,
            "Lateral" => Perspective.Lateral,
            "Back" => Perspective.Back,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }
    
}

public enum Perspective
{
    FirstPerson,
    Lateral,
    Back
}