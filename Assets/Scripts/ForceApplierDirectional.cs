using UnityEngine;
using System.Collections;

public class ForceApplierDirectional : ForceApplierBase {

	[SerializeField] Vector3 direction = new Vector3(0, -1, 0);

	void Awake()
	{
		direction.Normalize();
	}

	public override void ApplyTo(GameObject obj)
	{
		ForceApplierBase.ApplyAcceleration(obj, direction * acceleration);
	}

}
