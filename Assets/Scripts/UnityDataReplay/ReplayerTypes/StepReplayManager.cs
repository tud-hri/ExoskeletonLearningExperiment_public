using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UnityDataReplay.ReplayerTypes
{
    public class StepReplayManager : MonoBehaviour
    {
        /// <summary>
        /// Event(s) to trigger when a step has been taken. Can pass the Step class instance as a dynamic argument (containing some details about the step)
        /// </summary>
        /// <returns></returns>
        /// 
        [SerializeField] float totalReplayTime = 1.65f;

        [SerializeField] private GaitFeedback _gaitFeedback;
        [Tooltip("Items in this event will be triggered each time a step is being taken.")]
        public StepEvent onDoStep = new StepEvent();

        public UnityEvent onFinishStep = new UnityEvent();

        [Header("Specific Settings")] 
        [SerializeField] private Transform globalWalkingDirection; 
        [SerializeField] public float stepWidth = 0.30f; 
        [SerializeField] private float hipTrajAmplitude = 0.04f;
        [Tooltip("This is scaled by the hipTrajAmplitude! So the maximum should be set to 1.0")] [SerializeField] private AnimationCurve hipHeightCurve = 
            new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0));
        
        [SerializeField] private GameObjectReplayer leftFoot;
        [SerializeField] private GameObjectReplayer rightFoot;
        [SerializeField] private GameObjectReplayer hip;
        
        [Header("Step Limits [m]")]
        private static float _stepLength = 0.6f;
        [SerializeField] private float maxStepLengthScaling = 1.5f;
        //[SerializeField] private float minStepLengthScaling = 0.3f;
        [SerializeField] Transform avatar;

        // priv
        private bool _initialized;
        private Vector3 _rightFootOffset;
        private Vector3 _leftFootOffset;
        private float _stepLengthFactor;
        private float _hipStepLengthFactor;
        private bool _isPlaying;

        public float UserHeight => avatar.localScale.y * 2;
        public float OptimalStepLength => UserHeight * 0.7774f / 2;
        
        //public float MinStepLength() => OptimalStepLength() * minStepLengthScaling;
        public float MaxStepLength => OptimalStepLength * maxStepLengthScaling;

        public float CurrentOptimalStepLength()
        {
            var frontPos = GetLegPos(FrontLeg());
            var backPos = GetLegPos(BackLeg());
            return (frontPos + OptimalStepLength/2) - backPos;
        }

        public void DoScaledStep(HipThrust hipThrust)
        {
            // Linear relation scalingFactor and SL
            if (hipThrust.LimitedByWalker) return;

            var newStepLength = ExtensionMethods.Remap(hipThrust.ScaledVal, 0, 1, 0, MaxStepLength); // We can scale this from 0 instead of minStepLength, because minStepLength should relate to minVel, and thus min ScalingFactor here that can be input
            DoStep(newStepLength, hipThrust);
        }
        
        private void DoStep(float newStepLength = -1f, HipThrust hipThrust = null)
        {
            var tolerance = double.Epsilon;
            if (newStepLength > 0 && Math.Abs(_stepLength - newStepLength) > tolerance) // check if SL changed
            {
                _stepLength = newStepLength;
                CalculateOffsets();
            }
            
            if (hip.IsReplaying()) return; // So we don't start another step before the previous one was finished

            var steppingLeg = FindSteppingLeg();
            var score = CalculateScore(hipThrust); 
            var currStep = new Step(_stepLength, steppingLeg, score, hipThrust.ScaledVal, CurrentOptimalStepLength());
            
            if (steppingLeg == Leg.Right) 
                rightFoot.StartReplay();
            else 
                leftFoot.StartReplay();
            
            hip.StartReplay();

            _isPlaying = true;
            StartCoroutine(WaitForEndAnimation(totalReplayTime));

            onDoStep.Invoke(currStep);
        }
        
        // Calculate the score for this step <<<<<<<<<<<<<<<<<<<<< we need to find the SL based on the foot position
        private float CalculateScore(HipThrust hipThrust)
        {
            var bezierCalc = _gaitFeedback.EvaluateCurve(hipThrust.ScaledVal); // Value from 0 to 1 depending no the scaledVal and the settings of the bezier curve that also defines the visual cue shape 
            var basePoints = 250;
            var bonusPoints = 750;
            float trunkMultiplier = ExtensionMethods.Remap(hipThrust.trunkScaledInput, 0.0f, 1.0f, 1.0f, 0.1f); // Trunk value from 0 to 1.0
            return (basePoints + bezierCalc * bonusPoints) * trunkMultiplier;
        }

        private IEnumerator WaitForEndAnimation(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            _isPlaying = false;
            onFinishStep.Invoke();
        }

        private void CalculateOffsets()
        {
            // Foot placement offsets
            var lVec = (_stepLength / 4) * globalWalkingDirection.forward;
            var wVec = ( stepWidth  / 2) * globalWalkingDirection.right;
            
            var lMultiplier = IsRight(FindSteppingLeg()) ? -1 : 1;
            _rightFootOffset = wVec +  1 * lMultiplier * lVec;
            _leftFootOffset = -wVec + -1 * lMultiplier * lVec;

            // stepLength factors
            _stepLengthFactor = 1 / GetDataStepLength(leftFoot);
            _hipStepLengthFactor = 1 / GetDataStepLength(hip);

            // Set the scaling to match the desired steplength
            var forward = globalWalkingDirection.forward;
            var up = globalWalkingDirection.up;
            leftFoot.positionScaling    = forward * (_stepLengthFactor * _stepLength) + up;
            rightFoot.positionScaling   = forward * (_stepLengthFactor * _stepLength) + up;
            hip.positionScaling         = forward * (_hipStepLengthFactor * (_stepLength / 2)) + up;
        }

        public void InitializeAll()
        {
            if (_initialized) return; // We only want to initialize once... 
            _stepLength = OptimalStepLength;
            
            leftFoot.LocalInit();
            rightFoot.LocalInit();
            hip.LocalInit();

            // Modify the replay data
            FixLastDataPoints(leftFoot);
            FixLastDataPoints(rightFoot);
            FixLastDataPoints(hip);
            SetHipYData();
            
            // Set Transforms
            CalculateOffsets();
            var hipPosProj = Vector3.ProjectOnPlane(hip.transform.position, globalWalkingDirection.up);
            rightFoot.transform.position = hipPosProj + _rightFootOffset;
            rightFoot.SetInitialTransform();
            leftFoot.transform.position  = hipPosProj + _leftFootOffset;
            leftFoot.SetInitialTransform();
            hip.SetInitialTransform();
            
            _initialized = true;
        }
        
        public void ResetStartPos()
        {
            rightFoot.ResetStartTransform();
            leftFoot.ResetStartTransform();
            hip.ResetStartTransform();
        }


        public bool IsPlaying() => _isPlaying;
        
        ////////////////////////// Leg Stuff

        public Leg FrontLeg()
        {
            float posDiff = rightFoot.transform.position.z - leftFoot.transform.position.z;
            return posDiff <= 0 ? Leg.Left : Leg.Right;
        }

        public Leg FindSteppingLeg() => BackLeg();
        public Leg BackLeg() => OtherLeg(FrontLeg());
        public Leg OtherLeg(Leg leg) => leg == Leg.Left ? Leg.Right : Leg.Left;

        private bool IsRight(Leg leg) => leg == Leg.Right;


        public float GetLegPos(Leg leg)
        {
            switch (leg)
            {
                case Leg.Left:
                    return leftFoot.transform.position.z;
                case Leg.Right:
                    return rightFoot.transform.position.z;
                default:
                    Debug.LogError("Unrecognized leg input...");
                    return 0.0f;
            }
        }

        /////////////////////// Some data processing
        private void FixLastDataPoints(GameObjectReplayer gor) // Make sure the last data points match the first ones in the walking direction (so we get a proper loop without any drift)
        {
            var diff = gor.GetEndPos()-gor.GetBeginPos();
            var smoothOverNPoints = 15;
            var posData = gor.GetPosData();
            for (var i = gor.GetEndIndex() - smoothOverNPoints; i <= gor.GetEndIndex(); i++)
            {
                var progress = 1 + (float)(i - gor.GetEndIndex()) / smoothOverNPoints;
                //Debug.Log(progress);
                var endPos = posData[i] - Vector3.ProjectOnPlane(diff, globalWalkingDirection.forward) * progress;
                gor.SetPosDataPoint(i,endPos);
            }
        }

        private float GetDataStepLength(GameObjectReplayer gor)
        {
            var begin = gor.GetBeginPos();
            var end = gor.GetEndPos();
            var forward = globalWalkingDirection.forward;
            begin = Vector3.Project(begin,forward);
            end = Vector3.Project(end,forward);
            return Vector3.Magnitude(end - begin);
        }
        
        private void SetHipYData()
        {
            // Decrease initial transform by the trajectory amplitude
            var hipTransform = hip.transform;
            var posHip = hipTransform.position;
            posHip.y -= hipTrajAmplitude;
            hipTransform.position = posHip;
            
            // Modify the data accorting to the set curve
            var posData = hip.GetPosData();
            var beginIndex = hip.GetBeginIndex();
            var endIndex = hip.GetEndIndex();
            var loopSize = (float)(endIndex - beginIndex);
            
            for (var i = hip.GetBeginIndex(); i <= hip.GetEndIndex(); i++)
            {
                var currPos = posData[i];
                currPos.y = hipHeightCurve.Evaluate((i-beginIndex) / loopSize) * hipTrajAmplitude;
                hip.SetPosDataPoint(i,currPos);
            }
        }
    }
}

/// <summary>
/// Event containing a Step as a parameter
/// </summary>
[Serializable] public class StepEvent : UnityEvent<Step>
{
    
}

/// <summary>
/// The details about a step taken by the stepreplaymanager
/// </summary>
[Serializable]
public class Step
{
    public float StepLength { get; }
    public float Score { get; }

    public Leg SteppingLeg { get; }
    public float ScaledInput { get; }
    public float CurrentOptimalStepLength { get; }

    public Step(float stepLength, Leg steppingLeg, float score = 0, float scaledInput = 1.0f, float currentOptimalStepLength = 1.0f)
    {
        this.StepLength = stepLength; // in m
        this.SteppingLeg = steppingLeg;
        this.Score = score;
        this.ScaledInput = scaledInput;
        this.CurrentOptimalStepLength = currentOptimalStepLength; // in m
    }

    public Leg FrontLeg()
    {
        return SteppingLeg == Leg.Left ? Leg.Right : Leg.Left;
    }
}

/// <summary>
/// Left or Right leg
/// </summary>
public enum Leg { Left, Right }
