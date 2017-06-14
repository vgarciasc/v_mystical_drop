using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class Grid : MonoBehaviour {
	public int grid_ID = -1;
	public PlayerOnGrid player;

	public static int columns = 7;
	public static int rows = 9;

	[SerializeField]
	Transform tileContainer;

	[SerializeField]
	List<List<Tile>> tiles = new List<List<Tile>>();

	#region initialization
	void Start () {
		Init_Tiles();

		if (Is_Local_Grid()) {
			StartCoroutine(Spawn_Lines());
		}
	}

	IEnumerator Spawn_Lines() {
		float delay = 2f;
		int time = 0;

		while (true) {
			yield return new WaitForSeconds(delay);
			yield return new WaitUntil(() => player != null);
			yield return new WaitUntil(() => !player.is_having_animation);

			time++;

			Request_Spawn_Line_Of_Balls();
		}
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Z) && Is_Local_Grid()) {
			Request_Spawn_Line_Of_Balls();
		}
	}

	bool Is_Local_Grid() {
		return grid_ID == PlayerOnGrid.Get_Local_Player_ID();
	}

	void Init_Tiles() {
		for (int i = 0; i < rows; i++) {
			tiles.Add(new List<Tile>());
			
			for (int j = 0; j < columns; j++) {
				tiles[i].Add(tileContainer.GetChild(i * columns + j).GetComponentInChildren<Tile>());
				tiles[i][j].grid = this;
				tiles[i][j].name = "Tile #" + (i * columns + j);
				tiles[i][j].tile_ID = (i * columns + j);
			}
		}
	}
	#endregion

	#region getters
	public Tile Get_Player_Initial_Position() {
		return tiles[tiles.Count - 1][tiles[tiles.Count - 1].Count / 2];
	}

	public List<Tile> Get_First_Row() {
		return tiles[0];
	}

	public List<Tile> Get_Last_Row() {
		return tiles[tiles.Count - 1];
	}

	public Tile Get_Tile_by_ID(int tile_ID) {
		int row = tile_ID / columns;
		int column = tile_ID % columns;

		//Debug.Log("tile_ID: " + tile_ID);
		//Debug.Log("tiles[row][column] ~ tiles[" + row + "][" + column + "]: " + tiles[row][column]);

		return tiles[row][column];
	}

	public Tile Get_Tile_Down(Tile tile) {
		foreach (List<Tile> list_tile in tiles) {
			if (list_tile.Contains(tile) && list_tile != Get_Last_Row()) {
				return tiles[tiles.IndexOf(list_tile) + 1][list_tile.IndexOf(tile)];
			}
		}

		Debug.Log("This should not be happening. Game is over.");

		return null;
	}

	public Tile Get_Tile_Up(Tile tile) {
		int row = tile.tile_ID / columns;
		int column = tile.tile_ID % columns;

		if (row > 0) {
			return tiles[row - 1][column];
		}

		return null;
	}

	public Tile Get_Ball_Nearest_Player(PlayerOnGrid player) {
		for (int i = rows - 1; i >= 0; i--) {
			Tile aux = tiles[i][player.current_column];
			if (aux.hasBall) {
				return aux;
			}
		}

		Debug.Log("No balls.");
		return null;
	}
	#endregion

	public void Request_Spawn_Line_Of_Balls() {
		BallColor last_color = BallColor.NONE;

		int[] ball_colors = new int[columns];
		for (int i = 0; i < columns; i++) {
			last_color = Get_Possible_Color(last_color, Get_Tile_Down(Get_First_Row()[i]));
			ball_colors[i] = (int) last_color;
		}

		player.Cmd_Spawn_Line_Of_Balls(grid_ID, ball_colors);
	}

	public BallColor Get_Possible_Color(BallColor last_color, Tile down) {
		BallColor output = BallColor.NONE;
		List<BallColor> banned = new List<BallColor>() {last_color};

		if (down.hasBall) {
			banned.Add(down.ballColor);
		}

		do {
			output = (BallColor) Random.Range(0, System.Enum.GetValues(typeof(BallColor)).Length - 1);
		} while (output == BallColor.NONE || banned.Contains(output));

		return output;
	}

	public IEnumerator Spawn_Line_Of_Balls(int[] colors) {
		Coroutine move_down_animation = null;

		for (int i = rows - 1; i >= 0; i--) {
			for (int j = columns - 1; j >= 0; j--) {
				move_down_animation = StartCoroutine(tiles[i][j].Move_Down());
			}
		}

		yield return move_down_animation;

		int k = 0;
		foreach (Tile tile in Get_First_Row()) {
			tile.Activate_Ball((BallColor) colors[k++]);
		}
	}

	#region commands

	public Tile Get_Player_New_Tile(PlayerOnGrid player, Command cmd) {
		Tile aux = Get_Last_Row()[player.current_column];

		switch (cmd) {
		case Command.MOVE_LEFT:
			if (player.current_column > 0) {
				aux = Get_Last_Row()[player.current_column - 1];
			}
			break;

		case Command.MOVE_RIGHT:
			if (player.current_column < Get_Last_Row().Count - 1) {
				aux = Get_Last_Row()[player.current_column + 1];
			}
			break;
		}

		return aux;
	}

	//starting from the bottom
	public List<Tile> Get_Vacant_Tiles_In_Column(int column) {
		List<Tile> output = new List<Tile>();
		Tile aux;

		for (int i = rows - 2; i >= 1; i--) {
			aux = tiles[i][column];

			if (!aux.hasBall) {
				output.Add(aux);
			}
		}

		if (output.Count == 0) {
			Debug.Log("This should not be happening. Column is full!");
		}

		return output;
	}

	public Tile Is_There_Match_On_Board() {
		foreach (List<Tile> list in tiles) {
			foreach (Tile tile in list) {
				if (Get_Adjacent_Same_Color(tile).Count > 2) {
//					Debug.Log(tile);
//					Debug.Break();
					return tile;
				}
			}
		}

		return null;
	}

	public IEnumerator Sort_Board() {
		Tile can_match = null;

		while ((can_match = Is_There_Match_On_Board()) != null) {
			List<Tile> aux = Get_Adjacent_Same_Color(can_match);
			yield return Disappear_Balls(aux);
			yield return Update_Board();
		}
	}

	public IEnumerator Disappear_Balls(List<Tile> balls) {
		Coroutine ball_disappearing_animation = null;

		foreach (Tile tl in balls) {
			ball_disappearing_animation = StartCoroutine(tl.Disappear());
		}

		if (ball_disappearing_animation != null) {
			yield return ball_disappearing_animation;
		}

		yield break;
	}

	public IEnumerator Update_Board() {
		List<Tile> marked = new List<Tile>();

		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < columns; j++) {
				Tile aux = tiles[i][j];

				if (aux.hasBall) {
					continue;
				}

				Tile below = Get_Ball_Below(aux);
				if (below != null) {
					below.Move_To(aux.tile_ID);
				}
			}
		}

		//hardcoded. its ugly i know. its the time for the balls to move to their positions
		yield return new WaitForSeconds(0.2f);
	}

	Tile Get_Ball_Below(Tile tile) {
		int row = tile.tile_ID / columns;
		int column = tile.tile_ID % columns;

		for (int i = row; i < rows - 1; i++) {
			Tile aux = tiles[i][column];
			if (aux.hasBall) {
				return aux;
			}
		}

		//Debug.Log("No ball below " + tile + ".");
		return null;
	}

	List<Tile> Get_Adjacent_Same_Color(Tile tile) {
		List<Tile> marked = new List<Tile>() { tile };
		BallColor color = tile.ballColor;
		Tile next_tile;
		
		int k = 0;

		while (true) {
			next_tile = marked[k];

			foreach (Tile tl in Get_Adjacent_Tiles(next_tile)) {
				//Debug.Log("<color=gray>Testing " + tl + " (added by " + next_tile + ")</color>");
				if (tl.ballColor == color &&
					tl.hasBall &&
					!marked.Contains(tl)) {
					//Debug.Log("<color=green>Added: " + tl + " by " + next_tile + ".</color>\n" + tl + " is " + tl.ballColor + "\n" + next_tile + " is " + next_tile.ballColor);
					marked.Add(tl);
				}
			}

			k++;
			if (k >= marked.Count) {
				break;
			}
		}

		return marked;
	}

	List<Tile> Get_Adjacent_Tiles(Tile tile) {
		List<Tile> aux = new List<Tile>();
		int i = tile.tile_ID / columns; //linha
		int j = tile.tile_ID % columns; //coluna

		//Debug.Log("<b>rows: </b>" + rows);
		//Debug.Log("<b>columns: </b>" + columns);

		if (i > 0) {
			aux.Add(tiles[i - 1][j]);
		}
		if (j > 0) {
			aux.Add(tiles[i][j - 1]);
		}
		if (i < rows - 1) {
			aux.Add(tiles[i + 1][j]);
		}
		if (j < columns - 1) {
			aux.Add(tiles[i][j + 1]);
		}

		//string s = "<b>Adjacents to Tile #" + tile.tile_ID + "</b>\n";
		//foreach (Tile tl in aux) {
		//	s += "Tile #" + tl.tile_ID + ", ";
		//}
		//Debug.Log(s);

		return aux;
	}

	#endregion
}
