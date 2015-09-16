using UnityEngine;
using System.Collections;

public class AsteroidController : MonoBehaviour {
	public float _rotationTumble;
	public float _speed;
	public GameObject _explosion;

	void Start () {
		Rigidbody rigidbody = GetComponent<Rigidbody> ();
		rigidbody.angularVelocity = Random.insideUnitSphere * _rotationTumble;
		rigidbody.velocity = transform.forward * -_speed;
	}

	void OnTriggerEnter (Collider other) {
		if (other.tag == "Shot") {
			Instantiate(_explosion, transform.position, transform.rotation);
			_explosion.transform.localScale = transform.localScale;
			Destroy(gameObject);
			Destroy(other);
		}
	}
}
