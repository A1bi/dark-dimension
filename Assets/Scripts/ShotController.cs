using UnityEngine;
using System.Collections;

public class ShotController : MonoBehaviour {
	public float _speedFactor;

	private Rigidbody _rigidbody;
	private GameObject _player;

	void Start () {
		_player = GameObject.FindGameObjectWithTag ("Player");
		GetComponent<Rigidbody> ().velocity = _player.GetComponent<Rigidbody>().velocity * _speedFactor;
	}
}
