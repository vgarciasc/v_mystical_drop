using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine;
using DG.Tweening;

public enum BallColor {
	RED,
	BLUE,
	GREEN,
	YELLLOW,
	NONE
}

public class Tile : NetworkBehaviour {
	public int tile_ID = -1;
	public Grid grid;

	[Header("Ball")]
	[SerializeField]
	Image ballSprite;
	[SerializeField]
	GameObject ballPrefab;

	[SyncVar]
	public BallColor ballColor;
	[SyncVar]
	public bool hasBall = false;

	void Start() {
		Deactivate_Ball();
	}

	void Update() {
		ballSprite.enabled = hasBall;
		ballSprite.color = Get_Ball_Color(ballColor);
	}

	public void Activate_Ball(BallColor color) {
		ballColor = color;
		hasBall = true;

		ballSprite.enabled = true;
		ballSprite.color = Get_Ball_Color(ballColor);
	}

	public void Deactivate_Ball() {
		hasBall = false;
		ballSprite.color = Color.white;
		ballSprite.enabled = false;
	}

	public void Move_Down() {
		if (hasBall) {
			Deactivate_Ball();
			Tile down = grid.Get_Tile_Down(this);

			if (down != null) {
				down.Activate_Ball(ballColor);
			}
		}
	}

	public void Move_Up() {
		if (hasBall) {
			Tile up = grid.Get_Tile_Up(this);

			if (up != null &&
				!up.hasBall) {
				Deactivate_Ball();
				up.Activate_Ball(ballColor);
			}
		}
	}

	public void Move_To(int tile_ID) {
		StartCoroutine(Move_To(grid.Get_Tile_by_ID(tile_ID)));

		Cmd_Move_To(tile_ID);
	}

	[Command]
	void Cmd_Move_To(int tile_ID) {
		Rpc_Move_To(tile_ID);
	}

	[ClientRpc]
	void Rpc_Move_To(int tile_ID) {
		StartCoroutine(Move_To(grid.Get_Tile_by_ID(tile_ID)));
	}

	public IEnumerator Move_To(Tile tile) {
		BallColor color = ballColor;
		GameObject ball = Instantiate_Ball_For_Anim();
		float delay = 0.1f * Mathf.Abs((tile_ID - tile.tile_ID) / 7);
		ball.transform.DOMove(tile.transform.position, delay);

		Deactivate_Ball();

		yield return new WaitForSeconds(delay);

		Destroy(ball);
		tile.Activate_Ball(color);
	}

	public static Color Get_Ball_Color(BallColor ball) {
		Color aux = Color.white;

		switch (ball) {
			case BallColor.YELLLOW:
				aux = Color.yellow;
				aux += new Color(0.3f, 0.3f, 0.3f);
				break;
			case BallColor.RED:
				aux = Color.red;
				aux += new Color(0f, 0.3f, 0.3f);
				break;
			case BallColor.BLUE:
				aux = Color.blue;
				aux += new Color(0.3f, 0.3f, 0f);
				break;
			case BallColor.GREEN:
				aux = Color.green;
				aux += new Color(0.3f, 0f, 0.3f);
				break;
		}

		return aux;
	}

	public GameObject Instantiate_Ball_For_Anim() {
		GameObject aux = Instantiate(ballPrefab, this.transform.parent.parent, false);
		aux.GetComponentInChildren<Image>().color = Get_Ball_Color(ballColor);
		aux.transform.position = this.transform.position;

		return aux;
	}
}
