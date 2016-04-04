﻿using UnityEngine;
using System.Collections;

public abstract class ForceApplierBase : MonoBehaviour {

	[SerializeField] protected float acceleration = 9.81f;

	public abstract void ApplyTo(GameObject obj);


	public static bool ApplyAcceleration(GameObject obj, Vector3 accelVector)
	{
		if (obj == null)
			return false;
		Rigidbody rb = obj.GetComponent<Rigidbody>();
		if (rb) {
			rb.AddForce(accelVector * rb.mass);
		}
		return true;
	}

	public static bool ApplyForce(GameObject obj, Vector3 forceVector)
	{
		if (obj == null)
			return false;
		Rigidbody rb = obj.GetComponent<Rigidbody>();
		if (rb) {
			rb.AddForce(forceVector);
		}
		return true;
	}

}
