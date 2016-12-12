
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#pragma warning disable 0618 // obsolete method used.

public class PlayerController : MonoBehaviour {

	public float speed;

	//private Rigidbody2D rigid_body_component;
	private bool right_holding = false;
	private bool left_holding = false;
	private bool up_holding = false;
	private bool down_holding = false;
	private bool re_enter_holding = false;
	private bool restart_holding = false;
	private Vector3 previous_position;
	private bool is_moving = false;
	private bool is_recursing = false;
	private bool is_exiting = false;
	private Vector3 next_position;
	private float start_time;

	public Stack<GameObject> room_stack;
	public Text recursion_level_display;
	public Text pickup_count_display;
	public GameObject room_prefab;

	public int bananas = 0;
	// Use this for initialization
	void Start () {

		Debug.Log(System.Environment.Version);


		room_stack = new Stack<GameObject> ();
		room_stack.Push(GameObject.Find ("room"));
		gameObject.transform.position = room_stack.Peek ().GetComponent<PlayerLocation> ().starting_location;
		previous_position = gameObject.transform.position;
		recursion_level_display.text = currentRecursionLevelDisplay(room_stack);
		pickup_count_display.text = currentPickupCountDisplay (bananas);
	}

	void Update() {
		if (is_moving) {
			update_move ();
		} else if (is_recursing) {
			update_recurse ();
		} else if (is_exiting) {
			update_exit ();
		}
	}

	void FixedUpdate() {
		move_player ();
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.CompareTag ("Pick Up")) {
			pick_up_object (other.gameObject);
		} else if (other.gameObject.CompareTag ("Exit")) {
			exit_room (other.gameObject);
		} else if (other.gameObject.CompareTag ("Wall")) {
			bump_into_wall ();
		}
	}

	void move_player() {
		bool recurse = Input.GetKey (KeyCode.R);
		bool right = Input.GetKey(KeyCode.RightArrow);
		bool left  = Input.GetKey(KeyCode.LeftArrow);
		bool up = Input.GetKey(KeyCode.UpArrow);
		bool down  = Input.GetKey(KeyCode.DownArrow);
		bool restart = Input.GetKey (KeyCode.K);

		if (!is_moving && !is_recursing && !is_exiting) {

			if (recurse && !re_enter_holding) {
				re_enter_holding = true;

				if (room_stack.Count () >= 30) {
					stack_overflow ();
				} else {
					recurse_player();
				}
			}

			if (!recurse) {
				Vector3 new_position = gameObject.transform.position;
				if (right && !right_holding) {
					right_holding = true;
					is_moving = true;
					new_position = gameObject.transform.position + Vector3.right;
				} else if (left && !left_holding) {
					left_holding = true;
					is_moving = true;
					new_position = gameObject.transform.position + Vector3.left;
				} else if (up && !up_holding) {
					up_holding = true;
					is_moving = true;
					new_position = gameObject.transform.position + Vector3.up;
				} else if (down && !down_holding) {
					down_holding = true;
					is_moving = true;
					new_position = gameObject.transform.position + Vector3.down;
				} else if (restart && !restart_holding) {
					Debug.Log ("restarting level");
					SceneManager.LoadScene ("Level1");
				}

				previous_position = gameObject.transform.position;
				start_time = Time.time;
				next_position = new_position;
			
				if (room_stack.Count () > 0) {
					room_stack.Peek ().GetComponent<PlayerLocation> ().location = new Vector2 (gameObject.transform.position.x, gameObject.transform.position.y);
				}

				if (!right) {
					right_holding = false;
				}
				if (!left) {
					left_holding = false;
				}
				if (!up) {
					up_holding = false;
				}
				if (!down) {
					down_holding = false;
				}
				if (!recurse) {
					re_enter_holding = false;
				}
			}
		}
	}

	string currentRecursionLevelDisplay(Stack<GameObject> room_stack) {
		return "R-Level: " + (room_stack.Count () - 1).ToString ();
	}

	string currentPickupCountDisplay(int item_count) {
		return "Item count: " + item_count.ToString();
	}

	void pick_up_object(GameObject obj) {
		obj.SetActive (false);
		bananas++;
		if (room_stack.Peek().gameObject.GetComponent<LockController>().exit_sprite.gameObject.GetComponent<ExitController>().can_leave(bananas)) {
			foreach (GameObject room in room_stack) {
				room.GetComponent<LockController> ().lock_sprite.gameObject.SetActive (false);
			}
		}
		pickup_count_display.text = currentPickupCountDisplay (bananas);
	}

	void exit_room(GameObject exit) {
		if (exit.GetComponent<ExitController>().can_leave(bananas)) {
			leave_room ();
		} else {
			bump_into_wall();
		}
	}

	void bump_into_wall() {
		start_time = Time.time;
		next_position = previous_position;
	}

	void stack_overflow() {
		Debug.Log ("Stack Overflow: Too many levels, ya dingus");
	}

	void recurse_player() {
		// add new room to the stack
		GameObject room = (GameObject)Instantiate (room_prefab, Vector3.zero, Quaternion.identity);

		// deactivate current room
		room_stack.Peek ().SetActive (false);

		// activate all children in new room
		room.SetActiveRecursively (true);

		// push new room onto the stack
		room_stack.Push (room);
		//room_stack.Add(room);

		// move player to starting location
		start_time = Time.time;
		next_position = room_stack.Peek ().GetComponent<PlayerLocation> ().starting_location;
		is_recursing = true;
		gameObject.GetComponent<BoxCollider2D> ().enabled = false;

		// set recursion level display to one deeper
		recursion_level_display.text = currentRecursionLevelDisplay (room_stack);

		//room_stack[0].GetComponent<PlayerLocation>().location.x = 4;
		//Debug.Log (room_stack.Peek().GetComponent<PlayerLocation>().location.x);
	}

	void leave_room() {
		// destory current room (pop off stack)
		Destroy (room_stack.Pop ());

		if (room_stack.Count () == 0) {
			Debug.Log ("you win!");
		} else {

			is_moving = false;
			is_exiting = true;
			// move player to previous location
			start_time = Time.time;
			next_position = room_stack.Peek ().GetComponent<PlayerLocation> ().location;
			gameObject.GetComponent<BoxCollider2D> ().enabled = false;
			// reactivate previous
			room_stack.Peek ().SetActive (true);
			// change recursion level display
			recursion_level_display.text = currentRecursionLevelDisplay (room_stack);
		}
	}

	void update_move() {
		gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, next_position, (Time.time - start_time) / 0.05f);
		if (Time.time - start_time > 0.05f) {
			gameObject.transform.position = next_position;
			is_moving = false;
			gameObject.GetComponent<BoxCollider2D> ().enabled = true;
		}
	}

	void update_recurse() {
		gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, next_position, (Time.time - start_time) / 0.35f);
		if (Time.time - start_time > 0.35f) {
			gameObject.transform.position = next_position;
			is_recursing = false;
			gameObject.GetComponent<BoxCollider2D> ().enabled = true;
		}
	}

	void update_exit() {
		gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, next_position, (Time.time - start_time) / 0.35f);
		if (Time.time - start_time > 0.35f) {
			gameObject.transform.position = next_position;
			is_exiting = false;
			gameObject.GetComponent<BoxCollider2D> ().enabled = true;
		}
	}
}
