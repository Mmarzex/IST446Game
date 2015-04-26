using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTriggerEnter2D(Collider2D other) {
		Debug.Log("Door colliding with " + other.name);
		if (other.tag == "player_2") {
			Debug.Log ("player at door");

			var mpcontroller = GameObject.Find ("MPController");
			GameObject.DontDestroyOnLoad(mpcontroller);

			var mp = mpcontroller.GetComponent<MultiplayerController>();
			mp.takeTurn();

			Application.LoadLevel("Finish");
		}
	}
}
