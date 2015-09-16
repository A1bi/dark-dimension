using UnityEngine;
using System.Collections;

public class CheckpointAsteroidController : MonoBehaviour {
	public float _movingSpeedFactor = 0.5f;
	public float _movingRange = 50.0f;

	private Vector3 _initialPosition;
	private float _stepAhead;

	void Start () {
		_initialPosition = transform.localPosition;
		_movingSpeedFactor *= Random.Range(1, 3);
		_stepAhead = Random.Range(5, 30);
	}

	void Update () {
		float distance = Mathf.SmoothStep(-_movingRange/2, _movingRange/2, Mathf.PingPong ((Time.time + _stepAhead) * _movingSpeedFactor, 1));
		transform.localPosition = new Vector3(_initialPosition.x + distance, _initialPosition.y - distance, _initialPosition.z);
	}
}
