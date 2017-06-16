using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
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
	[SyncVar(hook ="CallHook")]
	public int player_ID = -1;
	[SyncVar]
	public int current_column = -1;
	[SyncVar]
	public BallColor color_carried = BallColor.NONE;
	[SyncVar]
	public int quantity_carried = 0;
	
	public bool is_having_animation = false;

	public bool game_is_over = false;

	[SerializeField]
	Grid grid;
	[SerializeField]
	Clock clock;
	[SerializeField]
	Image ball;

	public override void OnStartLocalPlayer() {
		int length = GameObject.FindGameObjectsWithTag("Player").Length;

		if (length == 1) {
			player_ID = 0;
			Cmd_Set_ID(0);
		}
		else {
			Cmd_Set_ID(length - 1);
			player_ID = length - 1;
		}

		grid = GameObject.FindGameObjectsWithTag("Grid")[player_ID].GetComponent<Grid>();
		grid.grid_ID = player_ID;
		grid.player = this;

		Cmd_Move(grid.Get_Player_Initial_Position().tile_ID);
	}

	void CallHook(int num) {
		Debug.Log("New player connected! Player ID: #" + num);
		grid = GameObject.FindGameObjectsWithTag("Grid")[num].GetComponent<Grid>();

		grid.player = this;
	}

	void Start() {
		this.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
		grid = GameObject.FindGameObjectsWithTag("Grid")[player_ID].GetComponent<Grid>();

		if (isLocalPlayer) {
			Cmd_Move(grid.Get_Player_Initial_Position().tile_ID);
			clock = GameObject.FindGameObjectWithTag("Clock").GetComponent<Clock>();
			clock.Gong_Event += grid.Request_Spawn_Line_Of_Balls;
		}
	}

	[Command]
	void Cmd_Set_ID(int ID) {
		player_ID = ID;
	}

	void Update() {
		ball.enabled = quantity_carried > 0;
		ball.color = Tile.Get_Ball_Color(color_carried);

		if (!isLocalPlayer)
			return;

		if (Input.GetKeyDown(KeyCode.G)) {
			Cmd_Game_Over();
		}

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
		if (game_is_over) {
			return;
		}

		if (cmd == Command.GRAB &&
			!is_having_animation) {
			Cmd_Player_Grab();
		}

		if (cmd == Command.THROW &&
			!is_having_animation) {
			Cmd_Player_Throw();
		}

		if (cmd == Command.MOVE_DOWN || cmd == Command.MOVE_UP ||
		 cmd == Command.MOVE_LEFT || cmd == Command.MOVE_RIGHT) {
			Cmd_Move(grid.Get_Player_New_Tile(this, cmd).tile_ID);
		}
	}

	#region Throw
	[Command]
	void Cmd_Player_Throw() {
        int colorCarried = (int) color_carried;
        int quantityCarried = quantity_carried;

		color_carried = BallColor.NONE;
		quantity_carried = 0;
		
        //StartCoroutine(Push_Ball_Animation(colorCarried, quantityCarried));
		Rpc_Player_Throw(colorCarried, quantityCarried);
	}

	[ClientRpc]
	void Rpc_Player_Throw(int colorCarried, int quantityCarried) {
		StartCoroutine(Push_Ball_Animation(colorCarried, quantityCarried));
	}

	IEnumerator Push_Ball_Animation(int colorCarried, int quantityCarried) {
        Debug.Log("quantityCarried: " + quantityCarried);
		is_having_animation = true;

		List<Tile> vacant = grid.Get_Vacant_Tiles_In_Column(current_column);
		List<Tile> use_in_anim = new List<Tile>();
		List<Tile> will_be_filled = new List<Tile>();

		for (int i = 0; i < quantityCarried; i++) {
			//the array contains the vacant tiles closest to player to use during animation
			use_in_anim.Add(vacant[i]);
			//the array contains the vacant tiles that will be filled by the end of animation
			will_be_filled.Add(vacant[vacant.Count - 1 - i]);
		}

		Coroutine animation_to_wait = null;

		for (int i = 0; i < use_in_anim.Count; i++) {
			Tile tl = use_in_anim[i];

			if (!tl.hasBall) {
				animation_to_wait = StartCoroutine(tl.Push_Animation(
					will_be_filled[will_be_filled.Count - 1 - i],
					(BallColor) colorCarried));
			}
		}

		yield return animation_to_wait;
		yield return grid.Sort_Board();

		is_having_animation = false;
	}
	#endregion

	#region Movement
	[Command]
	void Cmd_Move(int dest_tile_ID) {
		Tile dest = grid.Get_Tile_by_ID(dest_tile_ID);

		current_column = (dest.tile_ID) % 7;
		this.transform.DOMove(new Vector3(dest.transform.position.x + 3,
			dest.transform.position.y + 22), 0.1f);

		Rpc_Move(dest_tile_ID);
	}

	[ClientRpc]
	void Rpc_Move(int dest_tile_ID) {
		Tile dest = grid.Get_Tile_by_ID(dest_tile_ID);
		this.transform.DOMove(new Vector3(dest.transform.position.x + 3,
			dest.transform.position.y + 22), 0.1f);
	}
	#endregion

	#region Grab
	[Command]
	void Cmd_Player_Grab() {
		Tile ball_tile = grid.Get_Ball_Nearest_Player(this);

		if (ball_tile != null &&
			(this.color_carried == ball_tile.ballColor ||
			this.color_carried == BallColor.NONE)) {

			this.color_carried = ball_tile.ballColor;
			this.quantity_carried++;

			Rpc_Player_Grab(ball_tile.tile_ID);

			StartCoroutine(Grab_Ball_Movement(ball_tile.tile_ID));
		}
	}

	[ClientRpc]
	void Rpc_Player_Grab(int tile_ID) {
		StartCoroutine(Grab_Ball_Movement(tile_ID));
	}

	IEnumerator Grab_Ball_Movement(int tile_ID) {
		is_having_animation = true;

		Tile tile = grid.Get_Tile_by_ID(tile_ID);
		GameObject ball = tile.Instantiate_Ball_For_Anim();
		tile.Cmd_Deactivate_Ball();

		ball.transform.DOMove(this.transform.position, 0.1f);
		ball.transform.DOScale(new Vector2(0.1f, 0.1f), 0.1f);

		yield return Destroy_Ball(ball, 0.1f);

		is_having_animation = false;
	}

	IEnumerator Destroy_Ball(GameObject ball, float duration) {
		yield return new WaitForSeconds(duration);
		Destroy(ball.gameObject);
	}
	#endregion

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
//		if (grid == null) {
//			grid = GameObject.FindGameObjectsWithTag("Grid")[playerID].GetComponent<Grid>();
//		}
//
//		StartCoroutine(grid.Spawn_Line_Of_Balls(colors));
		Rpc_Spawn_Line_Of_Balls(player_ID, colors);
	}

	[ClientRpc]
	public void Rpc_Spawn_Line_Of_Balls(int playerID, int[] colors) {
		if (grid == null) {
			grid = GameObject.FindGameObjectsWithTag("Grid")[playerID].GetComponent<Grid>();
		}

		StartCoroutine(grid.Spawn_Line_Of_Balls(colors));
	}

	public bool Is_Local_Player() {
		return isLocalPlayer;
	}

	[Command]
	public void Cmd_Game_Over() {
		StartCoroutine(Game_Over(false));
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
			if (player.GetComponent<PlayerOnGrid>().player_ID != this.player_ID) {
				StartCoroutine(player.GetComponent<PlayerOnGrid>().Game_Over(true));
			}
		}

		Rpc_Game_Over();
	}

	[ClientRpc]
	public void Rpc_Game_Over() {
		StartCoroutine(Game_Over(false));
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
			if (player.GetComponent<PlayerOnGrid>().player_ID != this.player_ID) {
				StartCoroutine(player.GetComponent<PlayerOnGrid>().Game_Over(true));
			}
		}
	}

	public IEnumerator Game_Over(bool victory) {
		if (victory) {
			this.GetComponent<Animator>().SetTrigger("victory");
		}
		else {
			this.GetComponent<Animator>().SetTrigger("dead");
        }

        quantity_carried = 0;
        grid.Game_Over(victory);
        if (clock != null) {
            clock.stop_the_clock = true;
        }
        game_is_over = true;

		yield return new WaitForSeconds(0.5f);

//		var tween = this.transform.DOMoveY(this.transform.position.y + 50, 0.5f);
//		tween.SetEase(Ease.Flash);
//
//		yield return new WaitForSeconds(0.5f);
//
//		tween = this.transform.DOMoveY(this.transform.position.y - 150, 0.75f);
//		tween.SetEase(Ease.Flash);
	}

    public IEnumerator Receive_Push() {
        yield return new WaitForSeconds(2.0f);

        grid.Request_Spawn_Line_Of_Balls();
    }

    public PlayerOnGrid Get_Other_Player() {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
            if (player.GetComponent<PlayerOnGrid>().player_ID != this.player_ID) {
                return player.GetComponent<PlayerOnGrid>();
            }
        }

        return null;
    }
}