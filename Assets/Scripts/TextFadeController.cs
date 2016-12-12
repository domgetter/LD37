using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFadeController : MonoBehaviour {

	public float start_time;
	public float time_alive;

	private Text text;
	// Use this for initialization
	void Start () {
		text = gameObject.GetComponent<Text> ();
		text.CrossFadeAlpha (0.0f, 0.0f, false);
		StartCoroutine (MyMethod ());
	}
	
	// Update is called once per frame
	void Update () {
	}

	IEnumerator MyMethod() {
		yield return new WaitForSeconds(start_time);
		text.CrossFadeAlpha (1.0f, 1.0f, false);
		yield return new WaitForSeconds(time_alive);
		text.CrossFadeAlpha (0.0f, 1.0f, false);
	}

}
