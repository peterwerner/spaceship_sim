using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (CapsuleCollider))]
public class MoveControl : MonoBehaviour {

	public enum MoveMode { WALK, JETPACK };

	[Serializable]
	public class MovementSettings {
		public float walkSpeed = 4;
		public float jetpackSpeed = 4;
		public float jumpSpeed = 40;
		public float sprintSpeedMultiplier = 1.8f;
		public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
		public float dragWhileGrounded = 10f;
		[Tooltip ("When gravity is below this value, enter jetpack mode")]
		public float gravityThreshold = 0.1f;
	}

	[Serializable]
	public class AdvancedSettings {
		public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
		[Tooltip("set it to 0.1 or more if you get stuck in wall")]
		public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)	
	}


	[HideInInspector] public Vector3 gravity = Vector3.zero;
	[SerializeField] Camera cam;
	[SerializeField] MouseLook mouseLook;
	[SerializeField] MovementSettings movementSettings;
	[SerializeField] AdvancedSettings advancedSettings;
	new Rigidbody rigidbody;
	new CapsuleCollider collider;
	MoveMode moveMode = MoveMode.WALK;
	bool jump = false, isGrounded = false, wasGrounded = false;
	Vector3 groundContactNormal;



	void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		rigidbody.drag = 0;
		collider = GetComponent<CapsuleCollider>();
		mouseLook.Init();
		Cursor.lockState = CursorLockMode.Locked;
	}


	void Update()
	{
		if (gravity.magnitude > movementSettings.gravityThreshold)
			moveMode = MoveMode.WALK;
		else
			moveMode = MoveMode.JETPACK;

		if (moveMode == MoveMode.WALK) 
		{
			// Orient such that 'up' is opposite to the direction of gravity
			mouseLook.rotatePlayerX = false;
			transform.rotation = Quaternion.FromToRotation(transform.up, -1 * gravity.normalized) * transform.rotation;
		}

		else if (moveMode == MoveMode.JETPACK) 
		{
			// Orient to rotate with the camera
			mouseLook.rotatePlayerX = true;
		}

		if (Input.GetButtonDown("Jump"))
			jump = true;
		mouseLook.Update();
		gravity = Vector3.zero;
	}


	void FixedUpdate()
	{
		GroundCheck();
		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
		if (Input.GetButton("Sprint") && Mathf.Abs(Vector3.Angle(input, Vector3.forward)) <= 90)
			input *= movementSettings.sprintSpeedMultiplier;

		if (moveMode == MoveMode.WALK) 
		{
			Vector3 v = input * movementSettings.walkSpeed * SlopeMultiplier();
			v = transform.rotation * v;
			if (isGrounded) {
				rigidbody.drag = movementSettings.dragWhileGrounded;
				if (rigidbody.velocity.magnitude < v.magnitude)
					rigidbody.AddForce(v, ForceMode.Impulse);
				if (jump) {
					rigidbody.drag = 0;
					rigidbody.AddForce(transform.up * movementSettings.jumpSpeed, ForceMode.Impulse);
					jump = false;
				}
				else if (input.magnitude < float.Epsilon && rigidbody.velocity.magnitude < 1)
					rigidbody.Sleep();
			}
			else
				rigidbody.drag = 0;
		}

		else if (moveMode == MoveMode.JETPACK) 
		{
			rigidbody.drag = 0;
			input *= movementSettings.jetpackSpeed;
			input = cam.transform.rotation * input;
			rigidbody.AddForce(input, ForceMode.Acceleration);
		}
	}



	private float SlopeMultiplier()
	{
		float angle = Vector3.Angle(groundContactNormal, transform.up);
		return movementSettings.SlopeCurveModifier.Evaluate(angle);
	}


	/// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
	private void GroundCheck()
	{
		wasGrounded = isGrounded;
		RaycastHit hitInfo;
		if (Physics.SphereCast(transform.position, collider.radius * (1.0f - advancedSettings.shellOffset), -1 * transform.up, out hitInfo,
			((collider.height/2f) - collider.radius) + advancedSettings.groundCheckDistance, ~0, QueryTriggerInteraction.Ignore))
		{
			isGrounded = true;
			groundContactNormal = hitInfo.normal;
		}
		else
		{
			isGrounded = false;
			groundContactNormal = transform.up;
		}
		/*
		if (!wasGroudned && isGrounded && jumping)
		{
			jumping = false;
		}
		*/
	}

}
