using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(BoxCollider))]
public class FlowConnector : MonoBehaviour {

	[Tooltip ("Forces this connector to not connect to ignored rooms.")]
	[SerializeField] FlowRoom[] ignore;
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
		List<FlowRoom> ignoreList = new List<FlowRoom>(ignore);
		float closestDistA = Mathf.Infinity, closestDistB = Mathf.Infinity;

		// Connect to the rooms that this is currently touching. This should connect to either 1 or 2 rooms.
		bool foundTooManyRooms = false;
		boxCollider = (BoxCollider)GetComponent(typeof(BoxCollider));
		foreach (FlowRoom room in FlowRoom.InstanceList) {
			if (room.Collider.bounds.Intersects(boxCollider.bounds) && !ignoreList.Contains(room)) {
				if (!roomA)
					roomA = room;
				else if (!roomB)
					roomB = room;
				else
					foundTooManyRooms = true;
			}
		}
		// If this found more than 2 rooms, pick the 2 rooms with the closest voxel to this
		// Let roomA be the closest room and roomB be the second closest room
		if (foundTooManyRooms) {
			roomA = null;	roomB = null;
			foreach (FlowRoom room in FlowRoom.InstanceList) {
				if (room.Collider.bounds.Intersects(boxCollider.bounds) && !ignoreList.Contains(room)) {
					float distClosestInRoom = Mathf.Infinity;
					foreach (FlowVoxel voxel in room.FlowVoxels) {
						distClosestInRoom = Mathf.Min(distClosestInRoom, Vector3.Distance(voxel.Position, ClosestPointOnCollider(voxel.Position)));
					}
					if (distClosestInRoom < closestDistA) {
						roomB = roomA;
						closestDistB = closestDistA;
						roomA = room;
						closestDistA = distClosestInRoom;
					}
					else if (distClosestInRoom < closestDistB) {
						roomB = room;
						closestDistB = distClosestInRoom;
					}
				}
			}
		}
		// If this found no rooms, destroy self and report error
		if (!roomA) {
			Debug.LogError("Error: connector at " + this.transform.position.ToString() + " found 0 rooms. Destroying connector.");
			GameObject.Destroy(this.gameObject);
			return;
		}

		closestDistA = Mathf.Infinity;
		foreach (FlowVoxel voxelA in roomA.FlowVoxels) {
			if (Vector3.Distance(voxelA.Position, ClosestPointOnCollider(voxelA.Position)) < closestDistA)
				closestDistA = Vector3.Distance(voxelA.Position, ClosestPointOnCollider(voxelA.Position));
		}
			
		// Register pairs of voxels, where each pair has one voxel from roomA and one from roomB
		// If roomB is null, each pair has one voxel from roomA and the ambient atmosphere voxel
		List<FlowVoxel[]> pairList = new List<FlowVoxel[]>();
		if (!roomB) {
			foreach (FlowVoxel voxel in roomA.FlowVoxels) {
				if (Vector3.Distance(voxel.Position, ClosestPointOnCollider(voxel.Position)) <= closestDistA + 0.01f) {
					FlowVoxelConst ambientVoxel = new FlowVoxelConst(ClosestPointOnCollider(voxel.Position));
					roomA.AddExtraVoxel(ambientVoxel);
					pairList.Add(new FlowVoxel[] { voxel, ambientVoxel });
				}
			}
		}
		// If roomB is NOT null, look at all voxels in A and B that are within one voxel-width of the connector bounds
		// For each of the A voxels, pair it with the closest B voxel (if no B voxel within one voxel-width, abandon the A voxel)
		else {
			closestDistB = Mathf.Infinity;
			foreach (FlowVoxel voxelB in roomB.FlowVoxels) {
				if (Vector3.Distance(voxelB.Position, ClosestPointOnCollider(voxelB.Position)) < closestDistB)
					closestDistB = Vector3.Distance(voxelB.Position, ClosestPointOnCollider(voxelB.Position));
			}

			List<FlowVoxel> candidatesB = new List<FlowVoxel>();
			foreach (FlowVoxel voxelB in roomB.FlowVoxels)
				if (Vector3.Distance(voxelB.Position, ClosestPointOnCollider(voxelB.Position)) <= closestDistB + 0.01f)
					candidatesB.Add(voxelB);
			
			float distanceClosest = Mathf.Infinity;
			foreach (FlowVoxel voxelA in roomA.FlowVoxels)
				if (Vector3.Distance(voxelA.Position, ClosestPointOnCollider(voxelA.Position)) <= closestDistA + 0.01f)
					foreach (FlowVoxel voxelB in candidatesB)
						if (Vector3.Distance(voxelA.Position, voxelB.Position) < distanceClosest)
							distanceClosest = Vector3.Distance(voxelA.Position, voxelB.Position);
			
			foreach (FlowVoxel voxelA in roomA.FlowVoxels) {
				if (Vector3.Distance(voxelA.Position, ClosestPointOnCollider(voxelA.Position)) <= closestDistA + 0.01f) {
					FlowVoxel closestInB = null;
					foreach (FlowVoxel voxelB in candidatesB)
						if (Vector3.Distance(voxelA.Position, voxelB.Position) <= distanceClosest + 0.01f)
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


	Vector3 ClosestPointOnCollider (Vector3 point)
	{
		Quaternion rotationIntial = boxCollider.transform.rotation;
		boxCollider.transform.rotation = Quaternion.Euler(Vector3.zero);
		Vector3 pointRotated = Quaternion.Inverse(rotationIntial) * (point - boxCollider.transform.position) + boxCollider.transform.position;
		Vector3 closestRotated = boxCollider.bounds.Contains(pointRotated) ? pointRotated : boxCollider.ClosestPointOnBounds(pointRotated);
		Vector3 closest = rotationIntial * (closestRotated - boxCollider.transform.position) + boxCollider.transform.position;
		boxCollider.transform.rotation = rotationIntial;
		return closest;
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

		Gizmos.color = Color.black;
		if (pairs != null) {
			for (int i = 0; i < pairs.GetLength(0); i++) {
				if (roomA)
					Gizmos.DrawLine(pairs[i, 0].Position, ClosestPointOnCollider(pairs[i, 0].Position));
				if (roomB)
					Gizmos.DrawLine(pairs[i, 1].Position, ClosestPointOnCollider(pairs[i, 1].Position));
			}
		}
	}

}
