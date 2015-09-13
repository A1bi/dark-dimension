using UnityEngine;
using System.Collections;

public class DestroyOutOfView : MonoBehaviour {
	private GameObject _camera;

	void Start () {
		_camera = GameObject.FindGameObjectWithTag ("MainCamera");
	}

	void LateUpdate () {
		if (transform.position.z < _camera.transform.position.z) {
			Destroy(gameObject);
		}
	}
}
