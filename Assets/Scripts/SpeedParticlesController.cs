using UnityEngine;

public class SpeedParticlesController : MonoBehaviour {
	private ParticleEmitter _emitter;
	private float _lastZ;

	void Start () {
		_emitter = GetComponent<ParticleEmitter>();
	}

	void Update () {
		float speed = (transform.position.z - _lastZ) / Time.deltaTime;
		_lastZ = transform.position.z;

		_emitter.emit = speed > 10;

		Vector3 velo = _emitter.localVelocity;
		velo.z = -speed / 8.0f;
		_emitter.localVelocity = velo;
	}
}
