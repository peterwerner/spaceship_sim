using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(BoxCollider))]
public class FlowConnector : MonoBehaviour {

	public bool isOpen = false;
	bool wasOpen = false;
	float flowCheap = 0;
	FlowRoom roomA = null, roomB = null;
	BoxCollider boxCollider;
	FlowVoxel[ , ] pairs;	// Pairs of: roomA voxel, roomB voxel


	void Update()
	{
		if (wasOpen && !isOpen)
			Close();
		else if (!wasOpen && isOpen)
			Open();
		wasOpen = isOpen;
	}
		
	void Start()
	{
		// Connect to the rooms that this is currently touching. This should connect to either 1 or 2 rooms.
		boxCollider = (BoxCollider)GetComponent(typeof(BoxCollider));
		foreach (FlowRoom room in FlowRoom.InstanceList) {
			if (room.Collider.bounds.Intersects(boxCollider.bounds)) {
				if (!roomA)
					roomA = room;
				else if (!roomB)
					roomB = room;
				else
					print("Error: connector at " + this.transform.position.ToString() + " found more than 2 rooms. Ignoring extra rooms.");
			}
		}
		if (!roomA) {
			print("Error: connector at " + this.transform.position.ToString() + " found 0 rooms. Destroying connector.");
			GameObject.Destroy(this.gameObject);
			return;
		}
		// Register pairs of voxels, where each pair has one voxel from roomA and one from roomB
		// If roomB is null, each pair has one voxel from roomA and the ambient atmosphere voxel
		List<FlowVoxel[]> pairList = new List<FlowVoxel[]>();
		if (!roomB) {
			foreach (FlowVoxel voxel in roomA.FlowVoxels) {
				if (Vector3.Distance(voxel.Position, boxCollider.ClosestPointOnBounds(voxel.Position)) < FlowSimManager.Radius * 2) {
					FlowVoxelConst ambientVoxel = new FlowVoxelConst(boxCollider.ClosestPointOnBounds(voxel.Position));
					roomA.AddExtraVoxel(ambientVoxel);
					pairList.Add(new FlowVoxel[] { voxel, ambientVoxel });
				}
			}
		}
		// If roomB is NOT null, look at all voxels in A and B that are within one voxel-radius of the connector bounds
		// For each of the A voxels, pair it with the closest B voxel (if no B voxel within one voxel-width, abandon the A voxel)
		else {
			List<FlowVoxel> candidatesB = new List<FlowVoxel>();
			foreach (FlowVoxel voxelB in roomB.FlowVoxels)
				if (Vector3.Distance(voxelB.Position, boxCollider.ClosestPointOnBounds(voxelB.Position)) < FlowSimManager.Radius * 2)
					candidatesB.Add(voxelB);
			
			float distanceClosest = Mathf.Infinity;
			foreach (FlowVoxel voxelA in roomA.FlowVoxels)
				if (Vector3.Distance(voxelA.Position, boxCollider.ClosestPointOnBounds(voxelA.Position)) < FlowSimManager.Radius * 2)
					foreach (FlowVoxel voxelB in candidatesB)
						if (Vector3.Distance(voxelA.Position, voxelB.Position) < distanceClosest)
							distanceClosest = Vector3.Distance(voxelA.Position, voxelB.Position);
			
			foreach (FlowVoxel voxelA in roomA.FlowVoxels) {
				if (Vector3.Distance(voxelA.Position, boxCollider.ClosestPointOnBounds(voxelA.Position)) < FlowSimManager.Radius * 2) {
					FlowVoxel closestInB = null;
					foreach (FlowVoxel voxelB in candidatesB)
						if (Vector3.Distance(voxelA.Position, voxelB.Position) <= distanceClosest + 0.001f)
							closestInB = voxelB;
					if (closestInB != null)
						pairList.Add(new FlowVoxel[] { voxelA, closestInB });
				}
			}
		}
		// Convert list of pairs to array of pairs
		pairs = new FlowVoxel[pairList.Count, 2];
		int i = 0;
		foreach (FlowVoxel[] pair in pairList) {
			pairs[i, 0] = pair[0];
			pairs[i++, 1] = pair[1];
		}
		if (isOpen)
			Open();
		// Notify the room(s)
		if (roomA)
			roomA.AddConnector(this);
		if (roomB)
			roomB.AddConnector(this);
	}


	void FixedUpdate()
	{
		if (isOpen && pairs != null) {
			float atmoDiff = 0;
			if (roomA) {
				if (roomB)
					atmoDiff = roomA.AvgAtmosphere - roomB.AvgAtmosphere;
				else
					atmoDiff = roomA.AvgAtmosphere - FlowSimManager.AmbientAtmosphere;
			}
			flowCheap = atmoDiff * FlowSimManager.Radius * 2 * pairs.Length;	// atmoDiff * approximate area

			if (roomA && roomA.SimulationType == FlowRoom.SimType.CHEAP) {
				for (int i = 0; i < pairs.GetLength(0); i++) {
					pairs[i, 0].SetAtmosphere(roomA.AvgAtmosphere);
					pairs[i, 0].Flow = roomA.GetCheapFlow();
				}
			}
			if (roomB && roomB.SimulationType == FlowRoom.SimType.CHEAP) {
				for (int i = 0; i < pairs.GetLength(0); i++) {
					pairs[i, 1].SetAtmosphere(roomB.AvgAtmosphere);
					pairs[i, 1].Flow = roomB.GetCheapFlow();
				}
			}
		}
		else
			flowCheap = 0;
	}


	public void Open() 
	{ 
		isOpen = true;
		for (int i = 0; i < pairs.GetLength(0); i++) {
			pairs[i, 0].AddNeighbor(pairs[i, 1], false);
			pairs[i, 1].AddNeighbor(pairs[i, 0], false);
		}
	}

	public void Close()
	{
		isOpen = false;
		for (int i = 0; i < pairs.GetLength(0); i++) {
			pairs[i, 0].RemoveNeighbor(pairs[i, 1], false);
			pairs[i, 1].RemoveNeighbor(pairs[i, 0], false);
		}
	}


	/// <summary>Should only be used for cheap simulation.</summary>
	public float GetCheapOutflowRate(FlowRoom room)
	{
		if (isOpen && room == roomA)
			return flowCheap;
		if (isOpen && room == roomB)
			return -1 * flowCheap;
		return 0;
	}


	// Display the gizmo in the editor - this doesn't affect the actual game
	void OnDrawGizmos() 
	{
		Gizmos.color = isOpen ? Color.cyan : Color.magenta;
		if (boxCollider)
			Gizmos.DrawWireCube(transform.position, Vector3.Scale(boxCollider.size, transform.lossyScale));
		Gizmos.color = Color.blue;
		if (pairs != null) 
			for (int i = 0; i < pairs.GetLength(0); i++)
				Gizmos.DrawLine(pairs[i, 0].Position, pairs[i, 1].Position);
	}

}
