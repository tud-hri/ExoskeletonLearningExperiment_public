using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SensorCalibration : MonoBehaviour
{
    // Put the tracker stably on a surface, run MatchGravity. This flips the transform so that gravity points down
    // Then either put the tracker stably on the surface again, but rotate about 90 deg & run the method again.
    
    // imuTransform should be child of the tracked device transform it is attached to in reality
    
    [SerializeField] Transform imuTransform;
    [SerializeField] HipThrustInput hipThrustInput;
    [SerializeField] private Transform simulatedImuTransform;

    //private Vector3 prevDownDirection;

    private IEnumerator DoMatchingEnumerator(int windowSize = 30)
    {
        // First we find the average of windowSize measurements to find the **Gravity direction in local space**
        List<Vector3> accValues = new List<Vector3>();
        for (var i = 0; i <= windowSize; i++)
        {
            //accValues.Add(SimulatedAcc(0.01f));
            accValues.Add(hipThrustInput.SendAccRaw());
            yield return 0; // wait a frame to take next measurement
        }

        Vector3 currAvgAcceleration = new Vector3(
            accValues.Average(x=>x.x),
            accValues.Average(x=>x.y),
            accValues.Average(x=>x.z));
        
        // We transform this to worldspace and find the angle with the down direction of our unity scene
        var worldSpaceGVec = imuTransform.localToWorldMatrix.MultiplyVector(currAvgAcceleration);
        var angle = Vector3.Angle(worldSpaceGVec, Vector3.down);
        // We use this angle to rotate the transform to match up these vectors (in world space)        
        var rotAxis = Vector3.Cross(worldSpaceGVec, Vector3.down);
        imuTransform.RotateAround(imuTransform.position,rotAxis,angle);
        
        // We save the down direction in local space, such that we can make sure that our next measurement is at roughly 90 deg difference so that we quickly end up at the correct solution
        //prevDownDirection = imuTransform.worldToLocalMatrix.MultiplyVector(Vector3.down);
        Debug.Log($"Rotated by angle: {angle}"); // Once this angle is sufficiently small, we can stop as we know our correct orientation.
    }
    
}
