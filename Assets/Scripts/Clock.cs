using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine;
using DG.Tweening;

public class Clock : NetworkBehaviour {
	[SerializeField]
	Image circle;
	[SerializeField]
	GameObject clockPrefab;

	[SerializeField]
	List<SpawningCircle> spawners = new List<SpawningCircle>();

	[SyncVar]
	public float fill = 0f;

	float cycle_duration_in_seconds = 5f;
	bool full = false;

	void Update() {
		fill += (Time.deltaTime) / cycle_duration_in_seconds;

		circle.fillAmount = fill;
		fill = Mathf.Clamp(fill, 0f, 1f);

		full = (fill >= 1f);
		if (full) {
			Gong();
		}
	}

	void Gong() {
		Reset_Fill();

		GameObject aux = Instantiate(clockPrefab, this.transform.parent, false);
		aux.transform.DOScale(3 * Vector3.one, 0.4f);
		aux.GetComponent<Image>().DOColor(Color.clear, 0.4f);

		foreach (SpawningCircle spawner in spawners) {
			spawner.Spawn_();
		}
	}

	void Reset_Fill() {
		fill = 0f;
	}
}
