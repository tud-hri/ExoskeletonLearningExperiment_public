using System.Globalization;
using UnityEngine;
using UnityDataReplay.ReplayerTypes;
using System;
using UnityEngine.Events;


// This class handles the detection of hip thrust input and triggers appropriate events based on the input.
public class HipThrustInput : MonoBehaviour
{
    // Serialized fields for configuration in the Unity Editor.
    [SerializeField] private bool usePeakAcc = true; // Determines if peak acceleration should be used.

    // References to other components.
    [SerializeField] private WalkerInput walkerInput;
    [SerializeField] private WeightShiftDetection weightShiftDetection;
    [SerializeField] private TrunkInclinationInput trunkInclination;

    // Unity events that are triggered based on different conditions.
    [Tooltip("Triggered each time a step is taken. Passes the final scaled velocity during the thrust.")]
    public UnityEvent<HipThrust> OnHipThrust = new UnityEvent<HipThrust>();
    [Tooltip("Triggered each time max velocity is reached. Passes the final scaled velocity during the thrust.")]
    public UnityEvent<HipThrust> OnMaxReached = new UnityEvent<HipThrust>();
    public UnityEvent<HipThrust> OnWalkerCollision = new UnityEvent<HipThrust>();

    // Configuration values for velocity and acceleration.
    [SerializeField] public float minVel = 0.01f;
    [SerializeField] public float maxVel = 0.05f;
    [SerializeField] public float minAcc = 0.10f;
    [SerializeField] public float maxAcc = 0.40f;

    // Thresholds for triggering steps and detecting thrust distance.
    [SerializeField] private float thresholdTriggerStep = 5f;
    [SerializeField] private float thrustDistanceThreshold = 0.02f; 
    [SerializeField] private Transform hipTarget; // Reference to the hip target transform.

    // IMU related fields.
    [SerializeField] private Transform imuTransform;
    [SerializeField] private float gravityScaling = 1.0f;

    // Private fields for internal state management.
    private StepReplayManager _stepReplayManager;
    private float _velocity;
    private float _peakAcc;
    private static Vector3 accVecRaw;
    private static Vector3 accVec;
    private float[] _accWindow = new float[3];
    private bool _isTriggered;
    private bool _thrustStarted;
    private bool _minDistanceReached;

    // Cooldown related fields.
    [SerializeField] private float thrustCooldown = 0.1f; // Cooldown duration in seconds.
    private float _cooldown;
    private float _thrustStartPos;

    // Helper methods to get the current state.
    public bool IsTriggered() => _isTriggered;
    public bool IsThrustStarted() => _thrustStarted;
    private float _scaledInput;
    public float ScaledInput() => _scaledInput;
    public bool MinDistanceReacher() => _minDistanceReached;
    public Vector3 SendAccRaw() => accVecRaw; // Methods to send acceleration data.
    public Vector3 SendAcc() => accVec; // Methods to send acceleration data.

    private void Start()
    {
        // Initialize acceleration window.
        _accWindow[0] = 0; _accWindow[1] = 0; _accWindow[2] = 0;

        // Get reference to the StepReplayManager in the scene.
        _stepReplayManager = FindObjectOfType<StepReplayManager>();
        if (_stepReplayManager == null) Debug.LogError("Could not find StepReplayManager, is it in the scene?");
    }

    private void Update()
    {
        // Check various conditions before processing hip thrust input.
        if (!UXF.Session.instance.InTrial) return; // Exit if not in a trial.
        if (_cooldown > 0)
        {
            _cooldown -= Time.deltaTime;
            return;
        }
        if (_stepReplayManager.IsPlaying()) return;
        if (walkerInput.DoingWalkerMatching()) return;

        if (weightShiftDetection.Correct || _thrustStarted)
            CheckTrigger();
    }


    // Reading data from Delsys Sensor
    public void SensorDataDelsys(Packet packet)
    {
        var ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.NumberDecimalSeparator = ",";

        var strings = packet.ReadStringArray();
        var floats = new float[3];

        for (var i = 0; i < 3; i++)
        {
            floats[i] = float.Parse(strings[i], ci);
        }
        accVecRaw = new Vector3(floats[0], floats[1], floats[2]);
        
        Debug.Log("Reading IMU data " + (accVecRaw.x != 0));
        // Transform the acc vector to world space and subtract the gravity component
        accVec = imuTransform.localToWorldMatrix.MultiplyVector(accVecRaw) - Vector3.down*gravityScaling;
    }

    // Method to check if the conditions for triggering a hip thrust are met.
    public void CheckTrigger()
    {   
        _isTriggered = false;
        
        // Get acceleration in z direction
        float acc = accVec.z;
        
        // Check hip translation in Z
        Vector3 hipPos = hipTarget.position;

        bool checkStart = acc >= thresholdTriggerStep && _thrustStarted == false;
        if (checkStart)
        {
            _thrustStartPos = hipPos.z;
            _thrustStarted = true;
            Debug.Log("Starting thrust");
        }
        if (!_thrustStarted) return;

        _velocity += acc * Time.deltaTime; // The accumulated velocity vector

        // Create the HipThrust here already so that we can use it for the visual cue input
        if (_peakAcc < acc) _peakAcc = acc;
        var maxVal      = usePeakAcc ? maxAcc : maxVel;         // save the final value
        var minVal      = usePeakAcc ? minAcc : minVel;         // save the final value
        var thrustInput = usePeakAcc ? _peakAcc : _velocity;    // save the final value
        var hipThrust = new HipThrust(thrustInput, minVal,maxVal);

        // Set some more params for the hipthrust]
        hipThrust.thrustDistanceThreshold = thrustDistanceThreshold;
        hipThrust.thrustDistance = hipTarget.position.z - _thrustStartPos;
        if (hipThrust.thrustDistance < 0) acc = 0; // We don't want to register any backwards thrusts. just cancel the movement
        _minDistanceReached = hipThrust.MinDistanceReached;
        
        _scaledInput = hipThrust.ScaledVal;

        // Detect if the hipthrust has ended
        bool belowThresholdDetection = acc < thresholdTriggerStep;
        if (!belowThresholdDetection) return;

        _cooldown = thrustCooldown; // activate the cooldown

        // Thrust end detected
        _thrustStarted = false;
        _velocity = 0; // Reset the velocity back to 0
        _peakAcc = 0;

        // Set the extra params for the hipThrust
        hipThrust.thrustDistance    = hipTarget.position.z - _thrustStartPos;
        hipThrust.trunkScaledInput  = trunkInclination.ScaledInput();
        hipThrust.scaledWalkerInput = walkerInput.ScaledInput();
        
        OnMaxReached.Invoke(hipThrust); // This triggers, even though the step might not trigger (due to walker collision for example)
        
        // Check threshold value 
        if (!hipThrust.MinDistanceReached)
        {
            Debug.LogWarning("Not enough movement in z, total distance = " + hipThrust.thrustDistance + "[m]");
            ResetInputVal();
            return;
        }
        if (!hipThrust.MinValReached)
        {
            Debug.LogWarning($"Not enough {(usePeakAcc ? "acceleration" : "velocity" )} accumulated (even though enough movement)");
            ResetInputVal();
            // Trigger error cue ???
            return;
        }

        TriggerDetection(hipThrust);
    }

    public void ResetInputVal() { _scaledInput = 0; }

    // Method to handle the detection of a hip thrust.
    public void TriggerDetection(HipThrust hipThrust)
    {
        // Check walker input
        if (hipThrust.LimitedByWalker)
        {
            OnWalkerCollision.Invoke(hipThrust);
            Debug.LogWarning("Colliding with walker, not taking a step");
            ResetInputVal();
            return;
        }
        
        OnHipThrust.Invoke(hipThrust); // Successful hip thrust triggered here

        hipThrust.Msg();
        _isTriggered = true;
    }

    public void DebugHipThrust(float _scaledVal = 0.666f)
    {
        var hipThrust = new HipThrust(_scaledVal, 0.0f, 1.0f)
        {
            thrustDistance = 1.0f,
            trunkScaledInput = 1.0f,
            scaledWalkerInput = 1.0f,
        };
        
        OnMaxReached.Invoke(hipThrust);
        TriggerDetection(hipThrust);
        _isTriggered = true;
    }
}

// Holds all the information for a single hipthrust, this can be easily sent around for logging, and triggering a step.
[Serializable]
public class HipThrust
{
    // Required values
    public float thrustInput;
    public float usedMax;
    public float usedMin;
    
    // Extra values
    public float thrustDistance;
    public float trunkScaledInput;
    public float scaledWalkerInput;

    public float thrustDistanceThreshold = 0.02f; // [m]
    public bool MinDistanceReached => thrustDistanceThreshold < thrustDistance; // Check hip thrust distance threshold (to prevent tiny super high frequency movements, which are probably noise)

    // Calculated values
    public float ScaledVal => ExtensionMethods.Remap(thrustInput, 0, usedMax, 0, 1);

    // Some useful checks
    public bool LimitedByWalker => scaledWalkerInput < ScaledVal || thrustInput > usedMax; // If its above the usedMax, it should really be limited by the walker
    public bool MinValReached => usedMin < thrustInput;

    public HipThrust(float _thrustInput, float _usedMin, float _usedMax)
    {
        thrustInput = _thrustInput;
        usedMin = _usedMin;
        usedMax = _usedMax;
    }

    public void Msg()
    {
        Debug.Log($"Hip thrust done with input: {thrustInput}  \n  scaledVal: {ScaledVal}  |  thrustDistance: {thrustDistance}   |   trunkScaledInput {trunkScaledInput}");
    }
}