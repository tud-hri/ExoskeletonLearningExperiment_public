using UnityEngine;

// Attach to walker, call using VRCalibrationWand (attached to controller)
public class CalibrateWalker : MonoBehaviour
{
    [SerializeField] Transform _calibrationPoint;
    [SerializeField] private WalkerMotion walkerMotion;

    // Scale Walker based on CalibrationWand
    public void ScaleWalker(VRCalibrationWand wand) // walker model height is 0.9m (at scale 1.0)
    {
        Vector3 scaleBy = wand.GetScaleValue(_calibrationPoint.position, false, true, false);

        var t = transform;
        Vector3 newGlobalScale = Vector3.Scale(t.localScale, Quaternion.Inverse(t.localRotation) * scaleBy);
        
        transform.localScale = newGlobalScale;
    }

    public void ShiftWalkerOffset(VRCalibrationWand wand)
    {
        var offset = wand.transform.position.z - _calibrationPoint.position.z;
        walkerMotion.WalkerOffsetZ += offset;
    }
}
