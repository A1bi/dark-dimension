using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	enum Weapon { Normal, AutoTarget };

	[Header("Speed")]
	public int _normalSpeed = 1000;
	public float _boostFactor = 4;
	public float _boostTransitionDuration = 1.0f;
	public float _initialBoostAmount = 50.0f;
	public float _checkpointBoostIncrease = 30.0f;
	public float _boostDecrease = 0.3f;
	public float _boostIncrease = 0.05f;
	public AudioClip _boostSound;
	[Header("Rotation")]
	public float _rotationSpeed = 150;
	public int _maxRoll = 70;
	public int _maxPitch = 50;
	public int _maxYaw = 30;
	public float _yawRatio = 0.3f;
	public bool _yInverted = true;
	[Header("Shooting")]
	public GameObject _shot;
	public Transform _shotSpawner;
	public float _shotsPerSecond;
	public AudioClip[] _shotSounds;
	[Header("General")]
	public GameObject _explosion;
	public GameObject _turbine;
	public int _asteroidDamage = 25;
	public int _planetDamage = 100;
	public AudioClip _damageSound;
	[Header("UI")]
	public Slider _boostSlider;
	public Slider _healthSlider;
	public GameObject _weaponText;

	private Rigidbody _rigidbody;
	private GameController _gameController;
	private float _boost;
	private float _health;
	private bool _boostDemanded;
	private float _currentSpeed;
	private float _previousSpeed = -1;
	private float _targetSpeed;
	private float _speedEasingStep = 1f;
	private System.Func<float, float> _speedEasing;
	private float _nextShot;
	private bool _shootingDemanded;
	private Weapon _currentWeapon = Weapon.AutoTarget;
	private Vector3 _initialPosition;
	private Quaternion _initialRotation;
	private Vector3 _initialVelocity;

	void Start () {
		_rigidbody = GetComponent<Rigidbody> ();
		_gameController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController> ();
		_initialPosition = transform.position;
		_initialRotation = transform.rotation;
		_initialVelocity = _rigidbody.angularVelocity;

		SwitchWeapon ();
	}

	void Update () {
		if (!_gameController._inGame)
			return;

		if ((_shootingDemanded || Input.GetButton ("Fire1")) && Time.time > _nextShot) {
			Shoot ();
		}

		if (Input.GetButtonUp ("Weapon")) {
			SwitchWeapon ();
		}

		UpdateSlider (_boostSlider, _boost);
		UpdateSlider (_healthSlider, _health);
	}

	void FixedUpdate () {
		if (!_gameController._inGame)
			return;

		UpdateSpeed ();
		UpdateRotation ();
		UpdateTurbine ();
	}

	void OnTriggerEnter (Collider other) {
		if (other.tag == "Asteroid") {
			TakeDamage (_asteroidDamage);

		} else if (other.tag == "Planet") {
			TakeDamage (_planetDamage);

		} else if (other.tag == "Checkpoint") {
			ChangeBoost (_checkpointBoostIncrease);
			_gameController.PlayerHitCheckpoint ();
		
		} else if (other.tag == "Finish") {
			_gameController.PlayerHitFinish ();
		}
	}

	public void Reset() {
		gameObject.SetActive (true);
		_boost = _initialBoostAmount;
		_boostDemanded = false;
		_currentSpeed = _targetSpeed = _normalSpeed;
		_health = 100;
		transform.position = _initialPosition;
		transform.rotation = _initialRotation;
		_rigidbody.angularVelocity = _initialVelocity;
	}

	public void DemandBoost (bool toggle) {
		_boostDemanded = toggle;
	}

	public void DemandShooting (bool toggle) {
		_shootingDemanded = toggle;
	}

	void Shoot () {
		_nextShot = Time.time + 1 / _shotsPerSecond;
		GameObject shot = (GameObject)Instantiate(_shot, _shotSpawner.position, _shotSpawner.rotation);

		shot.GetComponent<ShotController> ()._autoTarget = _currentWeapon == Weapon.AutoTarget;
		
		AudioClip clip = _shotSounds[Random.Range(0, _shotSounds.Length)];
		PlaySound(clip);
	}

	void UpdateSpeed() {
		bool boostDemanded = _boostDemanded || Input.GetButton("Boost");
		bool boosting = _targetSpeed > _normalSpeed;
		bool boostStarting = !boosting && boostDemanded;
		bool boostEnding = (boosting && !boostDemanded) || _boost <= 0;

		if (boosting) {
			ChangeBoost (-_boostDecrease);
		} else {
			ChangeBoost (_boostIncrease);
		}

		if (boostStarting && _boost > 15) {
			float boostSpeed = _normalSpeed * _boostFactor;
			changeSpeed (boostSpeed);
			_speedEasing = easeIn;
			PlaySound (_boostSound);
		}

		if (boostEnding || _previousSpeed < 0) {
			changeSpeed(_normalSpeed);
			_speedEasing = easeOut;
		}
		
		if (boostEnding) {
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

	void UpdateRotation() {
		float hAxis, vAxis;
		if (SystemInfo.supportsAccelerometer) {
			hAxis = Mathf.Max (Mathf.Min (Input.acceleration.x * 3.0f, 1), -1) * _rotationSpeed * 0.6f;
			vAxis = Mathf.Max (Mathf.Min (Input.acceleration.y / 2.0f + 0.35f, 1), -1)  * -_rotationSpeed * 1.2f;
		} else {
			hAxis = Input.GetAxis("Horizontal") * _rotationSpeed;
			vAxis = Input.GetAxis("Vertical") * _rotationSpeed;
		}

		float roll = hAxis * Time.deltaTime;
		float pitch = vAxis * Time.deltaTime;
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

	void UpdateTurbine () {
		ParticleEmitter emitter = _turbine.GetComponent<ParticleEmitter> ();
		float factor = _currentSpeed / _normalSpeed;
		emitter.minEmission = 50 * factor;
		emitter.maxEmission = 150 * factor;
	}

	void TakeDamage(float amount) {
		_health -= amount;

		if (_gameController._inGame && _health <= 0) {
			Instantiate (_explosion, transform.position, transform.rotation);
			gameObject.SetActive (false);
			_gameController.PlayerDied ();
		} else {
			PlaySound(_damageSound);
		}
	}

	void ChangeBoost(float amount) {
		_boost += amount;
	}

	public void SwitchWeapon () {
		_currentWeapon = (Weapon)((int)++_currentWeapon % System.Enum.GetNames(typeof(Weapon)).Length);
		
		string name = "";
		switch (_currentWeapon) {
		case Weapon.Normal:
			name = "Normalschuss";
			break;
		case Weapon.AutoTarget:
			name = "Zielsuchend";
			break;
		}
		
		_weaponText.GetComponent<Text>().text = name;
	}

	void UpdateSlider(Slider slider, float value) {
		slider.value = Mathf.Lerp (slider.value, value, Time.deltaTime * 3.0f);
	}

	void PlaySound (AudioClip clip) {
		AudioSource source = GetComponent<AudioSource> ();
		source.clip = clip;
		source.Play();
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