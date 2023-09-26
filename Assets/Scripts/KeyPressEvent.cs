using UnityEngine;
using UnityEngine.Events;

public class KeyPressEvent : MonoBehaviour
{
	public KeyCode eventKey = KeyCode.F1;

	public UnityEvent onKeypress;

	void Update()
	{
		if (Input.GetKeyDown(eventKey))
			onKeypress.Invoke();
	}
}
