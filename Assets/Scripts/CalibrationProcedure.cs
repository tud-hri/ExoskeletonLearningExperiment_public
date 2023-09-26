using UnityEngine;
using UnityEngine.Events;
using UXF;

public class CalibrationProcedure : MonoBehaviour
{
    [SerializeField] public UnityEvent calibrateMethodsFirstTrial;

    [SerializeField] public UnityEvent calibrateMethods;

    [SerializeField] public bool alwaysCalibrateMethodsFirstTrial = true;

    public void CalibrateAll()
    {
        if (Session.instance.CurrentTrial.number <= 1 || alwaysCalibrateMethodsFirstTrial)
        {
            calibrateMethodsFirstTrial.Invoke();
        }
        
        calibrateMethods.Invoke();
    }
    
}
