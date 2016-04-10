using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class MouseLook {

	[HideInInspector] public bool rotatePlayerX = false;
	[SerializeField] Transform character;
	[SerializeField] Camera cam;
	[SerializeField] float XSensitivity = 3f;
	[SerializeField] float YSensitivity = 3f;
	[SerializeField] float MinimumX = -90F;
	[SerializeField] float MaximumX = 90F;
	Quaternion cameraTargetRot;


	public void Init ()
	{
		cameraTargetRot = cam.transform.localRotation;
	}


	public void Update () 
	{
		float yRot = Input.GetAxis("Mouse X") * XSensitivity;
		float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

		if (rotatePlayerX)
			character.Rotate(new Vector3(-xRot, yRot, 0), Space.Self);
		else
			character.Rotate(new Vector3(0, yRot, 0), Space.Self);
		
		cameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f);
		cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);
		cam.transform.localRotation = cameraTargetRot;
	}


	Quaternion ClampRotationAroundXAxis(Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;

		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

		angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

		q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

		return q;
	}

}
