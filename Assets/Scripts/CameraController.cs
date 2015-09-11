using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public GameObject _player;
	public float _damping = 15.0f;
	
	private Vector3 _distance;
		
	void Start() {
		_distance = transform.position - _player.transform.position;
	}
	
	void LateUpdate() {
		Vector3 targetPosition = _player.transform.position + _distance;
		Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _damping);
		newPosition.z = targetPosition.z;
		transform.position = newPosition;
	}
}
