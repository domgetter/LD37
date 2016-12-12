using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LoadTitleScreen : MonoBehaviour {

	// Use this for initialization
	private bool skipped;
	void Start () {
		skipped = false;
		StartCoroutine (LoadTitleScreenScene());
	}

	void Update() {

		if (Input.GetKey (KeyCode.Return)) {
			skipped = true;
			SceneManager.LoadScene ("title_screen", LoadSceneMode.Single);
		}
	}

	IEnumerator LoadTitleScreenScene() {
		yield return new WaitForSeconds(43);
		if (!skipped) {
			SceneManager.LoadScene ("title_screen", LoadSceneMode.Single);
		}
	}
}
