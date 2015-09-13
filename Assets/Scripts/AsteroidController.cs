using UnityEngine;
using System.Collections;

public class AsteroidController : MonoBehaviour {
	public float _rotationTumble;
	public float _speed;

	void Start () {
		Rigidbody rigidbody = GetComponent<Rigidbody> ();
		rigidbody.angularVelocity = Random.insideUnitSphere * _rotationTumble;
		rigidbody.velocity = transform.forward * -_speed;
	}
}
