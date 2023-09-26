using System;
using System.Collections;
using System.Collections.Generic;
using UnityDataReplay.ReplayerTypes;
using UnityDataReplay.Utility;
using UnityEngine;

namespace UnityDataReplay
{
    public class ReplayManager : MonoBehaviour
    {
        [SerializeField] private bool disableAllReplayers;
        [Header("General replay settings")] // Input in the Unity inspector
        [Tooltip("When enabled, will start all replayers at the startup. Otherwise StartReplay should be called from an external script (or using the editor controls).")]
        [SerializeField] private bool replayAtStartup;
        [Tooltip("If replaying at the startup, after how much time should it start? This prevents race conditions so that all loggers are properly initialized.")]
        [SerializeField] private float startupWaitTime;

        [Header("List of replayers")] // Array of GameObjects that can be replayed
        [SerializeField] private List<Replayer> replayers = new List<Replayer>();

        [Header("Replay Settings")] 
        [SerializeField] private float timeScalingFactor = 1.0f;
        [SerializeField] private float beginTime;
        [Tooltip("If < 0, all data is replayed")]
        [SerializeField] private float endTime = -1.0f;
    
        [Header("Editor Controls")]
        [SerializeField] private bool enableEditorControls = false;
        [Tooltip("Start replay by clicking this bool in the editor. Useful mostly for testing.")]
        [SerializeField] private bool startReplays;
        [SerializeField] private bool stopReplays;
        [SerializeField] private float setTime;
        [SerializeField] private bool reSetAllSettings;
        
        private float _prevSetTime;
        private const float Tolerance = 0.005f;
        private bool _initialized;

        private void OnValidate()
        {
            if (!enableEditorControls) return;
            if (!_initialized) return;
            
            if (reSetAllSettings)
            {
                SetAllReplaySettings();
                reSetAllSettings = false;
            }
            else if (startReplays)
            {
                StartAllReplays();
                startReplays = false;
            }
            else if (stopReplays)
            {
                StopAllReplays();
                stopReplays = false;
            }
            else if (Math.Abs(setTime - _prevSetTime) > Tolerance)
            {
                _prevSetTime = setTime;
                UpdateAllWithTime(setTime);
            }
        }


        private void UpdateAllWithTime(float f)
        {
            foreach (var r in replayers) r.UpdateWithTime(f, true);
        }

        void Start()
        {
            StartCoroutine(LateStart(startupWaitTime));
                
            _initialized = true;
        }

        private void SetAllReplaySettings()
        {
            foreach (var r in replayers)
            {
                r.SetTimesAndReTrim(beginTime,endTime);
                r.timeScalingFactor = timeScalingFactor;
            }
        }

        IEnumerator LateStart(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            if (replayAtStartup) 
                StartAllReplays();
            if (disableAllReplayers) 
                DisableAllReplayers();
            
        }

        private void StartAllReplays()
        {
            SetAllReplaySettings();
            foreach (var r in replayers) r.StartReplay();
        }
    
        private void StopAllReplays()
        {
            foreach (var r in replayers) r.StopReplay();
        }
    
        public void AddReplayer(Replayer replayer)
        {
            replayers.Add(replayer);
        }
    
        public void RemoveReplayer(Replayer replayer)
        {
            replayers.Remove(replayer);
        }

        private void DisableAllReplayers()
        {
            foreach (var r in replayers) r.enabled = false;
        }

        /*private void ShiftAllPauseTo(Replayer replayer) // This causes issues when any of the replayers is removed........................
        {
            foreach (var r in replayers)
            {
                r.OnPause -= r.PauseReplay;
                replayer.OnPause += r.PauseReplay;
                
                var gor = r.GetComponent<GameObjectReplayer>();
                if (!gor) continue;
                
                r.OnPause -= gor.SetPause;
                replayer.OnPause += gor.SetPause;
            }
        }
        
        private void ShiftAllPauseBackFrom(Replayer replayer)
        {
            foreach (var r in replayers)
            {
                replayer.OnPause -= r.PauseReplay;
                r.OnPause += r.PauseReplay;
                
                var gor = r.GetComponent<GameObjectReplayer>();
                if (!gor) continue;
                
                r.OnPause += gor.SetPause;
                replayer.OnPause -= gor.SetPause;
            }
        }*/
    }
}