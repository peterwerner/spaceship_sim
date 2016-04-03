using UnityEngine;
using System.Collections;

public abstract class ForceApplierBase : MonoBehaviour {

	[SerializeField] protected float force = 9.81f;

	public abstract void ApplyTo(GameObject obj);

	protected static bool ApplyForce(GameObject obj, Vector3 forceVector)
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
