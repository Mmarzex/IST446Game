using UnityEngine;
using System.Collections;


public class Bullet : MonoBehaviour {

	public MPlayer player;
	private float KILLED_ENEMY = 100;

	// Use this for initialization
	void Start() {
		player = GameObject.FindWithTag("player_2").GetComponent<MPlayer>();
	}
	
	// Update is called once per frame
	void Update() {

	}


	void OnTriggerEnter2D(Collider2D other) {
		//Debug.Log (other.name);
		if (other != null) {
			if (other.name != "Background" && other.name != "Enemy(Clone)") {
				if (!other.name.Contains("boundary")) {
					//Debug.Log (this.tag + " colliding with " + other.tag);
				}
				Destroy(this.gameObject);

				if(other.name =="Player(Clone)"){
					player.TakeDamage();
				}

			}
			if (other.name == "Enemy(Clone)" && this.tag == "player_bullet") {
				player.incrementScore(KILLED_ENEMY);
				Debug.Log (player.score);
				Destroy (other.gameObject);
				Destroy (this.gameObject);
			}
		}
	}
}
