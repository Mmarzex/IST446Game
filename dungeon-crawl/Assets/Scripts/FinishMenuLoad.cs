using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FinishMenuLoad : MonoBehaviour {
	public Text scoreText;
	// Use this for initialization
	void Start () {
		scoreText = GameObject.Find("Score Text").GetComponent<Text>();
		scoreText.text = "Score: " + PlayerPrefs.GetInt("score");
	}
}
