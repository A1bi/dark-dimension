using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckpointConroller : MonoBehaviour {
	public GameObject _asteroid;
	public int _numberOfAsteroids = 6;
	public int _numberOfAsteroidsDeviation = 2;

	private List<GameObject> _asteroids;

	void Start() {
		_asteroids = new List<GameObject> ();

		transform.localScale = new Vector3 (transform.localScale.x, transform.localScale.y, -transform.localScale.z);

		int numberOfAsteroids = Random.Range (_numberOfAsteroids - _numberOfAsteroidsDeviation, _numberOfAsteroids + _numberOfAsteroidsDeviation);
		numberOfAsteroids = Mathf.Max (numberOfAsteroids, 3);

		// float circleAngle = (360.0f / _numberOfAsteroidsPerCheckpoint) * Mathf.PI / 180;
		float circleAngle = 2.0f / numberOfAsteroids * Mathf.PI;
		
		for (int i = 0; i < numberOfAsteroids; i++) {
			float currentAngle = circleAngle * i;
			Vector3 position = new Vector3 ();
			position.x = Mathf.Cos(currentAngle);
			position.y = Mathf.Sin(currentAngle);

			GameObject asteroid = (GameObject)Instantiate(_asteroid);
			asteroid.transform.parent = transform;
			asteroid.transform.localPosition = position;
			asteroid.transform.localScale = new Vector3(asteroid.transform.localScale.x, asteroid.transform.localScale.y, asteroid.transform.localScale.y);
			_asteroids.Add (asteroid);
		}
	}

	void Update () {
		List<GameObject> tmpAsteroids = new List<GameObject> (_asteroids);
		foreach (GameObject asteroid in tmpAsteroids) {
			if (asteroid == null) {
				_asteroids.Remove(asteroid);
			}
		}
		if (_asteroids.Count < 1) {
			Destroy(gameObject);
			return;
		}

		Vector3[] vertices = new Vector3[_asteroids.Count + 1];
		vertices[0] = new Vector3(0, 0, 0);
		int[] triangles = new int[_asteroids.Count * 3];
		
		for (int i = 0; i < _asteroids.Count; i++) {
			vertices[i+1] = _asteroids[i].transform.localPosition;
			triangles[i*3] = 0;
			triangles[i*3+1] = i+1;
			triangles[i*3+2] = ((i+1) % _asteroids.Count) + 1;
		}
		
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		GetComponent<MeshFilter> ().mesh = mesh;
		GetComponent<MeshCollider> ().sharedMesh = mesh;

		Vector3 position = transform.position;
		position.z -= 500 * Time.deltaTime;
		transform.position = position;
	}
}
