using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class GameOverController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var gmObject = GameObject.Find("GameOverText");
		var winner = PlayerPrefs.GetString("winner");

		(gmObject.GetComponent<Text>()).text = winner;
	}
	
	// Update is called once per frame
	void Update () {
		var gmObject = GameObject.Find("GameOverText");
		var winner = PlayerPrefs.GetString("winner");
		Debug.Log ("Winner==> " + winner);
		(gmObject.GetComponent<Text>()).text = winner;
	}
}
