using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleJSON;

public class DeathMenuLoad : MonoBehaviour {

	public Text scoreText;

	// Use this for initialization
	void Start () {
		scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
		scoreText.text = "Score: " + PlayerPrefs.GetInt("score");

		StartCoroutine(UpdateRoom());

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Make request to server to update the room with current player's finished score
	// Need to store the player's unique id in PlayerPrefs.GetString("playerId")
	private IEnumerator UpdateRoom() {
		WWWForm form = new WWWForm();
		form.AddField("playerid", PlayerPrefs.GetString("playerId"));
		form.AddField("status", 1);
		form.AddField("score", PlayerPrefs.GetInt("score"));

		WWW request = new WWW("http://107.170.10.115:3000/rooms/" + PlayerPrefs.GetString("roomId"), form);
		Debug.Log("Sending request to http://107.170.10.115:3000/rooms/" + PlayerPrefs.GetString("roomId"));
		Debug.Log("PlayerId=" + PlayerPrefs.GetString("playerId"));
		Debug.Log("Score=" + PlayerPrefs.GetInt("score"));

		yield return request;

		if (request.error == null) {
			Debug.Log("Updated room successfuly");
			var room = JSONNode.Parse(request.text);
			var roomStatus = room["isFinished"].AsBool;
			Debug.Log("Room is finished: " + roomStatus);
		}
	}
}
