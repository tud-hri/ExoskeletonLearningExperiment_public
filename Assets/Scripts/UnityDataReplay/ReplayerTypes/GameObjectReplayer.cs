using System;
using UnityDataReplay.Utility;
using UnityEditor;
using UnityEngine;

namespace UnityDataReplay.ReplayerTypes
{
    public class GameObjectReplayer : Replayer, IReplayer
    {
        // This script is used to replay the transform data of any GameObject.
        // The replay always happens relative to the starting position and rotation at Start()/Or when a new loop starts (if setStartTransformEveryLoop=true).
        // (unless useAbsoluteData=true)
        //

        [Header("Specific Settings")]
        //[SerializeField] private float posScalingFactor = 1.0f; // Deprecated, use transformReferenceFrame to scale data
        [SerializeField] private bool setStartTransformEveryLoop;

        [Tooltip("If specified, all translations and rotations are done in the frame of this transform, using local scale and localrotation rotation. Otherwise, the global reference frame is used.")]
        [SerializeField] private Transform transformReferenceFrame;
        [Tooltip("If enabled, data is not scaled relative to the starting position, but the absolute points in the data will be used. (All offsets and startpoints will be ignored)")][SerializeField] 
        private bool useAbsoluteData;
        
        [Tooltip("Additional offset added at the end of all calculations.")][SerializeField] 
        private Vector3 positionOffset;
        [Tooltip("Additional offset added at the end of all calculations.")][SerializeField] 
        private Vector3 rotationOffset;
        [SerializeField] public Vector3 positionScaling = Vector3.one;
        //[SerializeField] Vector3 rotationScaling = Vector3.one; // might add this later

        [Header("Header Strings")] 
        [SerializeField] private string[] posHeaders = new string[3]
        {
            "Position X",
            "Position Y",
            "Position Z"
        };
        [SerializeField] private string[] rotHeaders = new string[4]
        {
            "Rotation X",
            "Rotation Y",
            "Rotation Z",
            "Rotation W"
        };
        public string timeHeader = "Time Stamp [s]";

        private Vector3[] _positions;
        private Quaternion[] _rotations;
        
        // The transform at the first keyframe
        private Vector3 _beginPos = Vector3.zero;
        private Quaternion _beginRot = Quaternion.identity;
        // The transform at Start() or at beginning of new Loop
        private Vector3 _startPos = Vector3.zero;
        private Quaternion _startRot = Quaternion.identity;
        // The transform at the very beginning of the replay
        private Vector3 _initialPos = Vector3.zero;
        private Quaternion _initialRot = Quaternion.identity;

        private Vector3 _pauseOffsetPos;
        //private Vector3 _pauseOffsetRot;

        public void LocalInit()
        {
            // Get the data
            var data = Initialize();
            _positions = MakeVector3SFromDict(data, posHeaders);
            _rotations = MakeQuaternionsFromDict(data, rotHeaders);
            SetInitialTransform();


            // Add the methods to the events
            OnTrimData      += SetBeginPositions;
            OnLoopForward   += SetBegin;
            OnLoopBackward  += SetEnd;
            //OnPause         += SetPause;
        }

        public void SetInitialTransform()
        {
            // Set all starting values
            var t = transform;
            _startPos = _initialPos = t.position;
            _startRot = _initialRot = t.rotation;
            if (transformReferenceFrame)
                _startRot *= Quaternion.Inverse(transformReferenceFrame.rotation);
            SetBeginPositions();
        }

        public void ResetStartTransform()
        {
            var t = transform;
            // Set the begin positions
            t.position = _startPos = _initialPos;
            t.rotation = _startRot = _initialRot;
        }

        private void OnDestroy()
        {
            OnTrimData      -= SetBeginPositions;
            OnLoopForward   -= SetBegin;
            OnLoopBackward  -= SetEnd;
            //OnPause         -= SetPause;
        }
        
        public void SetData(int i)
        {
            var t = transform;
            t.rotation = GetNewRot(i);
            t.position = GetNewPos(i);
        }

        public Vector3[] GetPosData()    => _positions;
        public Quaternion[] GetRotData()    => _rotations;
        public Vector3 GetBeginPos()    => _beginPos;
        public Quaternion GetBeginRot() => _beginRot;
        public Vector3 GetEndPos()      => _positions[GetEndIndex()];
        public Quaternion GetEndRot()   => _rotations[GetEndIndex()];

        // Event methods
        
        private void SetBeginPositions() // The begin transform of the loop (in the data) is saved and subtracted so we always start from the begin position 
        {
            if (_positions == null) return;
            
            _beginPos = _positions[GetBeginIndex()];
            _beginRot = _rotations[GetBeginIndex()];
        }

        public void SetPause() => SetStartTransform();
        public void SetEnd() => SetStartTransform(true);
        public void SetBegin() => SetStartTransform(false);
        
        //public void SetEndBackward() => SetStartTransform(true);

        public void ReachedEnd()
        {
            _startPos += GetNewPos(GetCurrentIndex());
            //_startRot *= GetNewRot(GetCurrentIndex());
        }
        public void SetStartTransform(bool setEnd = false)
        {
            if (!setStartTransformEveryLoop) return;

            if (setEnd)
            {
                _startPos -= GetNewPos(GetEndIndex()) - _startPos - positionOffset;
                _startRot *= Quaternion.Inverse( GetNewRot(GetEndIndex())*Quaternion.Inverse(Quaternion.Euler(rotationOffset)) * Quaternion.Inverse(_startRot) ); // This might be wrong...
            }
            else
            {
                _startPos = GetNewPos(GetEndIndex()) - positionOffset;
                _startRot = GetNewRot(GetEndIndex()) * Quaternion.Inverse(Quaternion.Euler(rotationOffset));
            }
            
            if (transformReferenceFrame)
                _startRot *= Quaternion.Inverse(transformReferenceFrame.rotation);
            
        }

        // Calculate the new transform
        private Quaternion GetNewRot(int i)
        {
            if (useAbsoluteData) return _rotations[i];

            var addedRot = _rotations[i] * Quaternion.Inverse(_beginRot);
            if (transformReferenceFrame)
                addedRot *= transformReferenceFrame.rotation;

            var newRot = _startRot * addedRot;
            newRot *= Quaternion.Euler(rotationOffset);
            return newRot;
        }

        private Vector3 GetNewPos(int i)
        {
            if (useAbsoluteData) return _positions[i];

            var dataPos = _positions[i];

            var addedPos = dataPos - _beginPos; // The part we actually want to scale (only from the last point that we paused at). - posLastPause is accounted for in the latest _startPos
            addedPos = Vector3.Scale(addedPos, positionScaling); // Position scaling
            
            if (transformReferenceFrame)
                addedPos = transformReferenceFrame.localToWorldMatrix*addedPos;

            var newPos = _startPos + addedPos;

            newPos += positionOffset;
            
            return newPos;
        }

        public void SetPosDataPoint(int i, Vector3 pos) => _positions[i] = pos;
        public void SetRotDataPoint(int i, Quaternion rot) => _rotations[i] = rot;
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_startPos, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_beginPos, 0.1f);
        }
    }
}