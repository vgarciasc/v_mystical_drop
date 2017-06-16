using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;

public class Server : NetworkBehaviour {
	void Start() {
		StartCoroutine(SpawnBalls());
	}

	void Update() {
	}

	IEnumerator SpawnBalls() {
		while (true) {
			yield return new WaitForSeconds(1.0f);
		}
	}
}