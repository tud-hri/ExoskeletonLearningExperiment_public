using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRQuestionnaireToolkit;

public class QuestionnaireManager : MonoBehaviour
{
    private GenerateQuestionnaire questionnaires;
    private Canvas qCanvas;
    private int nLeftBeforeStop = 0;

    private void Start()
    {
        questionnaires = FindObjectOfType<GenerateQuestionnaire>();
        qCanvas = questionnaires.GetComponent<Canvas>();
    }


    public void CheckQuestionnaireStop()
    {
        nLeftBeforeStop--;
        Debug.Log($"Questionnaires left: {nLeftBeforeStop}");
        if (nLeftBeforeStop == 0) qCanvas.enabled = false;
    }


    public void ShowQuestionnaires(int nQuestionnaires)
    {
        qCanvas.enabled = true;
        nLeftBeforeStop = nQuestionnaires;
        questionnaires.ShowQuestionnaire();
    }

    public void SetNumQuestionnaire(int num){
        questionnaires.SetNumQuestionnaire(num);
    }
}
