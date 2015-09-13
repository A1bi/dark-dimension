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
	[Header("Spawning")]
	public float _spawnDistance;
	public float _horizontalSpawnRange;
	public float _verticalSpawnRange;
	[Header("Cameras")]
	public GameObject[] _cameras;

	private float _lastAsteroidZ;
	private float _lastPlanetZ;
	private int _currentCameraIndex = -1;

	void Start () {
		switchCamera ();
	}

	void Update () {
		if (Input.GetButtonUp ("Camera")) {
			switchCamera ();
		}
	}

	void LateUpdate () {
		SpawnAsteroid ();
		SpawnPlanet ();
	}

	void SpawnAsteroid () {
		if (!ShouldSpawnNewObject (_lastAsteroidZ, _asteroidDistance, _asteroidDistanceDeviation)) {
			return;
		}

		GameObject asteroid = GetRandomObjectFromArray(_asteroids);

		Vector3 playerPos = _player.transform.position;
		float posX = Random.Range(playerPos.x - _horizontalSpawnRange, playerPos.x + _horizontalSpawnRange);
		float posY = Random.Range(playerPos.y - _verticalSpawnRange, playerPos.y + _verticalSpawnRange);
		float posZ = playerPos.z + _spawnDistance;
		Vector3 position = new Vector3(posX, posY, posZ);
		GameObject newAsteroid = (GameObject)Instantiate(asteroid, position, Quaternion.identity);

		float size = Random.Range(1, _maxAsteroidSize);
		newAsteroid.transform.localScale = new Vector3(size, size, size);

		_lastAsteroidZ = playerPos.z;
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

		_lastPlanetZ = playerPos.z;
	}

	GameObject GetRandomObjectFromArray (GameObject[] array) {
		return array[Random.Range(0, array.Length)];
	}

	bool ShouldSpawnNewObject(float lastZ, float distance, float deviation) {
		float difference = _player.transform.position.z - lastZ;
		return difference >= Random.Range(distance - deviation, distance + deviation);
	}

	void switchCamera () {
		_currentCameraIndex = ++_currentCameraIndex % _cameras.Length;
		for (int i = 0; i < _cameras.Length; i++) {
			_cameras[i].SetActive(i == _currentCameraIndex);
		}
	}

	public void PlayerHitAsteroid () {
		if (_currentCameraIndex > 0) {
			switchCamera ();
		}
	}
}
