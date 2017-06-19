using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NetworkHUD : MonoBehaviour {
    public NetworkManager nm;
    public Text input;

    public void StartHost() {
        SetPort();
        NetworkManager.singleton.StartHost();
    }

    public void StartClient() {
        SetIPAddress();
        SetPort();
        NetworkManager.singleton.StartClient();
    }

    void SetPort() {
        NetworkManager.singleton.networkPort = 7777;
    }

    void SetIPAddress() {
        string ipAddress = input.text;
        NetworkManager.singleton.networkAddress = ipAddress;
    }

    public void GoToLobby() {
        nm.ServerChangeScene(nm.onlineScene);
    }
}