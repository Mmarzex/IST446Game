using UnityEngine;
using System.Collections;
using SimpleJSON;

public class Level : MonoBehaviour {

	public GameObject wall;
	public GameObject ground;
	public GameObject player;
	public GameObject enemy;
	public GameObject door;
	public GameObject treasure;
	public GameObject areaLight;

	private string rawMapData;
	private Vector3 playerSpawn;
	private Vector3 exitPosition;
	private Vector3 treasurePosition;

	private int mapWidth;
	private int mapHeight;

	// Use this for initialization
	void Start () {
		StartCoroutine(GetMap());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private IEnumerator GetMap() {
		// Use POST request for testing
		WWWForm form = new WWWForm();

		// Generate random players for testing
//		string playerid = Mathf.Round(Random.value * 1000).ToString();
//		form.AddField ("player_1", playerid);
//		form.AddField ("player_2", Mathf.Round(Random.value * 1000).ToString());

		// For actual game, ids are in pOneID and pTwoID PlayerPrefs;
		string playeroneid = PlayerPrefs.GetString("pOneID");
		string playertwoid = PlayerPrefs.GetString("pTwoID");

		form.AddField("player_1", playeroneid);
		form.AddField("player_2", playertwoid);

		// temporarily use player_1's id as stored playerid
//		PlayerPrefs.SetString("playerId", playerid);

		WWW www = new WWW("http://107.170.10.115:3000/rooms", form);
		// Wait for request to finish
		yield return www;
		if (www.error == null) {
			Debug.Log ("Request ok");
			Debug.Log (www.text);
			rawMapData = www.text;
			ParseMapData();
		} else {
			Debug.Log ("Request error" + www.error);
		}
	}

	private void ParseMapData() {
		
		Debug.Log ("Parsing map");
		var tileWidth = wall.GetComponent<SpriteRenderer>().bounds.size.x;
		var tileHeight = wall.GetComponent<SpriteRenderer>().bounds.size.y;
		var room = JSONNode.Parse (rawMapData);
		mapWidth = room["map"]["properties"]["width"].AsInt;
		mapHeight = room["map"]["properties"]["width"].AsInt;
		Debug.Log ("Map Width:" + mapWidth);
		Debug.Log ("Map Height:" + mapHeight);
		Debug.Log ("Tile size: " + tileWidth + "x" + tileHeight);
		
		var roomId = room["id"].ToString().Replace ("\"", "");
		PlayerPrefs.SetString("roomId", roomId);
		PlayerPrefs.Save();
		Debug.Log("Room id: " + PlayerPrefs.GetString ("roomId"));

		// Parse terrain/tiles
		for (int x = 0; x < mapWidth; x++) {
			for (int y = 0; y < mapHeight; y++) {
				int type = room["map"]["terrain"][x][y].AsInt;
				if (type == 0) {
					Instantiate (ground, new Vector3(y, mapWidth-x, 0), Quaternion.identity);
				} else if (type == 1) {
					Instantiate (wall, new Vector3(y, mapWidth-x, 0), Quaternion.identity);
				}
			}
		}

		playerSpawn = new Vector3(room["map"]["playerSpawn"]["y"].AsFloat, mapWidth - room["map"]["playerSpawn"]["x"].AsFloat, -2);
		Debug.Log (playerSpawn);
		Instantiate(player, playerSpawn, Quaternion.identity);

		foreach (JSONNode enemyobj in room["map"]["enemies"].AsArray) {
			Debug.Log ("Render enemy at: " + enemyobj["position"]["y"].AsFloat + "," + enemyobj["position"]["x"].AsFloat);
			Instantiate (enemy, new Vector3(enemyobj["position"]["y"].AsFloat, mapWidth - enemyobj["position"]["x"].AsFloat, -1), Quaternion.identity);
		}

		exitPosition = new Vector3(room["map"]["exitPosition"]["y"].AsFloat, mapWidth - room["map"]["exitPosition"]["x"].AsFloat, -1);
		Instantiate(door, exitPosition, Quaternion.identity);
		GameObject exitLightObj = Instantiate (areaLight, new Vector3(exitPosition.x, exitPosition.y, -2), Quaternion.identity) as GameObject;
		Light exitLight = exitLightObj.GetComponent<Light>();
		exitLight.color = new Color(0, 1, 1, 1);
		exitLight.intensity = 1f;
		exitLight.range = 10;

		treasurePosition = new Vector3(room["map"]["treasurePosition"]["y"].AsFloat, mapWidth - room["map"]["treasurePosition"]["x"].AsFloat, -1);
		Instantiate (treasure, treasurePosition, Quaternion.identity);
		GameObject treasureLightObj = Instantiate (areaLight, new Vector3(treasurePosition.x, treasurePosition.y, -2), Quaternion.identity) as GameObject;
		Light light = treasureLightObj.GetComponent<Light>();
		light.color = Color.yellow;
		light.intensity = 1f;
		light.range = 10;

		CreateMapBoundary();
	}

	// Create a wall around entire map to ensure the entire map is closed off
	private void CreateMapBoundary() {
		// Left border
		float x = -1;
		for (int y = 1; y <= mapHeight; y++) {
			Instantiate (wall, new Vector3(x, y, 0), Quaternion.identity);
		}

		// Top border
		int y1 = mapHeight + 1;
		for (x = -1; x <= mapWidth; x++) {
			Instantiate (wall, new Vector3(x, y1, 0), Quaternion.identity);
		}

		// Right border
		int x1 = mapWidth;
		for (int y = 1; y <= mapHeight; y++) {
			Instantiate (wall, new Vector3(x1, y, 0), Quaternion.identity);
		}

		// Bottom border
		y1 = 0;
		for (x = -1; x <= mapWidth; x++) {
			Instantiate (wall, new Vector3(x, y1, 0), Quaternion.identity);
		}
	}
}
