using UnityDataReplay.ReplayerTypes;
using UnityEngine;

public class WalkerMotion : MonoBehaviour
{
    // Serialized Properties
    [Header("Inputs")]
    [SerializeField] private StepReplayManager stepReplayManager; // to check if we are playing an animation
    [SerializeField] private HipThrustInput hipThrustInput; // to check if we are doing a hipthrust
    [SerializeField] private WeightShiftingInput weightShiftingInput; // We use this to find the middlepoint between the two feet
    [Header("Transforms we need")]
    [SerializeField] private Transform virtualWalker;
    [SerializeField] private Transform matchingWalker;
    [SerializeField] private Transform walkerTracker;
    [SerializeField] private Transform hipTracker;
    [Header("Tweaking Settings")]
    [SerializeField] private Vector3 walkerOffsetAvatar = new Vector3(0.095f, 0f, -0.01f);

    // Private
    private float _trackerDisplacement;
    private float _trackerCalibrationPos;
    private Vector3 _virtWalkerOffset;

    // Previous Step
    private float _frontFootWalkerDistancePrevStep;
    private float _zVirtWalkerPrevStep;
    private float _zWalkerPrevStep          => stepReplayManager.GetLegPos(stepReplayManager.FrontLeg()) + _frontFootWalkerDistancePrevStep;

    // Walker Matching
    private bool _walkersMatched;
    private bool _initialized = false;
    private float _factor;

    // Public walker position properties
    public bool DoingWalkerMatching => !_walkersMatched;
    public float WalkerOffsetZ
    {
        get => walkerOffsetAvatar.z;
        set => walkerOffsetAvatar.z = value;
    }
    public float FrontFootWalkerDistance    => virtualWalker.position.z - stepReplayManager.GetLegPos(stepReplayManager.FrontLeg());
    public float BackFootWalkerDistance     => virtualWalker.position.z - stepReplayManager.GetLegPos(stepReplayManager.BackLeg());// + AdditionalZOffset; (0.15)

    // Private walker position properties
    private Vector3 trackerDisplacement      => new Vector3(0, 0, _trackerDisplacement);
    private Vector3 VirtualWalkerBasePos    => weightShiftingInput.GetMiddlePoint() + _virtWalkerOffset;
    private Vector3 VirtualWalkerPos        => VirtualWalkerBasePos + trackerDisplacement;

    private Vector3 ScaledVirtualWalkerPos
    {
        get
        {
            Vector3 walkerPos = VirtualWalkerPos;
            walkerPos.z += _factor * (walkerPos.z - (_zVirtWalkerPrevStep)); // + stepReplayManager.OptimalStepLength / 2); // prevStepLength

            return walkerPos;
        }
    }

    void Update()
    {
        // Unless there is some reason to not move the walker...
        if (!_initialized) return;
        if (hipThrustInput.IsThrustStarted()) return; // Once the thrust started, we freeze the walker.
        if (stepReplayManager.IsPlaying()) return; // If we are playing the animation, we don't need to move it either.

        // We move the walker every frame based on the position of the tracker
        MoveWalker(); 
    }

    public void InitialCalibration()
    {
        // Calibrate initial walker tracker pos
        var walkerPos = walkerTracker.position.z;
        _trackerCalibrationPos = walkerPos;

        // Find the walker offset
        float initialWalkerOffset = walkerPos - hipTracker.position.z;
        _virtWalkerOffset   =   walkerOffsetAvatar;
        _virtWalkerOffset.z +=  initialWalkerOffset;
        virtualWalker.position = VirtualWalkerBasePos;

        _walkersMatched = true;
        _initialized    = true;
    }


    private void MoveWalker()
    {
        // Find the displacement to use externally
        _trackerDisplacement = walkerTracker.position.z - _trackerCalibrationPos;

        if (_walkersMatched) 
            virtualWalker.position = VirtualWalkerPos; // move the walker
        else // we need to match positions still
        {
            matchingWalker.position = ScaledVirtualWalkerPos;

            bool posMatching = (matchingWalker.position.z - _zVirtWalkerPrevStep) < 0.03f;
            if (!posMatching) return;
            
            matchingWalker.gameObject.SetActive(false);
            _walkersMatched = true;
        }
    }

    public void WalkersMatchedManual()
    {
        matchingWalker.gameObject.SetActive(false);
        _walkersMatched = true;
    }

    private float FindMovementScalingFactor()
    {
        float distanceTrackerLimit          = Mathf.Abs(walkerTracker.position.z - _trackerCalibrationPos);
        float distanceBetweenWalkers        = Mathf.Abs(_zWalkerPrevStep - _zVirtWalkerPrevStep);
        return Mathf.Max(1, distanceBetweenWalkers / distanceTrackerLimit); // We only need to scale it if its too far away
    }

    public void StepTriggeredActions() // Do these steps if a step event is invoked (on the StepReplayManager)
    {
        _frontFootWalkerDistancePrevStep = FrontFootWalkerDistance;
        _zVirtWalkerPrevStep = VirtualWalkerPos.z;
    }

    public void StepFinishedActions()
    {
        _factor = FindMovementScalingFactor();
        _walkersMatched = false;
    }
    
    public void ShowWalker()
    {
        matchingWalker.gameObject.SetActive(true);
    }
}
