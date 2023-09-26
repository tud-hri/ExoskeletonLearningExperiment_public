using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnityDataReplay.Utility
{
    public class Replayer : MonoBehaviour
    {
        [Header("Main Settings")] // Input in the Unity inspector
        [Tooltip("Select the csv-file containing the positions and rotations.")]
        [SerializeField] public TextAsset csvFile;
        [SerializeField] private bool addToReplayManager = true;
    
        [Header("Replay Settings")]
        [SerializeField] private float extraTimeOffSet; 
        [SerializeField] public float timeScalingFactor = 1.0f;
        [Tooltip("If doLoop=false, This makes it so t=0 refers to t=beginTime")][SerializeField] 
        private bool shiftReplayByBeginTime = true;
        [SerializeField] public bool doLoop;
        [Tooltip("End of the loop is always a breakpoint, more breakpoints can be set")][SerializeField] 
        public bool pauseAtBreakpoints;
        [Tooltip("Set data indices at which the data should pause if pauseAtBreakpoint is enabled.")][SerializeField] 
        private int[] breakpoints;
        
        [SerializeField] private float beginTime;
        [Tooltip("If < 0, all data is replayed")]
        [SerializeField] private float endTime = -1.0f;

        [Header("Editor Controls")] 
        [SerializeField] private bool enableEditorControls = false;
        [Tooltip("Start replay by clicking this bool in the editor. Useful mostly for testing.")]
        [SerializeField] private bool startReplay;
        [SerializeField] private bool stopReplay;
        [Tooltip("Set the index of the datapoint, this ignore any replay settings")][SerializeField] private int setIndex;
        [SerializeField] private float setTime;
        [SerializeField] private bool reTrimData;
    
        // Data Processing
        private MethodInfo _setDataMethod; // The method in the inheriting class that defined what data to set, note: This may NOT be null
        public List<MethodInfo> toRemove; // The method in the inheriting class that defined what data to set, note: This may NOT be null

        // Events
        public delegate void TrimDataHandler();
        public event TrimDataHandler OnTrimData;             // Event runs when the data gets trimmed (new begin and endtime get defined)

        public delegate void LoopForwardHandler();
        public event LoopForwardHandler OnLoopForward;       // Event runs when the end of the loop has been reached
        
        public delegate void LoopBackwardHandler();
        public event LoopBackwardHandler OnLoopBackward;     // Event runs when the beginning of a loop has been reached (when playing backwards)

        public delegate void PauseHandler();
        public event PauseHandler OnPause;                  // Event runs when the when pausing
        
        // Private
        private int _index;
        private float _replayTime;

        private float[] _times;
        private float _startTime;
        private float _startTimeOffset;

        private bool _initialized;
        private bool _isReplaying;
        public bool IsReplaying() => _isReplaying;

        private int _beginIndex;
        private int _endIndex;
        private int _pausedIndex;
        private float _loopTime;
        private int _nLoops = 0; // number of loops we are currently at
        
        float TOLERANCE = 0.005f;

        private void OnValidate()
        {
            if (!enableEditorControls) return;
            if (!_initialized) return;
            
            if (reTrimData)
            {
                SetTimesAndReTrim(beginTime,endTime);
                reTrimData = false;
            }
            else if (startReplay)
            {
                StartReplay();
                startReplay = false;
            }
            else if (stopReplay)
            {
                StopReplay();
                stopReplay = false;
            }
            else if (setIndex != _index)
            {
                SetIndex(setIndex);
                setTime = _times[setIndex];
            }
            else
            {
                if (Math.Abs(FindTimeInData(setTime) - FindTimeInData(_times[_index])) > TOLERANCE)
                {
                    UpdateWithTime(setTime);
                    setIndex = _index;
                }
            }
        }

        private void Update()
        {
            if (_isReplaying) UpdateWithTime(Time.time - _startTime + extraTimeOffSet, true);
        }

        public void StartReplay()
        {
            SetStartTime();
            _isReplaying = true;
        }
        public void StopReplay() => _isReplaying = false;

        protected Dictionary<string, string[]> Initialize()
        {
            _setDataMethod = GetType().GetMethod("SetData");
            if (_setDataMethod == null) Debug.LogError("You need to define a method called SetData(int index)");
            OnLoopBackward += Pause;
            OnLoopForward  += Pause;
            OnPause += PauseReplay;
            
            var data = GetReplayData(AssetDatabase.GetAssetPath(csvFile));

            // Set Initial datapoint
            SetTimesAndReTrim(beginTime,endTime);
            UpdateWithTime(beginTime + extraTimeOffSet);
            _pausedIndex = _beginIndex;

            // Add to the logging manager
            if (addToReplayManager)
            {
                ReplayManager replayManager = FindObjectOfType<ReplayManager>();
                if (replayManager)
                    replayManager.AddReplayer(this); // place this logger instance in a list in the logging manager so that we can easily work with multiple different loggers
                else
                    Debug.LogError("Can't find the replay manager!");
            }

            _initialized = true;
            Debug.Log($"Replayer for {name} intialized");
        
            return data;
        }

        private void OnDestroy()
        {
            OnLoopBackward -= Pause;
            OnLoopForward  -= Pause;
            OnPause -= PauseReplay;
        }

        #region TimeManagement

        public void UpdateWithTime(float replayTime, bool updateEditorControls = false)
        {
            if (!_initialized) return;
            _replayTime = replayTime; // We update the global replay time variable - we need this for the pausereplay event method (now also the checkloopchange method)
            CheckLoopChange();
            
            var timeInData = FindTimeInData(replayTime);
            var index = FindTimeIndex(timeInData, _beginIndex);
            
            // Have we reached a breakpoint yet?
            if (pauseAtBreakpoints)
            {
                foreach (var breakpoint in breakpoints)
                    if (_index < breakpoint && index >= breakpoint)
                        Pause();
            }

            SetIndex(index);
            
            // Update editor controls. 
            if (updateEditorControls)
            {
                //setTime = timeInData; // Time behaves differently here...
                setIndex = index;
            }
        }

        private void CheckLoopChange() // This checks if the amount of loops weve gone through has changed, and invokes any loop method if necessary.
        {
            var newNLoops = (int)Mathf.Floor(_replayTime / _loopTime);
            var dLoops = newNLoops - _nLoops;

            if (dLoops == 0) return;

            _nLoops = newNLoops;
            if (dLoops > 0)
                for (int i = 0; i < dLoops; i++) 
                    OnLoopForward?.Invoke();
            else 
                for (int i = 0; i < -1*dLoops; i++) 
                    OnLoopBackward?.Invoke();
        }
        
        private void Pause()
        {
            if (!pauseAtBreakpoints) return;
            
            OnPause?.Invoke();
        }

        public void PauseReplay()
        {
            _pausedIndex = _index;
            _startTimeOffset = _replayTime - extraTimeOffSet;
            _isReplaying = false;
        }
        
        

        // We always find the time withing the defined time-limits.
        private float FindTimeInData(float replayTime)
        {
            var timeInData = replayTime * timeScalingFactor;
            if (doLoop)
            {
                timeInData -= _nLoops * _loopTime;
                if (timeInData < 0) timeInData += _loopTime; // to wrap around to the other side of the loop using negative time
                timeInData += beginTime;
            }
            else if (shiftReplayByBeginTime) // This makes it so t=0 refers to t=beginTime, if disabled, there will be some empty time until t=beginTime
            {
                timeInData += beginTime;
                // We don't want to loop around when doLoop is false
                if (timeInData <= beginTime)    return beginTime; 
                if (timeInData >= endTime)      return endTime;
            }

            return timeInData;
        }

        private void SetStartTime() => _startTime = Time.time - _startTimeOffset;

        public void SetIndex(int i)
        {
            // To prevent errors
            if (i < 0)
            {
                Debug.Log("Can't set < 0, setting index to 0 instead.");
                i = 0;
            }

            var maxIndex = _times.Length - 1;
            if (i > maxIndex)
            {
                Debug.Log($"Can't set > length dataset ({maxIndex}), setting index to {maxIndex} instead.");
                i = maxIndex;
            }
            _index = i;
            _setDataMethod.Invoke(this, new object[] { i });
        }


        // The next datapoint is only taken once the timestamp has passed
        // (unless time is outside of limits, then we just take the limit index)
        private int FindTimeIndex(float time, int beginAt = 0)
        {
            // To prevent errors
            if (time <= _times[0]) return 0; 
            if (time >= _times[_times.Length-1]) return _times.Length-1;
        
            var i = beginAt;
            while (_times[i] <= time) i++;
        
            return i-1;
        }

        public void SetTimesAndReTrim(float setBeginTime, float setEndTime)
        {
            beginTime = setBeginTime;
            endTime = setEndTime;
            TrimData();
        }
    
        private void TrimData()
        {
            _beginIndex = FindTimeIndex(beginTime);

            var realEndTime = endTime;
            if (endTime < beginTime)
            {
                _endIndex = _times.Length-1;
                realEndTime = _times[_endIndex];
            }
            else _endIndex = FindTimeIndex(endTime, _beginIndex);

            _loopTime = realEndTime - beginTime;

            OnTrimData?.Invoke();
        }

        public int GetBeginIndex() => _beginIndex;
        public int GetEndIndex() => _endIndex;
        public int GetCurrentIndex() => _index;
        public int GetPausedIndex() => _pausedIndex;
        public bool IsInitialized() => _initialized;

        #endregion

    
        #region CsvReading

        private Dictionary<string,string[]> GetReplayData(string pathToCsvFile)
        {
            string localTimeHeader = (string)GetType().GetField("timeHeader").GetValue(this);
            var dict = CsvToDict(pathToCsvFile);
            _times = StringsToFloats(dict[localTimeHeader]);

            return dict;
        }

        private Dictionary<string, string[]> CsvToDict(string pathToCsvFile)
        {
            var data = File.ReadAllLines(pathToCsvFile);
            var keys = data[0].Split(',');
            string[][] vals = new string[keys.Length][];

            var nColumns = keys.Length;
            var nLines = data.Length-1;
        
            for (var i = 0; i < nColumns; i++)
                vals[i] = new string[nLines];

            for (var i = 1; i < nLines+1; i++)
            {
                var d = data[i].Split(',');
                for (var j = 0; j < nColumns; j++)
                    vals[j][i-1] = d[j];
            }
        
            var dict = new Dictionary<string, string[]>();
            for (var i = 0; i < nColumns; i++)
                dict[keys[i]] = vals[i];
        
            return dict;
        }

        // Public utility functions for converting data
    
        public float[] StringsToFloats(string[] strings)
        {
            var n = strings.Length;
            float[] floats = new float[n];
            for (int i = 0; i < n; i++)
                floats[i] = float.Parse(strings[i]);
        
            return floats;
        }

        public Vector3[] MakeVector3SFromDict(Dictionary<string, string[]> dict, string[] keys)
        {
            if (keys.Length != 3) Debug.LogError("Make sure keys is of length 3");
        
            var x = StringsToFloats(dict[keys[0]]);
            var y = StringsToFloats(dict[keys[1]]);
            var z = StringsToFloats(dict[keys[2]]);

            int n = x.Length; 
            Vector3[] vector3S = new Vector3[n];

            for (int i = 0; i < n; i++)
                vector3S[i] = new Vector3(x[i], y[i], z[i]);
            return vector3S;
        }
    
        public Quaternion[] MakeQuaternionsFromDict(Dictionary<string, string[]> dict, string[] keys)
        {
            if (keys.Length != 4) Debug.LogError("Make sure keys is of length 4");
        
            var x = StringsToFloats(dict[keys[0]]);
            var y = StringsToFloats(dict[keys[1]]);
            var z = StringsToFloats(dict[keys[2]]);
            var w = StringsToFloats(dict[keys[3]]);

            int n = x.Length; 
            Quaternion[] quaternions = new Quaternion[n];

            for (int i = 0; i < n; i++)
                quaternions[i] = new Quaternion(x[i], y[i], z[i],w[i]);
            return quaternions;
        }
        
        #endregion
         
    }
}

public interface IReplayer
{
    void SetData(int i);
}
