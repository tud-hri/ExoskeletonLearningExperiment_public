using System.Collections;
using UnityDataReplay.ReplayerTypes;
using UnityEngine;

public class WeightShiftDetection : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform maskOffset;

    public Vector3 additionalOffset;

    private StepReplayManager _stepReplayManager;
    private float _stepWidth;
    private bool _weightShiftCondition;

    private Coroutine _coroutine;
    private bool _cooldown;
    private bool wasPlaying;

    public bool Correct => _weightShiftCondition; // Is the weightShifting condition met?

    // Start is called before the first frame update
    private void Start()
    {
        _stepReplayManager = FindObjectOfType<StepReplayManager>();
        if (_stepReplayManager == null) Debug.LogError("Could not find StepReplayManager, is it in the scene?");
    }


    // Update is called once per frame
    private void Update()
    {
        if (wasPlaying != _stepReplayManager.IsPlaying())
        {
            _cooldown = true;
            StartCoroutine(WaitForNSeconds(0.2f));
        }
        wasPlaying = _stepReplayManager.IsPlaying();

        if (_stepReplayManager.IsPlaying()) return;

        var leadingLeg = _stepReplayManager.FrontLeg();
        if (leadingLeg == Leg.Left)
            _stepWidth = -_stepReplayManager.stepWidth / 2;
        else
            _stepWidth = _stepReplayManager.stepWidth / 2;


        // Line Position
        var currPos = maskOffset.position;
        currPos.z = _stepReplayManager.GetLegPos(_stepReplayManager.FrontLeg());
        currPos.x = _stepWidth;
        maskOffset.position = currPos + additionalOffset;
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
        spriteRenderer.color = Color.green;
        _weightShiftCondition = true;
    }

    private void OnTriggerExit(Collider other)
    {
        _weightShiftCondition = false;

        if (!_stepReplayManager.IsPlaying() && !_cooldown) _coroutine = DoBlink();
        else spriteRenderer.color = Color.white;
    }
    
    public Coroutine DoBlink() => StartCoroutine(BlinkMaterialColor(2,0.3f));

    private IEnumerator BlinkMaterialColor(int blinkTimes, float blinkTime)
    {
        

        for (var i = 0; i < blinkTimes; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(blinkTime/2);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(blinkTime/2);
        }
    }

    private IEnumerator WaitForNSeconds(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        _cooldown = false;
    }

}
