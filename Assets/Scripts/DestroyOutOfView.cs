using UnityEngine;
using System.Collections;

public class DestroyOutOfView : MonoBehaviour {
	private GameObject _camera;

	void Start () {
		_camera = GameObject.FindGameObjectWithTag ("MainCamera");
	}

	void LateUpdate () {
		float cameraZ = _camera.transform.position.z;
		if (transform.position.z < cameraZ) {
			Destroy(gameObject);
		}
	}
}
