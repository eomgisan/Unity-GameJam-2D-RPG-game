using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

public class CameraController : MonoBehaviour {
	
	public Transform target;
	float trackingSpeed = 2.0f;
	void Start () {

		if (target == null) {
			target = GameObject.Find ("Character").transform;
		}
	}

	
	void Update () {
		Vector3 position = target.position;
		position.y = target.position.y;
		position.z = -15;
		position.x = target.position.x + 1;
		position.y = target.position.y;
		transform.position = Vector3.Lerp (transform.position, position, trackingSpeed * 3 * Time.deltaTime);
	}
}
