using UnityEngine;
using System.Collections;

public class Treasure : MonoBehaviour {

	public MPlayer player;
	private float PICKED_UP_GOLD = 1000;


	// Use this for initialization
	void Start () {
		player = GameObject.FindWithTag("player_2").GetComponent<MPlayer>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D other) {
		//Debug.Log (other.name);
		if (other != null) {
			if(other.name =="Player(Clone)"){
				player.score += PICKED_UP_GOLD;
				Debug.Log("PICKED UP GOLD");
				Destroy (this.gameObject);
			}
		}
	}
}
