using UnityEngine;
using System.Collections;

public class DestroyOutOfView : MonoBehaviour {
	public float _maxDistance = 6000;

	private GameObject _camera;

	void Start () {
		_camera = GameObject.FindGameObjectWithTag ("MainCamera");
	}

	void LateUpdate () {
		float cameraZ = _camera.transform.position.z;
		if (transform.position.z < cameraZ || transform.position.z > (cameraZ + _maxDistance)) {
			Destroy(gameObject);
		}
	}
}
