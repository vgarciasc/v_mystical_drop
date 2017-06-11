using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
	void OnStartLocalPlayer() {
		this.GetComponent<SpriteRenderer>().color = Color.grey;
	}

	void Update() {
		if (!isLocalPlayer)
			return;

		Command cmd = Command.NONE;

		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			cmd = Command.MOVE_LEFT;
		}

		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			cmd = Command.MOVE_RIGHT;
		}

		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			cmd = Command.MOVE_UP;
		}

		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			cmd = Command.MOVE_DOWN;
		}

		ReceiveCommand(cmd);
	}

	void ReceiveCommand(Command cmd) {
		float length = 0.25f;

		switch (cmd) {
			case Command.MOVE_LEFT:
				this.transform.position -= new Vector3(length, 0);
				break;

			case Command.MOVE_RIGHT:
				this.transform.position += new Vector3(length, 0);
				break;

			case Command.MOVE_DOWN:
				this.transform.position -= new Vector3(0, length);
				break;

			case Command.MOVE_UP:
				this.transform.position += new Vector3(0, length);
				break;
		}
	}
}
