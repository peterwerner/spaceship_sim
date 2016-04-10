using UnityEngine;
using System.Collections;

public class ForceApplierPoint : ForceApplierBase {

	[SerializeField] bool invert = false;

	public override void ApplyTo(GameObject obj)
	{
		Vector3 direction = GetDirection(obj);
		if (invert)
			direction *= -1;
		ForceApplierBase.ApplyAcceleration(obj, direction * acceleration);
	}


	public override Vector3 GetDirection(GameObject obj) { return (transform.position - obj.transform.position).normalized; }

}
