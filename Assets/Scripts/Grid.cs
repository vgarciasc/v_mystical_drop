using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class Grid : MonoBehaviour {
	public int grid_ID = -1;
	public PlayerOnGrid player;

	int columns = 7;
	int rows = 9;

	[SerializeField]
	Transform tileContainer;

	[SerializeField]
	List<List<Tile>> tiles = new List<List<Tile>>();

	#region initialization
	void Start () {
		Init_Tiles();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Z) && grid_ID == PlayerOnGrid.Get_Local_Player_ID()) {
			Request_Spawn_Line_Of_Balls();
		}
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
		int[] ball_colors = new int[columns];
		for (int i = 0; i < columns; i++) {
			ball_colors[i] = Random.Range(0, 2);
		}

		player.Cmd_Spawn_Line_Of_Balls(grid_ID, ball_colors);
	}

	public void Spawn_Line_Of_Balls(int[] colors) {
		for (int i = rows - 1; i >= 0; i--) {
			for (int j = columns - 1; j >= 0; j--) {
				tiles[i][j].Move_Down();
			}
		}

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

	Tile Get_First_Vacant_Tile_In_Column(int column) {
		Tile aux;

		for (int i = 0; i < rows; i++) {
			aux = tiles[i][column];

			if (!aux.hasBall) {
				return aux;
			}
		}

		Debug.Log("This should not be happening.");
		return null;
	}

	public void Insert_Ball(int column, int quantity, BallColor color) {
		if (color == BallColor.NONE) {
			return;
		}

		Tile tile = null;
		for (int i = 0; i < quantity; i++) {
			tile = Get_First_Vacant_Tile_In_Column(column);
			if (tile != null) {
				tile.Activate_Ball(color);
			}
		}
		
		if (tile != null) {
			StartCoroutine(Check_For_Match(tile.tile_ID));
		}
	}

	IEnumerator Check_For_Match(int tile_ID) {
		foreach (Tile tl in Get_Adjacent_Same_Color(Get_Tile_by_ID(tile_ID))) {
			//tl.Deactivate_Ball();
			tl.ballColor = BallColor.NONE;
		}

		yield break;
		//yield return new WaitForSeconds(1.0f);
	}

	List<Tile> Get_Adjacent_Same_Color(Tile tile) {
		List<Tile> marked = new List<Tile>() { tile };
		Debug.Log("Tile Color: " + tile.ballColor);
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
