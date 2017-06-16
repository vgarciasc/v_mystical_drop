using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class SpawningCircle : MonoBehaviour {
	public int accumulated_lines = 0;
	Image sprite;

	[SerializeField]
	Grid grid;

	Coroutine spawning = null;

	public void Spawn_() {
		if (spawning != null) {
			StopCoroutine(spawning);
		}

//		spawning = StartCoroutine(Spawn());
	}

	public IEnumerator Spawn() {
		for (int i = 0; i < accumulated_lines; i++) {
			grid.Request_Spawn_Line_Of_Balls();

			sprite.transform.DOScale(this.transform.localScale / 1.25f, 1f);
			yield return new WaitForSeconds(0.5f);
		}

		spawning = null;
	}

	public void Add_Line() {
		BallColor color = (BallColor) Random.Range(0, System.Enum.GetNames(typeof(BallColor)).Length - 1);
		sprite.color = Tile.Get_Ball_Color(color);
		sprite.transform.DOScale(this.transform.localScale * 1.25f, 1f);

		accumulated_lines++;
	}

	public void Reset_Spawner() {
		sprite.transform.DOScale(Vector3.one, 1f);
	}
}