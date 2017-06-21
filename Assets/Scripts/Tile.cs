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
	YELLOW,
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

	public bool is_moving = false;

	[SyncVar]
	public BallColor ballColor;
	[SyncVar]
	public bool hasBall = false;
    [SyncVar]
    bool is_disappearing = false;

    void Start() {
		Cmd_Deactivate_Ball();
	}

	void Update() {
		ballSprite.enabled = hasBall;
		ballSprite.color = Get_Ball_Color(ballColor);
        if (!is_disappearing) {
            ballSprite.transform.localScale = Vector3.one;
        }
	}

    [Command]
    public void Cmd_Activate_Ball(BallColor color) {
		if (hasBall) {
            //this was already active. what are you trying to do?
            //Debug.Log("A1");
            //Debug.Break();
            Tile down = this;
			do {
				down = grid.Get_Tile_Down(down);
			} while (down.hasBall);
            if (down == null) {
                down = grid.Get_Tile_Up(down);
            }
            down.Cmd_Activate_Ball(color);
           
			return;
		}

		ballSprite.transform.localScale = Vector3.one;
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

    [Command]
	public void Cmd_Deactivate_Ball() {
        Deactivate_Ball();
    }

	public IEnumerator Push_Animation(Tile target, BallColor color) {
		GameObject ball = Instantiate_Ball_For_Anim(color);
		float delay = 0.1f;

		var tween = ball.transform.DOMove(target.transform.position, delay);
		tween.SetEase(Ease.InCirc);

		yield return new WaitForSeconds(delay);

		Destroy(ball);
		target.Cmd_Activate_Ball(color);

		yield break;
	}

	public IEnumerator Move_Down() {
		if (hasBall) {
			Cmd_Deactivate_Ball();
			Tile down = grid.Get_Tile_Down(this);

			if (down != null) {
				down.Cmd_Activate_Ball(ballColor);
			}
		}

		yield break;

//		if (hasBall) {
//			Tile down = grid.Get_Tile_Down(this);
//
//			if (down != null) {
//				yield return StartCoroutine(Move_To(down, true));
//			}
//		}
	}

	public void Move_To(int tile_ID) {
        //		StartCoroutine(Move_To(grid.Get_Tile_by_ID(tile_ID), true));
        //Debug.Log("C0");
		Cmd_Move_To(tile_ID);
	}

	[Command]
	void Cmd_Move_To(int tile_ID) {
		Rpc_Move_To(tile_ID);
	}

	[ClientRpc]
	void Rpc_Move_To(int tile_ID) {
		StartCoroutine(Move_To(grid.Get_Tile_by_ID(tile_ID), true));
	}

	public IEnumerator Move_To(Tile tile, bool use_delay_distance) {
		BallColor color = ballColor;
		Cmd_Deactivate_Ball();
//		Debug.Log("X");
//		Debug.Break();
		GameObject ball = Instantiate_Ball_For_Anim();

		float delay = 0.1f;
//		if (use_delay_distance) {
//			delay *= Mathf.Abs((tile_ID - tile.tile_ID) / Grid.columns);
//		}

		var tween = ball.transform.DOMove(tile.transform.position, delay);
		tween.SetEase(Ease.InCirc);

		yield return new WaitForSeconds(delay);

		Destroy(ball);
        //Debug.Log("A0");
		tile.Cmd_Activate_Ball(color);
	}

	public static Color Get_Ball_Color(BallColor ball) {
		Color aux = Color.white;

		switch (ball) {
			case BallColor.YELLOW:
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

	public GameObject Instantiate_Ball_For_Anim(BallColor color) {
		GameObject aux = Instantiate(ballPrefab, this.transform.parent.parent, false);
		aux.GetComponentInChildren<Image>().color = Get_Ball_Color(color);
		aux.transform.position = this.transform.position;

		return aux;
	}
    
    [Command]
	public void Cmd_Disappear() {
        //StartCoroutine(Disappear());
        Rpc_Disappear();
    }

    [ClientRpc]
    public void Rpc_Disappear() {
        StartCoroutine(Disappear());
    }

    IEnumerator Disappear() {
        is_disappearing = true;
        float delay = 0.1f;

        Vector3 originalScale = ballSprite.transform.localScale;
        ballSprite.transform.DOScale(Vector3.zero, delay);

        yield return new WaitForSeconds(delay);

        ballSprite.transform.localScale = originalScale;

        Deactivate_Ball();
        is_disappearing = false;
    }
}
