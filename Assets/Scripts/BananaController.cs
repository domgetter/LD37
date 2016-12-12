using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BananaController : MonoBehaviour {

	private Vector3 original_position;
	// Use this for initialization
	void Start () {
		original_position = gameObject.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.position = original_position + new Vector3 (0.0f, Mathf.Sin(Time.time*2.0f)/10.0f, 0.0f);
	}
}
