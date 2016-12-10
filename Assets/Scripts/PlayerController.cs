using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	public float speed;
	public float max_speed;

	//private Rigidbody2D rigid_body_component;
	private bool right_holding = false;
	private bool left_holding = false;
	private bool up_holding = false;
	private bool down_holding = false;
	private bool re_enter_holding = false;
	private Vector3 previous_position;

	public Stack<GameObject> room_stack;
	public Text recursion_level_display;
	public GameObject room_prefab;

	public int bananas = 0;
	// Use this for initialization
	void Start () {
		room_stack = new Stack<GameObject> ();
		room_stack.Push(GameObject.Find ("room"));
		gameObject.transform.position = room_stack.Peek ().GetComponent<PlayerLocation> ().starting_location;
		previous_position = gameObject.transform.position;
		recursion_level_display.text = currentRecursionLevelDisplay(room_stack);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate() {
		move_player ();


	}

	bool velocity_less_than_max(Rigidbody2D rigid_body) {
		return rigid_body.velocity.magnitude < max_speed;
		//return true;
	}

	void OnTriggerEnter2D(Collider2D other) {
		Debug.Log (other);
		if(other.gameObject.CompareTag("Pick Up")) {
			other.gameObject.SetActive (false);
			bananas++;
		}

		if (other.gameObject.CompareTag ("Exit")) {
			// destory current room (pop off stack)
			Debug.Log(room_stack.Count());
			Destroy(room_stack.Pop());

			if (room_stack.Count () == 0) {
				Debug.Log ("you win!");
			} else {

				// move player to previous location
				gameObject.transform.position = room_stack.Peek ().GetComponent<PlayerLocation> ().location;
				// reactivate previous
				room_stack.Peek ().SetActive (true);
				// change recursion level display
				recursion_level_display.text = currentRecursionLevelDisplay(room_stack);
			}
		}

		if (other.gameObject.CompareTag ("Wall")) {
			Debug.Log ("Bumped into wall");
			gameObject.transform.position = previous_position;
		}
	}

	void move_player() {
		bool re_enter = Input.GetKey (KeyCode.R);
		bool right = Input.GetKey(KeyCode.RightArrow);
		bool left  = Input.GetKey(KeyCode.LeftArrow);
		bool up = Input.GetKey(KeyCode.UpArrow);
		bool down  = Input.GetKey(KeyCode.DownArrow);

		if (re_enter && !re_enter_holding) {
			re_enter_holding = true;

			// add new room to the stack
			GameObject room = (GameObject) Instantiate(room_prefab, Vector3.zero, Quaternion.identity);

			// deactivate current room
			room_stack.Peek().SetActive(false);

			// activate all children in new room
			room.SetActiveRecursively(true);

			// push new room onto the stack
			room_stack.Push(room);
			//room_stack.Add(room);

			// move player to starting location
			gameObject.transform.position = room_stack.Peek().GetComponent<PlayerLocation>().starting_location;

			// set recursion level display to one deeper
			recursion_level_display.text = currentRecursionLevelDisplay(room_stack);

			//room_stack[0].GetComponent<PlayerLocation>().location.x = 4;
			//Debug.Log (room_stack.Peek().GetComponent<PlayerLocation>().location.x);
		}

		if (!re_enter) {
			Vector3 new_position = gameObject.transform.position;
			if (right && !right_holding) {
				right_holding = true;
				new_position = gameObject.transform.position + Vector3.right;
			} else if (left && !left_holding) {
				left_holding = true;
				new_position = gameObject.transform.position + Vector3.left;
			} else if (up && !up_holding) {
				up_holding = true;
				new_position = gameObject.transform.position + Vector3.up;
			} else if (down && !down_holding) {
				down_holding = true;
				new_position = gameObject.transform.position + Vector3.down;
			}

			previous_position = gameObject.transform.position;
			gameObject.transform.position = new_position;
			

			room_stack.Peek().GetComponent<PlayerLocation>().location = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);

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
			if (!re_enter) {
				re_enter_holding = false;
			}
		}
	}

	string currentRecursionLevelDisplay(Stack<GameObject> room_stack) {
		return "R-Level: " + (room_stack.Count () - 1).ToString ();
	}
}
