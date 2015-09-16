using UnityEngine;
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
	[Header("Spawning")]
	public float _spawnDistance;
	public float _horizontalSpawnRange;
	public float _verticalSpawnRange;
	[Header("General")]
	public GameObject[] _cameras;
	public float _goalDistance;

	private float _lastAsteroidZ;
	private float _lastPlanetZ;
	private float _lastCheckpointZ;
	private int _currentCameraIndex = -1;
	private float _checkpointDistance;

	void Start () {
		_checkpointDistance = _goalDistance / _numberOfCheckpoints;

		SwitchCamera ();
	}

	void Update () {
		if (Input.GetButtonUp ("Camera")) {
			SwitchCamera ();
		}
	}

	void LateUpdate () {
		SpawnAsteroid ();
		SpawnPlanet ();
		SpawnCheckpoint ();
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

		int leftOrRight = (Random.value > 0.5) ? 1 : -1;
		float posX = playerPos.x + _horizontalSpawnRange * leftOrRight;
		float posY = Random.Range(playerPos.y - _verticalSpawnRange, playerPos.y + _verticalSpawnRange);
		float posZ = playerPos.z + _spawnDistance;
		Vector3 position = new Vector3(posX, posY, posZ);
		GameObject newPlanet = (GameObject)Instantiate(planet, position, Quaternion.identity);

		float size = Random.Range(1, _maxPlanetSize);
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

	GameObject GetRandomObjectFromArray (GameObject[] array) {
		return array[Random.Range(0, array.Length)];
	}

	Vector3 GetPositionWithinSpawnRange() {
		Vector3 playerPos = _player.transform.position;
		float posX = Random.Range(playerPos.x - _horizontalSpawnRange, playerPos.x + _horizontalSpawnRange);
		float posY = Random.Range(playerPos.y - _verticalSpawnRange, playerPos.y + _verticalSpawnRange);
		float posZ = playerPos.z + _spawnDistance;
		return new Vector3(posX, posY, posZ);
	}

	bool ShouldSpawnNewObject(float lastZ, float distance, float deviation) {
		float difference = _player.transform.position.z - lastZ;
		return difference >= Random.Range(distance - deviation, distance + deviation);
	}

	void SwitchCamera () {
		_currentCameraIndex = ++_currentCameraIndex % _cameras.Length;
		for (int i = 0; i < _cameras.Length; i++) {
			_cameras[i].SetActive(i == _currentCameraIndex);
		}
	}

	public void PlayerHitAsteroid () {
		if (_currentCameraIndex > 0) {
			SwitchCamera ();
		}
	}

	public void PlayerHitCheckpoint () {
		print ("Checkpoint");
	}
}
