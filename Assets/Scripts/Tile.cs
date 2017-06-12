using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine;

public enum BallColor {
	GRAY,
	GREEN,
	NONE
}

public class Tile : NetworkBehaviour {
	public int tile_ID = -1;
	public Grid grid;

	[Header("Ball")]
	[SerializeField]
	Image ballSprite;

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

	public static Color Get_Ball_Color(BallColor ball) {
		Color aux = Color.white;

		switch (ball) {
			case BallColor.GRAY:
				aux = Color.gray;
				break;
			case BallColor.GREEN:
				aux = Color.green;
				break;
		}

		return aux;
	}
}
