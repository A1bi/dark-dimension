using UnityEngine;
using System.Collections;

public class ShotController : MonoBehaviour {
	public float _speedFactor;
	public bool _autoTarget = false;
	
	private GameObject _player;
	private GameObject _target;

	void Start () {
		_player = GameObject.FindGameObjectWithTag ("Player");

		if (_autoTarget) {
			FindTarget ();
		}

		if (!_autoTarget || !_target) {
			_autoTarget = false;
		} else {
			_speedFactor *= 2;
		}
	}

	void FixedUpdate () {
		if (_autoTarget) {
			if (_target == null) {
				_autoTarget = false;

			} else {
				transform.LookAt(_target.transform);
			}
		}

		transform.Translate(Vector3.forward * _speedFactor);
	}

	void FindTarget () {
		float distance = Mathf.Infinity;
		GameObject[] targets = GameObject.FindGameObjectsWithTag("Asteroid");

		foreach (GameObject target in targets) {
			if (target.transform.position.z > _player.transform.position.z + 1500) {

				float diff = (target.transform.position - _player.transform.position).sqrMagnitude;
				if (diff < distance) {
					distance = diff;
					_target = target;
				}

			}
		}
	}
}
