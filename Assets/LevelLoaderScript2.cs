using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class LevelLoaderScript2 : MonoBehaviour {

	public Transform wall_brick_prefab;
	public Transform walls_prefab;
	public Transform floor_brick_prefab;
	public Transform floors_prefab;
	public Transform room_base_prefab;
	public Transform rooms_prefab;
	public Transform bananas_prefab;
	public Transform banana_prefab;
	public Transform locks_prefab;
	public Transform lock_prefab;
	public Transform exits_prefab;
	public Transform exit_prefab;

	public Text recursion_level_text;
	public Text banana_text;
	public Text stack_overflow_warning;
	private bool warning_displayed = false;

	public Transform player;

	//keyboard state
	private bool right_holding = false;
	private bool left_holding = false;
	private bool up_holding = false;
	private bool down_holding = false;
	private bool recurse_holding = false;
	private bool restart_holding = false;

	private Vector3 player_start_location = Vector3.zero;

	private Stack<Transform> room_stack = new Stack<Transform>();

	void Start () {

		Debug.Log(System.Environment.Version);
		string[] level_map =
		  {"XXXXXXXX",
		   "X.P....X",
		   "X...B..E",
		   "X......X",
		   "XXXXXXXX"};
		int level_requirements = 3;

		float offset_x = -(level_map[0].Length)/2.0f + 0.5f;
		float offset_y = level_map.Length/2.0f - 0.5f;

		for (int row_index = 0; row_index < level_map.Length; row_index++) {
			string row = level_map[row_index];
			for (int column_index = 0; column_index < row.Length; column_index++) {
				instantiate_tile (row[column_index], column_index, -row_index, offset_x, offset_y);
			}
		}
		walls_prefab.transform.SetParent (room_base_prefab.transform);
		floors_prefab.transform.SetParent (room_base_prefab.transform);
		bananas_prefab.transform.SetParent (room_base_prefab.transform);
		locks_prefab.transform.SetParent (room_base_prefab.transform);
		exits_prefab.transform.SetParent (room_base_prefab.transform);

		room_base_prefab.gameObject.SetActive (false);

		room_base_prefab.gameObject.GetComponent<RoomController> ().player_start_location = player_start_location;
		room_base_prefab.gameObject.GetComponent<ExitController> ().required_count = level_requirements;

		Transform room = Instantiate (room_base_prefab, rooms_prefab.transform);
		room_stack.Push (room);
		room.gameObject.SetActive (true);
		player.GetComponent<PlayerControllerNew>().start_location = player_start_location;
		player.SendMessage ("MoveStart");

		recursion_level_text.GetComponent<RectTransform> ().localPosition = new Vector3 (-22.5f * (level_map[0].Length),32.5f * (level_map.Length),0.0f);
		banana_text.GetComponent<RectTransform> ().localPosition = new Vector3 (22.5f * (level_map[0].Length),32.5f * (level_map.Length),0.0f);
		set_rlevel_text ();
		set_banana_text();
		stack_overflow_warning.CrossFadeAlpha (0.0f, 0.0f, false);
	}

	void Update() {
		if (room_stack.Peek ().GetComponent<ExitController>().can_leave(player.GetComponent<PlayerControllerNew>().bananas)) {
			room_stack.Peek ().Find ("locks").GetChild(0).gameObject.SetActive(false);
			room_stack.Peek ().Find ("exits").GetChild(0).gameObject.SetActive(true);
		}
	}

	void instantiate_tile(char tile_type, int x, int y, float offset_x, float offset_y) {
		Vector3 position = transform.position + (y + offset_y) * Vector3.up + (x + offset_x) * Vector3.right;
		if (tile_type == 'X') {
			Transform new_object = Instantiate (wall_brick_prefab, position, Quaternion.identity);
			new_object.gameObject.SetActive (true);
			new_object.gameObject.transform.SetParent (walls_prefab.transform);
		}
		if (tile_type == '.' || tile_type == 'P' || tile_type == 'B' || tile_type == 'E') {
			Transform new_object = Instantiate (floor_brick_prefab, position, Quaternion.identity);
			new_object.gameObject.SetActive (true);
			new_object.gameObject.transform.SetParent (floors_prefab.transform);
		}
		if (tile_type == 'B') {
			Transform new_object = Instantiate (banana_prefab, position, Quaternion.identity);
			new_object.gameObject.SetActive (true);
			new_object.gameObject.transform.SetParent (bananas_prefab.transform);
		}
		if (tile_type == 'P') {
			player_start_location = position;
		}
		if (tile_type == 'E') {
			Transform new_lock = Instantiate (lock_prefab, position, Quaternion.identity);
			new_lock.gameObject.SetActive (true);
			new_lock.gameObject.transform.SetParent (locks_prefab.transform);
			Transform new_exit = Instantiate (exit_prefab, position, Quaternion.identity);
			//new_exit.gameObject.SetActive (true);
			new_exit.gameObject.transform.SetParent (exits_prefab.transform);
		}
	}

	void FixedUpdate() {
		bool recurse_key = Input.GetKey (KeyCode.R);
		bool right_key = Input.GetKey(KeyCode.RightArrow);
		bool left_key  = Input.GetKey(KeyCode.LeftArrow);
		bool up_key = Input.GetKey(KeyCode.UpArrow);
		bool down_key  = Input.GetKey(KeyCode.DownArrow);
		bool restart_key = Input.GetKey (KeyCode.K);

		if (recurse_key) {
			if (!recurse_holding) {
				recurse();
				recurse_holding = true;
			}
		} else {
			recurse_holding = false;
		}

		if (right_key) {
			if (!right_holding) {
				player.SendMessage ("MoveRight");
				right_holding = true;
			}
		} else {
			right_holding = false;
		}

		if (left_key) {
			if (!left_holding) {
				player.SendMessage ("MoveLeft");
				left_holding = true;
			}
		} else {
			left_holding = false;
		}

		if (up_key) {
			if (!up_holding) {
				player.SendMessage ("MoveUp");
				up_holding = true;
			}
		} else {
			up_holding = false;
		}

		if (down_key) {
			if (!down_holding) {
				player.SendMessage ("MoveDown");
				down_holding = true;
			}
		} else {
			down_holding = false;
		}

		if (restart_key) {
			if (!restart_holding) {
				SceneManager.LoadScene ("Level_with_brick2", LoadSceneMode.Single);
				player.SendMessage ("Restart");
				restart_holding = true;
			}
		} else {
			restart_holding = false;
		}

	}

	void recurse(){

		if (room_stack.Count () > 9) {
			Debug.Log ("stack overflow");
			// tell user Cannot Recurse: Stack Overflow Imminent!
			if (!warning_displayed) {
				StartCoroutine (WarnStackOverflow ());
			}
		} else {
			// deactivate current room
			room_stack.Peek ().gameObject.SetActive (false);

			// add new room to the stack
			Transform room = Instantiate (room_base_prefab, rooms_prefab.transform);
			room_stack.Push (room);
			room.gameObject.SetActive (true);

			set_rlevel_text ();

			// move player to starting location
			player.SendMessage ("Recurse");
		}

	}

	void PlayerHitBanana(GameObject banana) {
		player.SendMessage ("AddBanana");
		set_banana_text ();
		banana.SetActive (false);
	}

	void RecordPosition() {
		room_stack.Peek ().GetComponent<RoomController> ().player_location = player.transform.position;
	}

	void PlayerExitRoom() {

		// destory current room (pop off stack)
		if (room_stack.Count () == 1) {
			Debug.Log ("you win!");
			// go to next level
			SceneManager.LoadScene ("win_screen", LoadSceneMode.Single);
		} else {
			Destroy (room_stack.Pop ().gameObject);
			set_rlevel_text ();

			player.SendMessage ("ExitRoom", room_stack.Peek().GetComponent<RoomController>().player_location);
			// reactivate previous
			room_stack.Peek ().gameObject.SetActive (true);
			// change recursion level display
			//recursion_level_display.text = currentRecursionLevelDisplay (room_stack);
		}
	}

	void set_rlevel_text() {
		recursion_level_text.text = "R-Level: " + (room_stack.Count() - 1).ToString();
	}

	void set_banana_text() {
		banana_text.text = "Banans: " + (player.GetComponent<PlayerControllerNew>().bananas).ToString();
	}

	IEnumerator WarnStackOverflow() {
		Debug.Log ("stack overflow coroutine");
		warning_displayed = true;
		stack_overflow_warning.CrossFadeAlpha (1.0f, 0.2f, false);
		yield return new WaitForSeconds(1.8f);
		stack_overflow_warning.CrossFadeAlpha (0.0f, 0.5f, false);
		yield return new WaitForSeconds(0.5f);
		warning_displayed = false;
	}
}
