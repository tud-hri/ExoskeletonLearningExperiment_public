using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private bool connected = false;

    //public GameObject startMenu;
    public InputField usernameField;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void ReconnectToServer()
    {
        Debug.Log("Forcing reconnection to server...");
        Client.instance.ReconnectToServer();
    }

    public void ConnectToServer()
    {
        if (connected)
        {
            Debug.Log("Already connected, try reconnecting!");
            return;
        }
        else Debug.Log("Connecting to server...");
        
        connected = true;
        
        //startMenu.SetActive(false);
        //usernameField.interactable = false;
        Client.instance.ConnectToServer();
        Debug.Log("Connected to server!");
    }
}
