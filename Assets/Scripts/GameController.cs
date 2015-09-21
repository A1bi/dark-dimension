using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour {
	public GameObject _player;
	[Header("Asteroids")]
	public GameObject[] _asteroids;
	public float _asteroidDistance;
	public float _asteroidDistanceDeviation;
	public float _maxAsteroidSize;
	[Header("Planets")]
	public GameObject[] _planets;
	public int _planetDistance;
	public int _planetDistanceDeviation;
	public float _maxPlanetSize;
	[Header("Checkpoints")]
	public GameObject _checkpointPrefab;
	public GameObject[] _checkpointAsteroids;
	public float _minCheckpointSize;
	public float _maxCheckpointSize;
	public int _numberOfCheckpoints;
	public AudioClip _checkpointSound;
	[Header("Spawning")]
	public float _spawnDistance;
	public float _horizontalSpawnRange;
	public float _verticalSpawnRange;
	[Header("General")]
	public GameObject[] _cameras;
	public GameObject _finish;
	public float _finishDistance;
	public AudioClip _finishSound;
	[Header("UI")]
	public Text _timeText;
	public GameObject _mainMenu;
	public GameObject _helpMenu;
	public GameObject _inGameMenu;
	public GameObject _crosshairs;
	public GameObject _pauseMenu;
	public GameObject _endMenu;
	public AudioClip _startSound;
	[HideInInspector]
	public bool _inGame = false;

	private float _lastAsteroidZ;
	private float _lastPlanetZ;
	private float _lastCheckpointZ;
	private int _currentCameraIndex = -1;
	private float _checkpointDistance;
	private bool _finishSpawned = false;
	private float _time;

	void Start () {
		_checkpointDistance = _finishDistance / _numberOfCheckpoints;

		GameObject[] menus = { _mainMenu, _helpMenu, _inGameMenu, _pauseMenu, _endMenu };
		foreach (GameObject menu in menus) {
			menu.SetActive (true);
		}

		foreach (GameObject ui in GameObject.FindGameObjectsWithTag("Mobile")) {
			ui.SetActive (SystemInfo.supportsAccelerometer);
		}
		foreach (GameObject ui in GameObject.FindGameObjectsWithTag("Desktop")) {
			ui.SetActive (!SystemInfo.supportsAccelerometer);
		}

		SwitchCamera ();
		ToggleMenu (_mainMenu, true);
	}

	void Update () {
		if (_inGame) {
			if (Input.GetButtonUp ("Camera")) {
				SwitchCamera ();
			
			} else if (Input.GetButtonUp ("Cancel")) {
				TogglePauseMenu ();
			}

			_time += Time.deltaTime;
			_timeText.text = GetTimeString ();

		} else {
			if (Input.GetButtonUp ("Cancel")) {
				TogglePauseMenu (false);
			}
		}
	}

	void LateUpdate () {
		if (_inGame) {
			SpawnAsteroid ();
			SpawnPlanet ();
			SpawnCheckpoint ();
			SpawnFinish ();
		}
	}

	public void StartGame () {
		_inGame = true;
		_time = 0;
		_lastPlanetZ = _lastAsteroidZ = _lastCheckpointZ = 0;
		_player.GetComponent<PlayerController> ().Reset ();

		ToggleMenu (_mainMenu, false);
		ToggleMenu (_inGameMenu, true);
		PlaySound (_startSound);
	}

	public void RestartGame () {
		string[] tags = { "Planet", "Asteroid", "Checkpoint" };
		foreach (string tag in tags) {
			GameObject[] objects = GameObject.FindGameObjectsWithTag (tag);
			foreach (GameObject obj in objects) {
				Destroy(obj);
			}
		}

		StartGame ();
		TogglePauseMenu (false);
		ToggleMenu (_endMenu, false);
	}

	void EndGame (bool success) {
		_inGame = false;

		if (_currentCameraIndex > 0) {
			SwitchCamera ();
		}

		ToggleMenu (_inGameMenu, false);
		ToggleMenu (_endMenu, true);

		GameObject timeText = _endMenu.transform.Find ("Time Text").gameObject;
		timeText.SetActive (success);
		if (success) {
			PlaySound (_finishSound);
			timeText.GetComponent<Text>().text = "Deine Zeit: " + GetTimeString();
		}

		GameObject titleText = _endMenu.transform.Find ("Title").gameObject;
		titleText.GetComponent<Text> ().text = success ? "Geschafft!" : "Game Over";
	}

	void SpawnAsteroid () {
		if (!ShouldSpawnNewObject (_lastAsteroidZ, _asteroidDistance, _asteroidDistanceDeviation)) {
			return;
		}

		GameObject asteroid = GetRandomObjectFromArray(_asteroids);
		Vector3 position = GetPositionWithinSpawnRange ();
		GameObject newAsteroid = (GameObject)Instantiate(asteroid, position, Quaternion.identity);
		
		float size = Random.Range(1, _maxAsteroidSize);
		newAsteroid.transform.localScale = new Vector3(size, size, size);

		_lastAsteroidZ = position.z;
	}

	void SpawnPlanet () {
		if (!ShouldSpawnNewObject (_lastPlanetZ, _planetDistance, _planetDistanceDeviation)) {
			return;
		}

		GameObject planet = GetRandomObjectFromArray(_planets);
		
		Vector3 playerPos = _player.transform.position;

		float size = Random.Range(1, _maxPlanetSize);

		int leftOrRight = (Random.value > 0.5) ? 1 : -1;
		float posX = playerPos.x + (_horizontalSpawnRange + size) * leftOrRight;
		float posY = Random.Range(playerPos.y - _verticalSpawnRange, playerPos.y + _verticalSpawnRange);
		float posZ = playerPos.z + _spawnDistance;
		Vector3 position = new Vector3(posX, posY, posZ);

		GameObject newPlanet = (GameObject)Instantiate(planet, position, Quaternion.identity);
		newPlanet.transform.localScale = new Vector3(size, size, size);

		_lastPlanetZ = posZ;
	}

	void SpawnCheckpoint () {
		if (!ShouldSpawnNewObject (_lastCheckpointZ, _checkpointDistance, 0)) {
			return;
		}

		GameObject asteroid = GetRandomObjectFromArray(_checkpointAsteroids);
		float asteroidSize = Random.Range(1, _maxAsteroidSize);
		asteroid.transform.localScale = new Vector3(asteroidSize, asteroidSize, asteroidSize);

		Vector3 checkpointCenter = GetPositionWithinSpawnRange ();
		float checkPointSize = Random.Range (_minCheckpointSize, _maxCheckpointSize);
		GameObject checkpoint = (GameObject)Instantiate (_checkpointPrefab, checkpointCenter, Quaternion.identity);
		checkpoint.transform.localScale = new Vector3(checkPointSize, checkPointSize, checkPointSize);
		checkpoint.GetComponent<CheckpointConroller> ()._asteroid = asteroid;

		_lastCheckpointZ = checkpointCenter.z;
	}

	void SpawnFinish () {
		if (_finishSpawned || !ShouldSpawnNewObject (0, _finishDistance, 0)) {
			return;
		}

		GameObject finish = (GameObject)Instantiate (_finish);
		finish.transform.position = new Vector3 (0, 0, _player.transform.position.z + _spawnDistance);
		_finishSpawned = true;
	}

	GameObject GetRandomObjectFromArray (GameObject[] array) {
		return array[Random.Range(0, array.Length)];
	}

	Vector3 GetPositionWithinSpawnRange () {
		Vector3 playerPos = _player.transform.position;
		float posX = Random.Range(playerPos.x - _horizontalSpawnRange, playerPos.x + _horizontalSpawnRange);
		float posY = Random.Range(playerPos.y - _verticalSpawnRange, playerPos.y + _verticalSpawnRange);
		float posZ = playerPos.z + _spawnDistance;
		return new Vector3(posX, posY, posZ);
	}

	bool ShouldSpawnNewObject (float lastZ, float distance, float deviation) {
		float difference = _player.transform.position.z + _spawnDistance - lastZ;
		return difference >= Random.Range(distance - deviation, distance + deviation);
	}

	void SwitchCamera () {
		_currentCameraIndex = ++_currentCameraIndex % _cameras.Length;
		for (int i = 0; i < _cameras.Length; i++) {
			_cameras[i].SetActive(i == _currentCameraIndex);
		}
		_crosshairs.SetActive (_currentCameraIndex == _cameras.Length - 1);
	}

	void ToggleMenu (GameObject menu, bool toggle) {
		menu.GetComponent<Animator> ().SetBool("visible", toggle);
	}

	void PlaySound (AudioClip clip) {
		AudioSource source = GetComponent<AudioSource> ();
		source.clip = clip;
		source.Play();
	}

	string GetTimeString () {
		System.TimeSpan time = System.TimeSpan.FromSeconds (_time);
		return System.String.Format ("{0:00}:{1:00}:{2:00}", time.Minutes, time.Seconds, time.Milliseconds / 10);
	}

	public void PlayerDied () {
		EndGame (false);
	}

	public void PlayerHitCheckpoint () {
		PlaySound (_checkpointSound);
	}

	public void PlayerHitFinish () {
		EndGame (true);
	}

	public void VolumeSettingChanged (float volume) {
		AudioListener.volume = volume;
	}

	public void TogglePauseMenu () {
		bool visible = _pauseMenu.GetComponent<Animator> ().GetBool ("visible");
		TogglePauseMenu (!visible);
	}

	public void TogglePauseMenu (bool toggle) {
		Time.timeScale = toggle ? 0 : 1;
		_inGame = !toggle;
		ToggleMenu (_pauseMenu, toggle);
		if (!toggle) {
			ToggleHelpMenu (false);
		}
	}

	public void ToggleHelpMenu (bool toggle) {
		ToggleMenu (_helpMenu, toggle);
	}

	public void Quit () {
		Application.Quit ();
	}
}
