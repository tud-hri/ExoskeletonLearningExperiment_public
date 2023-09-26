using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UXF;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject firstScreen;
    [SerializeField] private GameObject secondScreen;
    [SerializeField] private GameObject thirdScreen;

    [SerializeField] private List<GameObject> scoreTexts;

    [Header("Score")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text stepsText;
    [SerializeField] private Text distanceText;
    [Header("Next stage")]
    [SerializeField] private Text stageText;
    [SerializeField] private Text trialText;
    [SerializeField] private Text timeText;
    [SerializeField] private GameObject trialParent;

    [Header("Stage Info")] 
    [SerializeField] private Text stageInfoText;

    public void SetTrialInfo(Trial trial)
    {
        // Set score texts
        if (trial.result != null)
        {
            firstScreen.SetActive(true);
            
            // only show points if cues (and therefore the point system) is actually enabled
            if (trial.settings.GetBool("cues_enabled"))
            {
                scoreText.text = trial.result["score"].ToString();
                foreach (GameObject scoreText in scoreTexts) 
                    scoreText.SetActive(true);
            }
            else
            {
                scoreText.text = "";
                foreach (GameObject scoreText in scoreTexts)
                    scoreText.SetActive(false);
            }

            stepsText.text = trial.result["steps"].ToString();
            distanceText.text = trial.result["distance"].ToString();
        }
        else secondScreen.SetActive(true);

        // Next trial text
        var nextTrial = trial.session.NextTrial;
        var stageName = nextTrial.settings.GetString("stage_name");
        stageText.text = CapitalizeFirstLetter(stageName);
        timeText.text = nextTrial.settings.GetString("runtime");

        int nTrials = nextTrial.block.trials.Count;
        if (nTrials > 1)
        {
            trialParent.SetActive(true);
            trialText.text = $"{nextTrial.numberInBlock}/{nTrials}";
        }
        else
            trialParent.SetActive(false);
        
        // Stage info text
        stageInfoText.text = (string)nextTrial.settings.GetDict("stage_info")[stageName];
    }
    
    private string CapitalizeFirstLetter (string str) => str.Length == 0 ? "" : $"{char.ToUpper(str[0])}{str.Substring(1)}";
}
