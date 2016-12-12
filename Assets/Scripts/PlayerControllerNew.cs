using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerNew : MonoBehaviour {

	private enum States {Idle, Moving, Exiting, Recursing};
	private States state = States.Idle;
	private Vector3 next_position = Vector3.zero;
	private Vector3 previous_position = Vector3.zero;
	public Vector3 start_location = Vector3.zero;
	public int bananas = 0;
	public Transform level;

	private float start_time = 0.0f;

	// Update is called once per frame
	void Update () {
		if (state == States.Moving) {
			update_move ();
		} else if (state == States.Exiting) {
			update_exit ();
		} else if (state == States.Recursing) {
			update_recurse ();
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.CompareTag ("Pick Up")) {
			pick_up_object (other.gameObject);
		} else if (other.gameObject.CompareTag ("Lock")) {
			bump_into_wall ();
		} else if (other.gameObject.CompareTag ("Exit")) {
			exit_room ();
		} else if (other.gameObject.CompareTag ("Wall")) {
			bump_into_wall ();
		}
	}

	void exit_room() {
		level.SendMessage ("PlayerExitRoom");
	}

	void bump_into_wall() {
		Debug.Log ("Bumping into wall");
		start_time = Time.time;
		state = States.Moving;
		next_position = previous_position;
	}

	void pick_up_object(GameObject obj) {
		level.SendMessage ("PlayerHitBanana", obj);
	}

	void update_move() {
		lerp_to_idle (0.05f);
	}

	void update_exit() {
		lerp_to_idle (0.35f);
	}

	void update_recurse() {
		lerp_to_idle (0.35f);
	}

	void lerp_to_idle(float time) {
		gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, next_position, (Time.time - start_time) / time);
		if (Time.time - start_time > time) {
			gameObject.transform.position = next_position;
			state = States.Idle;
			gameObject.GetComponent<BoxCollider2D> ().enabled = true;
		}
	}

	void MoveUp() {
		if (state == States.Idle) {
			state = States.Moving;
			next_position = transform.position + Vector3.up;
			previous_position = transform.position;
			start_time = Time.time;
		}
	}

	void MoveDown() {
		if (state == States.Idle) {
			state = States.Moving;
			next_position = transform.position + Vector3.down;
			previous_position = transform.position;
			start_time = Time.time;
		}
	}

	void MoveRight() {
		if (state == States.Idle) {
			state = States.Moving;
			next_position = transform.position + Vector3.right;
			previous_position = transform.position;
			start_time = Time.time;
		}
	}

	void MoveLeft() {
		if (state == States.Idle) {
			state = States.Moving;
			next_position = transform.position + Vector3.left;
			previous_position = transform.position;
			start_time = Time.time;
		}
	}

	void MoveStart() {
		transform.position = start_location;
		previous_position = transform.position;
	}

	void Recurse() {
		state = States.Recursing;
		gameObject.GetComponent<BoxCollider2D> ().enabled = false;
		level.SendMessage ("RecordPosition");
		next_position = start_location;
		previous_position = transform.position;
		start_time = Time.time;
	}

	void ExitRoom(Vector3 saved_position) {
		state = States.Exiting;
		gameObject.GetComponent<BoxCollider2D> ().enabled = false;
		next_position = saved_position;
		previous_position = saved_position;
		start_time = Time.time;
	}

	void AddBanana() {
		bananas++;
	}
}
