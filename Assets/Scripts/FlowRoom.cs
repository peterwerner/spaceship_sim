using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(BoxCollider))]
public class FlowRoom : ListComponent<FlowRoom> {

	// Static registry of objects -> rooms, where each object is currently owned by that room
	static Dictionary<GameObject, FlowRoom> roomObjectRegistry = new Dictionary<GameObject, FlowRoom>();

	[SerializeField] [Tooltip("0 is vacuum, 1 is 1 atm")] [Range(0, 1)] float atmosphereStart = 1;
	[SerializeField] ForceApplierBase gravity;
	float atmosphere;
	BoxCollider boxCollider;
	List<GameObject> ownedObjects = new List<GameObject>();
	FlowVoxel[ , , ] voxels = new FlowVoxel[0, 0, 0];
	List<FlowVoxel> voxelsExtra = new List<FlowVoxel>();	// Additional voxels tied to this room, ie: constants tacked on by connectors

	public float Atmosphere	{ get { return atmosphere; } }
	public BoxCollider Collider { get { return boxCollider; } }
	public FlowVoxel[ , , ] FlowVoxels { get { return voxels; } }


	void Start()
	{
		boxCollider = (BoxCollider)GetComponent(typeof(BoxCollider));
		atmosphere = atmosphereStart;
		// Construct voxels - fill the collider's bounds
		float voxelSize = FlowVoxelManager.Radius * 2;
		Vector3 colliderSize = Vector3.Scale(boxCollider.size, transform.lossyScale);
		Vector3 cornerLo = transform.position - 0.5f * colliderSize;
		Vector3 cornerHi = transform.position + 0.5f * colliderSize;
		Vector3 numVoxels = (cornerHi - cornerLo) / voxelSize;
		voxels = new FlowVoxel[(int)numVoxels.x, (int)numVoxels.y, (int)numVoxels.z];
		Vector3 offsetStart = (colliderSize - (new Vector3(voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2)) - Vector3.one) * voxelSize) / 2;
		/*
		int[ , ] offsets = {
			{0,0,-1},	{0,-1,0},	{-1, 0, 0}, 
			{-1,-1,0},	{-1,1,0},	{-1,0,0},	{-1,0,1},
			{-1,-1,-1},	{-1,-1,1},	{-1,1,-1},	{-1,1,1},
			{0,-1,-1},	{0,-1,1}
		};
		*/
		float x, y, z;
		int i, j, k;
		for (i = 0, x = cornerLo.x + offsetStart.x;   i < voxels.GetLength(0);   i++, x += voxelSize) {
			for (j = 0, y = cornerLo.y + offsetStart.y;   j < voxels.GetLength(1);   j++, y += voxelSize) {
				for (k = 0, z = cornerLo.z + offsetStart.z;   k < voxels.GetLength(2);   k++, z += voxelSize) 
				{
					voxels[i, j, k] = new FlowVoxel(new Vector3(x,y,z), atmosphereStart);
					
					if (i > 0)
						voxels[i, j, k].AddNeighbor(voxels[i-1, j, k], true);
					if (j > 0)
						voxels[i, j, k].AddNeighbor(voxels[i, j-1, k], true);
					if (k > 0)
						voxels[i, j, k].AddNeighbor(voxels[i, j, k-1], true);
					
					/*
					for (int q = 0; q < offsets.GetLength(0); q++) {
						int i2 = i + offsets[q,0], j2 = j + offsets[q,1], k2 = k + offsets[q,2];
						if (i2 >= 0 && j2 >= 0 && k2 >= 0 && i2 < voxels.GetLength(0) && j2 < voxels.GetLength(1) && k2 < voxels.GetLength(2))
							voxels[i, j, k].AddNeighbor(voxels[i2, j2, k2], true);
					}
					*/
				}
			}
		}
	}


	void FixedUpdate()
	{
		UpdateVoxels(Time.fixedDeltaTime);
		// Update atmosphere
		atmosphere = 0;
		foreach (FlowVoxel voxel in voxels)
			atmosphere += voxel.GetAtmosphere();
		atmosphere /= voxels.Length;
		// Apply gravity + atmospheric flow forces
		foreach (GameObject obj in ownedObjects) {
			gravity.ApplyTo(obj);
			ForceApplierBase.ApplyForce(obj, GetForceAt(obj.transform.position));
		}
	}


	public void UpdateVoxels(float timeStep)
	{
		foreach (FlowVoxel voxel in voxels)
			voxel.UpdateNextStep(timeStep);
		foreach (FlowVoxel voxel in voxelsExtra)
			voxel.UpdateNextStep(timeStep);
		foreach (FlowVoxel voxel in voxels)
			voxel.StepToNextStep(timeStep);
		foreach (FlowVoxel voxel in voxelsExtra)
			voxel.StepToNextStep(timeStep);
	}


	public Vector3 GetForceAt(Vector3 pos) 
	{
		bool success;
		return GetForceAt(pos, out success);
	}
	public Vector3 GetForceAt(Vector3 pos, out bool success) 
	{
		if (!boxCollider.bounds.Contains(pos)) {
			success = false;
			return Vector3.zero;
		}
		float voxelSize = FlowVoxelManager.Radius * 2;
		Vector3 colliderSize = Vector3.Scale(boxCollider.size, transform.lossyScale);
		Vector3 cornerHi = transform.position + 0.5f * colliderSize;
		Vector3 indices = (colliderSize - (cornerHi - pos)) / voxelSize;
		int i = (int)indices.x, j = (int)indices.y, k = (int)indices.z;
		if (i < 0 || j < 0 || k < 0 || i >= voxels.GetLength(0) || j >= voxels.GetLength(1) || k >= voxels.GetLength(2)) {
			success = false;
			return Vector3.zero;
		}
		success = true;
		return voxels[i, j, k].Flow * voxels[i, j, k].Flow.magnitude * FlowVoxelManager.FlowForceConstant;
	}


	void OnTriggerEnter(Collider other) 
	{
		FlowRoom room;
		if (roomObjectRegistry.TryGetValue(other.gameObject, out room))
			room.ownedObjects.Remove(other.gameObject);
		roomObjectRegistry.Remove(other.gameObject);
		roomObjectRegistry.Add(other.gameObject, this);
		ownedObjects.Add(other.gameObject);	
	}

	void OnTriggerExit(Collider other) 
	{
		FlowRoom room;
		if (roomObjectRegistry.TryGetValue(other.gameObject, out room) && room == this)
			roomObjectRegistry.Remove(other.gameObject);
		ownedObjects.Remove(other.gameObject);
	}


	public void AddExtraVoxel(FlowVoxel voxel) { voxelsExtra.Add(voxel); }


	// Display the gizmo in the editor - this doesn't affect the actual game
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, Mathf.Min(1, atmosphere), Mathf.Min(1, atmosphere));
		if (boxCollider)
			Gizmos.DrawWireCube(transform.position, Vector3.Scale(boxCollider.size, transform.lossyScale));
		Gizmos.color = Color.yellow;
		foreach (GameObject obj in ownedObjects)
			Gizmos.DrawWireCube(obj.transform.position, obj.transform.lossyScale);
	}

}
