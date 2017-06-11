using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using DG.Tweening;

public enum Command {
	NONE,
	MOVE_UP,
	MOVE_DOWN,
	MOVE_LEFT,
	MOVE_RIGHT,
	GRAB,
	THROW
}

public class PlayerOnGrid : NetworkBehaviour {
	[SyncVar]
	public bool all_set = false;
	[SyncVar]
	public int player_ID;
	[SyncVar]
	public int current_column = -1;
	[SyncVar]
	public BallColor color_carried = BallColor.NONE;
	[SyncVar]
	public int quantity_carried = 0;

	[SerializeField]
	Grid grid;

	public override void OnStartLocalPlayer() {
		int number_players = 0;

		foreach (GameObject aux in GameObject.FindGameObjectsWithTag("Player")) {
			if (aux.GetComponentInChildren<PlayerOnGrid>().all_set) {
				number_players++;
			}
		}

		Set_ID(number_players);

		grid = GameObject.FindGameObjectsWithTag("Grid")[number_players].GetComponent<Grid>();
		grid.grid_ID = player_ID;
		grid.player = this;

		Move(grid.Get_Player_Initial_Position());
	}

	void Start() {
		this.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);

		Move(GameObject.FindGameObjectsWithTag("Grid")[player_ID].GetComponent<Grid>().Get_Player_Initial_Position());
	}

	void Set_ID(int ID) {
		player_ID = ID;
		all_set = true;
	}

	void Update() {
		if (!isLocalPlayer)
			return;

		Command cmd = Command.NONE;

		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			cmd = Command.MOVE_LEFT;
		}

		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			cmd = Command.MOVE_RIGHT;
		}

		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			cmd = Command.MOVE_UP;
		}

		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			cmd = Command.MOVE_DOWN;
		}

		if (Input.GetKeyDown(KeyCode.A)) {
			cmd = Command.GRAB;
		}

		if (Input.GetKeyDown(KeyCode.S)) {
			cmd = Command.THROW;
		}

		if (cmd != Command.NONE) {
			Receive_Command(cmd);
		}
	}

	void Receive_Command(Command cmd) {
		if (cmd == Command.GRAB) {
			grid.Player_Grab(this);
		}

//		if (cmd == Command.THROW) {
//			grid.Throw(this);
//		}

		Move(grid.Get_Player_New_Tile(this, cmd));
	}

	void Move(Tile dest) {
		current_column = (dest.tile_ID) % 7;
		this.transform.DOMove(dest.transform.position, 0.1f);
	}

	public static int Get_Local_Player_ID() {
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
			if (player.GetComponentInChildren<PlayerOnGrid>().Is_Local_Player()) {
				return player.GetComponentInChildren<PlayerOnGrid>().player_ID;
			}
		}

		Debug.Log("This should not be happening.");
		return -1;
	}

	[Command]
	public void Cmd_Spawn_Line_Of_Balls(int playerID, int[] colors) {
		if (grid == null) {
			grid = GameObject.FindGameObjectsWithTag("Grid")[playerID].GetComponent<Grid>();
		}

		grid.Spawn_Line_Of_Balls(colors);
	}

	public bool Is_Local_Player() {
		return isLocalPlayer;
	}
}