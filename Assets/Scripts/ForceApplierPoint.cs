using UnityEngine;
using System.Collections;

public class ForceApplierPoint : ForceApplierBase {

	public override void ApplyTo(GameObject obj)
	{
		Vector3 direction = this.transform.position - obj.transform.position;
		ForceApplierBase.ApplyForce(obj, direction * force);
	}

	public static void ApplyTo(GameObject obj, Vector3 point, float force)
	{
		Vector3 direction = point - obj.transform.position;
		ForceApplierBase.ApplyForce(obj, direction * force);
	}

}
