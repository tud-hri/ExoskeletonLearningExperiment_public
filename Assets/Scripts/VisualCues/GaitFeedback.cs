using System.Collections;
using UnityDataReplay.ReplayerTypes;
using UnityEngine;

public class GaitFeedback : MonoBehaviour
{
    [Header("Outputs")]
    [Tooltip("The parent transform of the visual cue. This is the transform used to move and scale the cue.")]
    [SerializeField] private Transform parentTransform;
    [SerializeField] private Vector3 additionalOffset;
    [SerializeField] private TextureCurve curve = null;
    [SerializeField] private GradientSetter gradient = null;
    [SerializeField] private Material material = null;

    [Header("Inputs")]
    [SerializeField] private WeightShiftingInput weightShiftingInput;
    [SerializeField] private HipThrustInput hipThrustInput;
    [SerializeField] private TrunkInclinationInput trunkInclinationInput;
    [SerializeField] private WalkerInput walkerInput;
    [SerializeField] private StepReplayManager stepReplayManager;

    [Header("Scaling Limits")]
    [SerializeField] private float minZScale = 0.3f;
    [SerializeField] private float maxZScale = 2.0f;
    private float _thresholdPos;
    [SerializeField] private bool useWalkerCutoff = true;


    private void Start()
    {
        SetCueThreshold();
        var opt = stepReplayManager.OptimalStepLength / stepReplayManager.MaxStepLength;
        SetCueOptimum(opt);
        BakeTexture();
        ThrustTriggered(0.0f);
    }

    private void SetCueThreshold()
    {
        _thresholdPos = hipThrustInput.minAcc / hipThrustInput.maxAcc;
        _thresholdPos = Mathf.Clamp(_thresholdPos,0.01f,0.98f);

        curve.SetRedKeyKeyTime(1,_thresholdPos);
        gradient.SetGradientKeyTime(1, _thresholdPos);
        gradient.SetGradientKeyTime(0,_thresholdPos-0.01f);

        Debug.Log($"ThresholdPos = {_thresholdPos}, which relates to minVel = {hipThrustInput.minVel} and minStepLength = {_thresholdPos * stepReplayManager.MaxStepLength}");
        Debug.Log($"Pos = 1.0 relates to maxVel = {hipThrustInput.maxVel}, and maxStepLength = {stepReplayManager.MaxStepLength}");
    }

    private void SetCueOptimum(float scaledOptimumPos)
    {
        scaledOptimumPos = Mathf.Clamp(scaledOptimumPos,_thresholdPos,0.99f);
        
        var optiPos = Mathf.Min(GetAlphaPos(scaledOptimumPos));
        material.SetFloat("_OptimumIndicatorPos",optiPos);
        
        curve.SetRedKeyKeyTime(2,scaledOptimumPos); 
        gradient.SetGradientKeyTime(2, scaledOptimumPos);
    }

    public float EvaluateCurve(float t) => curve.EvaluateRedCurve(t);
    public Color EvaluateGradient(float t) => gradient.EvaluateGradient(t);

    private void BakeTexture()
    {
        curve.Bake();
        gradient.Bake();
        material.SetTexture("_CurveTex", curve.Texture);
        material.SetTexture("_GradientTex", gradient.Texture);
    }

    private void Update()
    {
        // Set correct z pos for cue
        var currPos = parentTransform.position;
        currPos.z = stepReplayManager.GetLegPos(stepReplayManager.FrontLeg());
        parentTransform.position = currPos + additionalOffset;

        SetAlphaPos(hipThrustInput.ScaledInput()); // Show the current acceleration by the hip thrust input

        if (hipThrustInput.IsThrustStarted()) 
        {
            return; // If the thrust started, we don't want to move the rest anymore
        }
        
        ScaleZ(trunkInclinationInput.ScaledInput());
        SetXPos(weightShiftingInput.GetHipXPos());

        if (useWalkerCutoff)
            SetWalkerCutoff(walkerInput.ScaledInput());
        else
            SetWalkerCutoff(1.0f);
    }

    private void ScaleZ(float scaledInput)
    {
        var scale = ExtensionMethods.Remap(scaledInput, 0, 1, maxZScale, minZScale);
        var localScale = parentTransform.localScale;
        localScale.z = scale;
        parentTransform.localScale = localScale;
    }
    
    private void SetAlphaPos(float scaledInput) => material.SetFloat("_AlphaPos", GetAlphaPos(scaledInput));

    private float GetAlphaPos(float scaledInput)
    {
        float minPos = -1.0f;
        float maxPos = 1.0f;
        return ExtensionMethods.Remap(scaledInput, 0, 1, minPos, maxPos);
    }

    private void SetXPos(float xPos)
    {
        var pos = parentTransform.position;
        pos.x = xPos;
        parentTransform.position = pos;
    }

    private void SetWalkerCutoff(float scaledInput)
    {
        var pos = GetAlphaPos(scaledInput);
        material.SetFloat("_CutOffPos", pos);
    }

    public void ThrustTriggered(HipThrust hipThrust)
    {
        var input = Mathf.Min(hipThrust.ScaledVal, 0.98f);
        ThrustTriggered(input);
    }
    
    public void ThrustTriggered(float scaledVal)
    {
        material.SetFloat("_IndicatorPos", GetAlphaPos(scaledVal));
    }

    public void ResetLastThrustPos() => ThrustTriggered(0.0f); // Just set it back to the start

    public void DoBlink() => StartCoroutine(BlinkMaterialFloat(2,0.4f,"_IndicatorPos",-10));

    private IEnumerator BlinkMaterialFloat(int blinkTimes, float blinkTime, string floatName, float offVal = 0.0f)
    {
        var currVal = material.GetFloat(floatName);

        for (var i = 0; i < blinkTimes; i++)
        {
            material.SetFloat(floatName,offVal);
            yield return new WaitForSeconds(blinkTime/2);
            material.SetFloat(floatName,currVal);
            yield return new WaitForSeconds(blinkTime/2);
        }
    }

    public void StepEnded() // something something, optimum scaled between limits or scaled using full range?
    {
        var scaledOptimumPos = stepReplayManager.CurrentOptimalStepLength()/stepReplayManager.MaxStepLength;
        //Debug.Log($"OptimumPos = {scaledOptimumPos}, relates to optVel = {scaledOptimumPos*hipThrustInput.maxVel}, and CurrOptimalStepLength = {stepReplayManager.CurrentOptimalStepLength()}");
        SetCueOptimum(scaledOptimumPos);
        BakeTexture();
    }
}
