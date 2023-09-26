using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerBlinking : MonoBehaviour
{
    [SerializeField] private GameObject Walker;

    private Material _walkerMaterial;
    private Color _walkerBaseColor;

    private bool _isBlinking = false;
    public bool IsBlinking => _isBlinking;

    private void Start()
    {
        _walkerMaterial = Walker.GetComponent<MeshRenderer>().material;
        _walkerBaseColor = _walkerMaterial.color;
    }


    public void DoBlink() => StartCoroutine(BlinkMaterialColor(2, 0.3f));

    private IEnumerator BlinkMaterialColor(int blinkTimes, float blinkTime)
    {
        _isBlinking = true;
        var baseColor = _walkerBaseColor;//_walkerMaterial.color;
        for (var i = 0; i < blinkTimes; i++)
        {
            _walkerMaterial.color = Color.red;
            yield return new WaitForSeconds(blinkTime / 2);
            _walkerMaterial.color = baseColor;
            yield return new WaitForSeconds(blinkTime / 2);
        }

        _isBlinking = false;
    }

}
