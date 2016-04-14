using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(NpcController))]
public class NpcBrain : MonoBehaviour {

	public Pathfinding.PathManagerRuntime pathManager;
	public Transform moveTarget;
	List<Vector3> path  = new List<Vector3>();


	void Update () { UpdatePath(); }


	public void UpdatePath ()
	{
		path = pathManager.GetShortestPath(this.transform.position, moveTarget.position);
	}


	void OnDrawGizmos ()
	{
		Gizmos.color = Color.black;
		Vector3 prev = transform.position;
		foreach (Vector3 curr in path) {
			Gizmos.DrawLine(curr, prev);
			prev = curr;
		}
	}

}
