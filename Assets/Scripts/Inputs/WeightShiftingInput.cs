using RootMotion.FinalIK;
using UnityEngine;

public class WeightShiftingInput : MonoBehaviour
{

    // Calculate the Point in the middle of the feet
    // and the position of the Hip with respect to to the the Middle Point

    [SerializeField] private VRIK vrik;
    [SerializeField] private GameObject rightFoot, leftFoot;

    private Vector3 _middlePoint;
    private float _xHipPos;
    private float _hipPos;

    public Vector3 GetMiddlePoint() => _middlePoint;


    // Update is called once per frame
    void Update()
    {
        var left = leftFoot.transform.position;
        var right = rightFoot.transform.position;
        
        _middlePoint = right - (right - left) / 2; // Point in the middle of the feet
        //Vector3 distanceBetweenFeet = left - right;

        if (vrik.solver.spine.pelvisTarget == null) return;
        
        _hipPos = vrik.solver.spine.pelvisTarget.position.x;
        _xHipPos = (_hipPos - _middlePoint.x);  // Position of the Hip wrt the MiddlePoint
    }

    public float GetHipPos()
    {
        return _xHipPos;
    }
    public float GetHipXPos() => _hipPos;
}
