using UnityEngine;
using System.Collections;

public class ForceApplierDirectional : ForceApplierBase {

	[SerializeField] Vector3 direction = new Vector3(0, -1, 0);


	public override void ApplyTo(GameObject obj)
	{
		ForceApplierBase.ApplyAcceleration(obj, GetDirection(obj) * acceleration);
	}


	public override Vector3 GetDirection(GameObject obj) { return direction.normalized; }

}
