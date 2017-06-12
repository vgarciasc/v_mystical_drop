using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class Grid : MonoBehaviour {
	public int grid_ID = -1;
	public PlayerOnGrid player;

	int columns = 9;
	int rows = 7;

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
		for (int i = 0; i < columns; i++) {
			tiles.Add(new List<Tile>());
			
			for (int j = 0; j < rows; j++) {
				tiles[i].Add(tileContainer.GetChild(i * rows + j).GetComponentInChildren<Tile>());
				tiles[i][j].grid = this;
				tiles[i][j].name = "Tile #" + (i * rows + j);
				tiles[i][j].tile_ID = (i * rows + j);
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
		int row = tile_ID / rows;
		int column = tile_ID % rows;

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
		for (int i = columns - 1; i >= 0; i--) {
			for (int j = rows - 1; j >= 0; j--) {
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

	#endregion
}
