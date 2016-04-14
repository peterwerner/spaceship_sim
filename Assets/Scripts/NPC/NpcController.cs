using UnityEngine;
using System.Collections;

[RequireComponent(typeof (AtmoFlowSim.FlowListener))]
public class NpcController : Affectable {

	public Vector3 lookTarget;
	[SerializeField] Animator animator;
	[SerializeField] float turnSpeed = 1;
	AtmoFlowSim.FlowListener flowListener;


	void Awake () 
	{
		flowListener = GetComponent<AtmoFlowSim.FlowListener>();
	}


	void Update ()
	{
		// Rotate such that 'up' is opposite to gravity
		if (flowListener.gravity.magnitude > float.Epsilon)
			transform.rotation = Quaternion.FromToRotation(transform.up, -1 * flowListener.gravity.normalized) * transform.rotation;
		// Rotate about our 'up' axis to face the look target
		Vector3 rightTarget = Vector3.Cross(lookTarget - transform.position, flowListener.gravity).normalized;
		transform.right = Vector3.MoveTowards(transform.right, rightTarget, turnSpeed * Time.deltaTime);
	}

}
