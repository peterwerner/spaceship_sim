using UnityEngine;
using System.Collections;

[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (CapsuleCollider))]
public class MoveControl : MonoBehaviour {

	public enum MoveMode { WALK, JETPACK };

	[HideInInspector] public Vector3 gravity = Vector3.zero, _gravity = Vector3.zero;
	[SerializeField] Camera cam;
	[SerializeField] MouseLook mouseLook;
	[Tooltip ("When gravity is below this value, enter jetpack mode")]
	[SerializeField] float gravityThreshold = 0.1f;
	new Rigidbody rigidbody;
	MoveMode moveMode = MoveMode.WALK;


	void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		mouseLook.Init();
		Cursor.lockState = CursorLockMode.Locked;
	}


	void FixedUpdate()
	{
		_gravity = gravity;
		if (gravity.magnitude > gravityThreshold)
			moveMode = MoveMode.WALK;
		else
			moveMode = MoveMode.JETPACK;
		gravity = Vector3.zero;
	}


	void Update()
	{
		if (moveMode == MoveMode.WALK) {
			// Orient such that 'up' is opposite to the direction of gravity
			mouseLook.rotatePlayerX = false;
			transform.rotation = Quaternion.FromToRotation(transform.up, -1 * _gravity.normalized) * transform.rotation;
		}
		else if (moveMode == MoveMode.JETPACK) {
			// Orient to rotate with the camera
			mouseLook.rotatePlayerX = true;
		}
		mouseLook.Update();
	}

}
