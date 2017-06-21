using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;

public class LobbyButton : MonoBehaviour {

	void Start () {
		Debug.Log("D");
		GameObject.FindGameObjectWithTag("BackToLobbyButton").GetComponentInChildren<Button>().onClick.AddListener(Back_to_Lobby);
	}

	void Back_to_Lobby() {
		Debug.Log("X");
		GameObject.FindGameObjectWithTag("Server").GetComponentInChildren<NetworkManager>().ServerChangeScene("Main");
	}
}
