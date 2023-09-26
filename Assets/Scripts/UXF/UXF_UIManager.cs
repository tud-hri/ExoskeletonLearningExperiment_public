using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using UXF.UI;

public class UXF_UIManager : MonoBehaviour
{
    [SerializeField] private StartScreenUI startScreenUI;
    public UIController UIController;
    public UXF_TaskManager taskManager;
    
    private PopupController popupController;

    private void Start()
    {
        if (UIController == null) UIController = FindObjectOfType<UIController>(true);
        if (popupController == null) popupController = UIController.GetComponentInChildren<PopupController>(true);
    }

    private void InitUXF_UI()
    {
        // May be a bit obsolute but leaving this here to potentially have some use in the future.
        UIController.gameObject.SetActive(true);
        
        //UXF.UI.
    }
    
    public void ShowStartupPopup(Session session)
    {
        InitUXF_UI();
        popupController.gameObject.SetActive(true);
        
        Popup startingPopup = new Popup
        {
            messageType = MessageType.Attention,
            message = "Press OK to start the experiment",
            onOK = session.BeginNextTrial
        };
        
        popupController.DisplayPopup(startingPopup);
    }
}
