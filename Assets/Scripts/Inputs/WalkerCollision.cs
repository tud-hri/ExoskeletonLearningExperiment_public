using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RootMotion.Demos;

public class WalkerCollision : MonoBehaviour
{
    [SerializeField] GameObject cameraLateral3PP;

    public int walkerCollision = 0;
    public Text UITimeText3PP;
    public Text UIPenaltyText3PP;
    public Text UITimeText1PP;
    public Text UIPenaltyText1PP;
    private float time;
    private float timePenalty = 2f;
    private bool _initialize = false;

    private void Start()
    {     
        UITimeText3PP.transform.parent.gameObject.SetActive(false);
        UIPenaltyText3PP.transform.parent.gameObject.SetActive(false);
        UITimeText1PP.transform.parent.gameObject.SetActive(false);
        UIPenaltyText1PP.transform.parent.gameObject.SetActive(false);
        time = timePenalty;
    }

    public void Initialize()
    {
        _initialize = true;   
    }

    public int IsWalkerCollision() => walkerCollision;

    public void resetWalkerCollision() => walkerCollision = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (_initialize) // <<<<<<<<<<<< Do we actually need this?
        {
            walkerCollision = 1;
            time = timePenalty;

            if (cameraLateral3PP.activeSelf == true)
            {
                UITimeText3PP.transform.parent.gameObject.SetActive(true);
                UIPenaltyText3PP.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                UITimeText1PP.transform.parent.gameObject.SetActive(true);
                UIPenaltyText1PP.transform.parent.gameObject.SetActive(true);
            }

        }
    }

    private void Update()
    {
        if (walkerCollision == 1 && time >= 0f)
        {
            time -= Time.deltaTime;
            UITimeText3PP.text = time.ToString("f2");
            UITimeText1PP.text = time.ToString("f2");
        }
        else if (time < 0f)
        {
            walkerCollision = 2;  // Intermediate value between collision (1) and no-collision (0) to Reset Position 

            UITimeText3PP.transform.parent.gameObject.SetActive(false);
            UIPenaltyText3PP.transform.parent.gameObject.SetActive(false);
            UITimeText1PP.transform.parent.gameObject.SetActive(false);
            UIPenaltyText1PP.transform.parent.gameObject.SetActive(false);
            time = timePenalty;
        }
    }

}
