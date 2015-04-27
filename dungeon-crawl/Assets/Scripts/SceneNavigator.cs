using UnityEngine;
using System.Collections;

public class SceneNavigator : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void onMenuClick() {
		Application.LoadLevel("Menu");
	}

	public void onCreditsClick() {
		Application.LoadLevel("Credits");
	}

	public void onInstructionsClick() {
		Application.LoadLevel("Instructions");
	}
}
