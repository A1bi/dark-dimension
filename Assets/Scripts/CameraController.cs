using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public GameObject _player;
	public float _damping = 15.0f;
	public bool _firstPerson = false;
	
	private Vector3 _distance;
	private GameController _gameController; 
		
	void Awake () {
		_distance = transform.position - _player.transform.position;
		_gameController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController> ();
	}
	
//	fixed rotation camera
	void LateUpdate () {
		if (!_gameController._inGame) {
			return;
		}

		Vector3 targetPosition = _player.transform.position + _distance;

		if (_firstPerson) {
			transform.position = targetPosition;
			transform.rotation = _player.transform.rotation;
		} else {
			Vector3 position = Vector3.Lerp (transform.position, targetPosition, Time.deltaTime * _damping);
			position.z = targetPosition.z;
			transform.position = position;
			transform.LookAt(_player.transform.position);
		}
	}

//	following angle camera
//	void LateUpdate () {
//		float currentAngle = transform.eulerAngles.y;
//		float newAngle = _player.transform.eulerAngles.y;
//		float angle = Mathf.LerpAngle(currentAngle, newAngle, Time.deltaTime * _damping);
//		
//		Quaternion rotation = Quaternion.Euler(0, angle, 0);
//		transform.position = _player.transform.position - (rotation * -_distance);
//		
//		transform.LookAt(_player.transform);
//	}
}
