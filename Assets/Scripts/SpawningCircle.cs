using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class SpawningCircle : NetworkBehaviour {
    public int accumulated_lines = 0;

	[SerializeField]
    Image sprite;
	[SerializeField]
	Grid grid_ally;
    [SerializeField]
    Grid grid_enemy;

    public bool can_spawn = false;

    void Start() {
        grid_enemy.match_push_event += Add_Line;
        grid_ally.spawn_lines_event += Spawn;
        //StartCoroutine(Spawn());
    }

    public void Spawn() {
        Cmd_Spawn();
    }

    [Command]
    public void Cmd_Spawn() {
        Rpc_Spawn();
    }

    [ClientRpc]
    public void Rpc_Spawn() {
        if (accumulated_lines > 0) {
            sprite.transform.DOScale(this.transform.localScale / 1.5f, 0.2f);
            accumulated_lines--;
        }

        if (accumulated_lines == 0) {
            sprite.DOColor(Color.white, 0.2f);
        }

    }

    public void Add_Line() {
        Cmd_Add_Line();
    }

    [Command]
    public void Cmd_Add_Line() {
        Rpc_Add_Line();
    }

    [ClientRpc]
	public void Rpc_Add_Line() {
        accumulated_lines++;

        BallColor color = (BallColor) Random.Range(0, System.Enum.GetNames(typeof(BallColor)).Length - 1);
		sprite.DOColor(Tile.Get_Ball_Color(color), 0.2f);
		sprite.transform.DOScale(this.transform.localScale * 1.5f, 0.2f);

        //StartCoroutine(Spawn());
	}

	public void Reset_Spawner() {
		sprite.transform.DOScale(Vector3.one, 1f);
	}
}