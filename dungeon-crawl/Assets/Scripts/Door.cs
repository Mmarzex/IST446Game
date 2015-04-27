using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {
	private float score;
	public MPlayer Player;
	// Use this for initialization
	void Start()
	{
		Player = MPlayer.FindObjectOfType<MPlayer>(); 
	}
	

	public void OnTriggerEnter2D(Collider2D other) {
		Debug.Log("Door colliding with " + other.name);
		if (other.tag == "player_2") {
			Debug.Log ("player at door");
			score = Player.score;
			PlayerPrefs.SetInt("score", (int)score);
			var mpcontroller = GameObject.Find ("MPController");
			GameObject.DontDestroyOnLoad(mpcontroller);

			var mp = mpcontroller.GetComponent<MultiplayerController>();
			mp.takeTurn();

			Application.LoadLevel("Finish");
		}
	}
}
