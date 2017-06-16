using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine;
using DG.Tweening;

public class Clock : MonoBehaviour {
	[SerializeField]
	Image circle;
	[SerializeField]
	Image circleOutline;
	[SerializeField]
	GameObject clockPrefab;

	[SerializeField]
	List<SpawningCircle> spawners = new List<SpawningCircle>();

	public float fill = 0f;

	float cycle_duration_in_seconds = 5f;
	bool full = false;

	public bool stop_the_clock = false;

	public delegate void VoidDelegate();
	public event VoidDelegate Gong_Event;

	void Update() {
        if (stop_the_clock || GameObject.FindGameObjectsWithTag("Player").Length < 2) {
            return;
        }

        fill += (Time.deltaTime) / cycle_duration_in_seconds;

		circleOutline.fillAmount = fill;
		circle.fillAmount = fill;
		fill = Mathf.Clamp(fill, 0f, 1f);

		full = (fill >= 1f);
		if (full) {
			StartCoroutine(Gong());
		}
	}

	IEnumerator Gong() {
		Reset_Fill();

		GameObject aux = Instantiate(clockPrefab, this.transform.parent, false);
        aux.transform.position = this.transform.position;
        aux.transform.DOScale(3 * Vector3.one, 0.4f);
		aux.GetComponent<Image>().DOColor(Color.clear, 0.4f);

		yield return new WaitForSeconds(0.4f);

		Destroy(aux);

		//foreach (SpawningCircle spawner in spawners) {
            //spawner.StartCoroutine(Spawn());
		//}

		if (Gong_Event != null) {
			Gong_Event();
		}
	}

	void Reset_Fill() {
		fill = 0f;
	}
}
