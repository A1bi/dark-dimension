using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("Speed")]
	public int _normalSpeed = 1000;
	public float _boostFactor = 4;
	public float _boostTransitionDuration = 1.0f;
	[Header("Rotation")]
	public float _rotationSpeed = 150;
	public int _maxRoll = 70;
	public int _maxPitch = 50;
	public int _maxYaw = 30;
	public float _yawRatio = 0.3f;
	public bool _yInverted = true;
	[Header("Fire")]
	public GameObject _shot;
	public Transform _shotSpawner;
	public float _shotsPerSecond;
	[Header("Misc")]
	public GameObject _explosion;

	private Rigidbody _rigidbody;
	private GameController _gameController;
	private float _currentSpeed;
	private float _previousSpeed = -1;
	private float _targetSpeed;
	private float _speedEasingStep = 1f;
	private Func<float, float> _speedEasing;
	private float _nextShot;

	void Start () {
		_rigidbody = GetComponent<Rigidbody> ();
		_gameController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController> ();
	}

	void Update () {
		if (Input.GetButton ("Fire1") && Time.time > _nextShot) {
			_nextShot = Time.time + 1 / _shotsPerSecond;
			Instantiate(_shot, _shotSpawner.position, _shotSpawner.rotation);
		}
	}

	void FixedUpdate () {
		updateSpeed();
		updateRotation();
	}

	void OnTriggerEnter (Collider other) {
		if (other.tag == "Asteroid") {
			Destroy (other);
			Instantiate (_explosion, transform.position, transform.rotation);
			gameObject.SetActive (false);
			_gameController.PlayerHitAsteroid ();

		} else if (other.tag == "Checkpoint") {
			_gameController.PlayerHitCheckpoint ();
		}
	}

	void updateSpeed() {
		if (Input.GetButtonDown("Boost")) {
			float boostSpeed = _normalSpeed * _boostFactor;
			changeSpeed(boostSpeed);
			_speedEasing = easeIn;
			
		} else if (Input.GetButtonUp("Boost") || _previousSpeed < 0) {
			changeSpeed(_normalSpeed);
			_speedEasing = easeOut;
		}
		
		if (Input.GetButtonDown("Boost") || Input.GetButtonUp("Boost")) {
			_speedEasingStep = 0f;
		}
		
		_speedEasingStep += Time.deltaTime;
		if (_speedEasingStep > _boostTransitionDuration) {
			_speedEasingStep = _boostTransitionDuration;
		}
		
		float t = _speedEasingStep / _boostTransitionDuration;
		_currentSpeed = Mathf.Lerp(_previousSpeed, _targetSpeed, _speedEasing(t));

		Vector3 addPosition = _rigidbody.rotation * Vector3.forward;
		_rigidbody.velocity = addPosition * _currentSpeed * Time.deltaTime;
	}

	void changeSpeed(float targetSpeed) {
		_previousSpeed = _currentSpeed;
		_targetSpeed = targetSpeed;
		_speedEasingStep = 0f;
	}

	float easeIn(float t) {
		return t * t;
	}

	float easeOut(float t) {
		return t * (2 - t);
	}

	void updateRotation() {
		float roll = Input.GetAxis("Horizontal") * Time.deltaTime * _rotationSpeed;
		float pitch = Input.GetAxis("Vertical") * Time.deltaTime * _rotationSpeed;
		if (!_yInverted) {
			pitch *= -1;
		}
		Quaternion addRotation = Quaternion.Euler(new Vector3(pitch, 0, -roll));
		
		Quaternion rotation = _rigidbody.rotation * addRotation;
		Vector3 angles = rotation.eulerAngles;
		angles.z = getConstrainedAngle(angles.z, _maxRoll);
		angles.x = getConstrainedAngle(angles.x, _maxYaw);
		angles.y = getNormalizedAngle(angles.z) / _maxRoll * -_maxPitch;
		if (angles.z > 180) {
			angles.y = 360 - angles.y;
		}
		_rigidbody.rotation = Quaternion.Lerp(Quaternion.Euler(angles), Quaternion.identity, (_currentSpeed - _normalSpeed) / (_normalSpeed * (_boostFactor - 1)));
	}

	float getConstrainedAngle(float angle, float constraint) {
		float normalized = getNormalizedAngle(angle);
		if (normalized > constraint) {
			normalized = constraint;
		}
		if (angle > 180) {
			normalized = 360 - normalized;
		}
		return normalized;
	}

	float getNormalizedAngle(float angle) {
		return (angle > 180) ? (360 - angle) : angle;
	}
}