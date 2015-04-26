using UnityEngine;
using System.Collections;

public class CameraDebug : MonoBehaviour {

	private Camera camera;

	// Use this for initialization
	void Start () {
		camera = this.GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		//this.transform.position += new Vector3(horizontal, vertical, 0);

		if (Input.GetKey(KeyCode.Z)) {
			if (camera.orthographicSize > 1)
				camera.orthographicSize -= 1;
		} else if (Input.GetKey(KeyCode.X)) {
			camera.orthographicSize += 1;
		}
	}
}
