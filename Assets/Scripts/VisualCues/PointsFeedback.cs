using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UXF;

public class PointsFeedback : MonoBehaviour
{
    [SerializeField] private GaitFeedback _gaitFeedback;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Text pointsText;
    [SerializeField] private Image panelImage;

    [SerializeField] private float fadeSpeed = 1.0f;
    
    private Color startColorPanel;
    float progress = 0.0f;
    
    private void Start()
    {
        startColorPanel = panelImage.color;
    }

    private void Update()
    {
        progress += fadeSpeed * Time.deltaTime;
        _canvasGroup.alpha = Mathf.Lerp(1.0f, 0.0f,progress);
    }

    public void ShowStepPoints(Step step)
    {
        var score = "+" + Mathf.Round(step.Score);
        var color = _gaitFeedback.EvaluateGradient(step.ScaledInput); // 
        
        SetPointText(score, color);
    }

    private void SetPointText(string pointsString, Color color)
    {
        pointsText.text = pointsString;
        
        pointsText.color = color;
        var panelAlpha = color;
        panelAlpha.a = startColorPanel.a;
        panelImage.color = panelAlpha;
        
        progress = 0.0f;
    }
}
