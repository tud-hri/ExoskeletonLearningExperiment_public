using UnityDataReplay.ReplayerTypes;
using UnityEngine;

[RequireComponent(typeof(WalkerMotion))]
public class WalkerInput : MonoBehaviour
{
    [SerializeField] private StepReplayManager stepReplayManager;
    [SerializeField] private float walkerZoffset;

    private WalkerMotion _walkerMotion;

    private void Start()
    {
        _walkerMotion = GetComponent<WalkerMotion>();
    }
    
    public float MaxWalkerDistance() => stepReplayManager.MaxStepLength;
    public float MinWalkerDistance() => 0;//stepReplayManager.MinStepLength();

    // Just pass on some stuff to make our lives easier
    public float GetWalkerDistance() => _walkerMotion.BackFootWalkerDistance + walkerZoffset;
    public bool DoingWalkerMatching() => _walkerMotion.DoingWalkerMatching;

    public float ScaledInput() => Mathf.Clamp(GetWalkerDistance() / MaxWalkerDistance(),0,1);
}
