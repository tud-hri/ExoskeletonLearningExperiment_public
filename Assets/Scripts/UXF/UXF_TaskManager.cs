using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using UnityEngine.UI;

namespace UXF
{
    public class UXF_TaskManager : MonoBehaviour
    {
        [SerializeField] private SceneStateManager _stateManager;
        [SerializeField] private QuestionnaireManager _questionnaireManager;

        public Text remainingTimeText;

        // Time variables
        private float _currTrialRunTime;
        private float _currTrialTime;
        private bool _startTimeCountdown;
        
        // Results variables
        private float _totalScore;
        private float _distanceTraveled;
        private int _stepsTaken;

        private void Update()
        {
            if (!_startTimeCountdown) return;
            
            // Calculate the remaining time
            _currTrialTime += Time.deltaTime;
            var remainingTime = _currTrialRunTime - _currTrialTime;
            // set some UI text element << should probably take care of this somewhere else.
            if (remainingTimeText != null) remainingTimeText.text = (Mathf.Round(remainingTime * 10) / 10) + "s";
            
            // Check if time is over & end trial
            if (!(remainingTime < 0)) return;

            Session.instance.EndCurrentTrial();
        }

        public void BeginNextTrial(Session session = null)
        {
            if (session == null) session = Session.instance;
            
            session.BeginNextTrialSafe();
            //Trial trial = session.CurrentTrial;
            //Debug.LogFormat("Started trial {0} (number in block: {1})", trial.number, trial.numberInBlock);
        }

        public void SetStartingTrial()
        {
            Session.instance.currentTrialNum = Session.instance.settings.GetInt("starting_trial");
            _questionnaireManager.SetNumQuestionnaire(Session.instance.settings.GetInt("starting_q"));
        }

        // ------- Trial specific setup -------

        private void SetupTrial(Trial trial)
        {
            // Set up the trial
            _stateManager.SetVisualCues(trial.settings.GetBool("cues_enabled",false));
            _stateManager.SetPerspective((Perspective)trial.settings.GetObject("perspective"));
            int runtime = trial.settings.GetInt("runtime");
            
            // Set up the countdown timer...
            _currTrialRunTime = runtime;
            
            _startTimeCountdown = true; 
        }

        private void CleanUpTrial(Trial trial)
        {
            Session session = trial.session;

            // Record the main metrics in the trial.results & reset the counters
            if (trial.result != null)
            {
                trial.result["score"] = _totalScore;
                trial.result["distance"] = _distanceTraveled;
                trial.result["steps"] = _stepsTaken;
            }
            else Debug.Log("Did not find results, probably not recording data during this trial");
            
            _totalScore = 0.0f;
            _distanceTraveled = 0.0f;
            _stepsTaken = 0;

            // Reset Time
            _currTrialTime = 0;
            _startTimeCountdown = false;

            // Check if last trial in block
            if (session.CurrentTrial.numberInBlock == session.CurrentBlock.trials.Count)
            {
                // Show the questionnaire
                int nQuestionnaires = trial.settings.GetInt("nQuestionnaires");
                if (nQuestionnaires > 0)
                    _questionnaireManager.ShowQuestionnaires(nQuestionnaires);
            }

            // ... Other cleanup tasks
            string nextTrial = "";
            if (trial != trial.session.LastTrial) nextTrial = session.NextTrial.number.ToString();
            Debug.LogFormat("Trial {0} done, Beginning next trial {1}", session.CurrentTrial.number, nextTrial);

            EndIfLastTrial(trial); 
        }
        
        private void EndIfLastTrial(Trial trial)
        {
            if (trial == trial.session.LastTrial)
            {
                trial.session.End();
            }
        }
        
        public void RestartTrial()
        {
            var s = Session.instance;
            s.EndCurrentTrial();
            s.currentTrialNum -= 1;
        }

        public void PrevTrial()
        {
            var s = Session.instance;
            s.EndCurrentTrial();
            s.currentTrialNum -= 2;
        }

        // ---------- Task-specific results ---------

        public void AddStep(Step step)
        {
            _totalScore += step.Score;
            _distanceTraveled += step.StepLength;
            _stepsTaken++;
            Debug.LogFormat("{0} steps taken, adding up to total distance traveled: {1} meters",_stepsTaken,_distanceTraveled);
        }

        public int NumberSteps() => _stepsTaken;

        public float Distance() => _distanceTraveled;

    }
}
