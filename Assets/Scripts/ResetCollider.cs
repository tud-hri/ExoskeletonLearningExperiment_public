using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCollider : MonoBehaviour
{

    [SerializeField] private Collider _collider;

    public void Reset()
    {
        StartCoroutine(ToggleCollider());
    }

    private IEnumerator ToggleCollider()
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(0.01f);
        _collider.enabled = true;
    }
}
