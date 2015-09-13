using UnityEngine;
using System.Collections;

public class DestroyTime : MonoBehaviour {
	public float _time;

	void Start () {
		Destroy (gameObject, _time);
	}
}
