using UnityEngine;
using RootMotion.FinalIK;

public class TrunkInclinationInput : MonoBehaviour
{
    // Detect trunk inclination based on head and hip positions of the avatar (VRIK) Target
    [SerializeField] VRIK vrik;
    
    [Tooltip("The minimum inclination that can be found by the GetAngle method.")]
    [SerializeField] public float minInclination = 15.0f;
    [Tooltip("The maximum inclination that can be found by the GetAngle method.")]
    [SerializeField] public float maxInclination = 90.0f;
    
    public float GetAngleRaw()
    {
        Transform spineTarget = vrik.solver.spine.pelvisTarget;
        Transform headTarget = vrik.solver.spine.headTarget;
        if (spineTarget == null || headTarget == null)
        {
            //Debug.LogError("Could not find VRIK avatar targets, returning 0 deg");
            return 0.0f;
        }

        Vector3 trunkVec = headTarget.position - spineTarget.position;

        trunkVec.x = 0; // ignore x-axis
        float factor = 1.0f;
        if (trunkVec.z < 0) factor = -1.0f; // negative angle if z-axis is negative

        return factor * Vector3.Angle(Vector3.up, trunkVec);
    }

    public float Angle => Mathf.Clamp(GetAngleRaw(), minInclination, maxInclination);

    public float ScaledInput() => ExtensionMethods.Remap(Angle, minInclination, maxInclination, 0, 1);
}
