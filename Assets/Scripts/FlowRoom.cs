using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(BoxCollider))]
public class FlowRoom : ListComponent<FlowRoom> {

	// Full simulation: uses voxels to create a 3d vector field of air flow
	// Cheap simulation: estimates a single flow vector for the room, based on connected rooms
	public enum SimType { FULL, CHEAP };

	// Static registry of objects -> rooms, where each object is currently owned by that room
	static Dictionary<GameObject, FlowRoom> roomObjectRegistry = new Dictionary<GameObject, FlowRoom>();

	[SerializeField] bool forceCheapSim = false;
	bool _forceCheapSim;
	[SerializeField] [Tooltip("0 is vacuum, 1 is 1 atm")] [Range(0, 1)] float atmosphereStart = 1;
	[SerializeField] ForceApplierBase gravity;
	float avgAtmosphere, avgFlowMagnitude;
	Vector3 flowForceCheap = Vector3.zero;
	Quaternion rotationInitial;
	SimType simType = SimType.FULL;
	BoxCollider boxCollider;
	List<GameObject> ownedObjects = new List<GameObject>();	// Objects that are currently within the trigger volume
	List<FlowConnector> connectors = new List<FlowConnector>();
	FlowVoxel[ , , ] voxels = new FlowVoxel[0, 0, 0];		// Discretization of the volume of the room; these voxels are used to determine forces
	List<FlowVoxel> voxelsExtra = new List<FlowVoxel>();	// Additional voxels tied to this room, ie: constants tacked on by connectors
	MoveControl player = null;

	public float AvgAtmosphere	{ get { return avgAtmosphere; } }
	public float Atmosphere	{ get { return avgAtmosphere * voxels.Length; } }
	public BoxCollider Collider { get { return boxCollider; } }
	public FlowVoxel[ , , ] FlowVoxels { get { return voxels; } }


	void Awake()
	{
		boxCollider = (BoxCollider)GetComponent(typeof(BoxCollider));
		rotationInitial = transform.rotation;
		avgAtmosphere = atmosphereStart;
		if (forceCheapSim) {
			_forceCheapSim = true;
			simType = SimType.CHEAP;
			return;
		}
		// Construct voxels - fill the collider's bounds
		float voxelSize = FlowSimManager.Radius * 2;
		Vector3 colliderSize = Vector3.Scale(boxCollider.size, transform.lossyScale);
		Vector3 cornerLo = transform.position - 0.5f * colliderSize;
		Vector3 cornerHi = transform.position + 0.5f * colliderSize;
		Vector3 numVoxels = 0.5f * Vector3.one + (cornerHi - cornerLo) / voxelSize;
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
					Vector3 pos = rotationInitial * (new Vector3(x,y,z) - transform.position) + transform.position;

					voxels[i, j, k] = new FlowVoxel(pos, atmosphereStart);
					
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
		ownedObjects.RemoveAll(item => item == null);

		if (simType == SimType.FULL) 
			UpdateVoxels(Time.fixedDeltaTime);

		else if (simType == SimType.CHEAP) 
			UpdateCheap(Time.fixedDeltaTime);

		// Apply gravity + atmospheric flow forces
		foreach (GameObject obj in ownedObjects) {
			gravity.ApplyTo(obj);
			ForceApplierBase.ApplyForce(obj, GetForceAt(obj.transform.position));
		}
	}


	void Update()
	{
		if (player)
			player.gravity = gravity.GetDirection(player.gameObject) * gravity.GetAcceleration();
	}


	// Used in full simulation
	void UpdateVoxels(float timeStep)
	{
		avgAtmosphere = 0;
		avgFlowMagnitude = 0;
		foreach (FlowVoxel voxel in voxels) {
			avgAtmosphere += voxel.GetAtmosphere();
			avgFlowMagnitude += voxel.Flow.magnitude;
		}
		avgAtmosphere /= voxels.Length;
		avgFlowMagnitude = avgFlowMagnitude * FlowSimManager.FlowVectorConstant / voxels.Length; 
		
		foreach (FlowVoxel voxel in voxels)
			voxel.UpdateNextStep(timeStep);
		foreach (FlowVoxel voxel in voxelsExtra)
			voxel.UpdateNextStep(timeStep);
		foreach (FlowVoxel voxel in voxels)
			voxel.StepToNextStep(timeStep);
		foreach (FlowVoxel voxel in voxelsExtra)
			voxel.StepToNextStep(timeStep);
	}

	// Used in cheap simulation
	void UpdateCheap(float timeStep)
	{
		float netOutflow = 0;
		foreach (FlowConnector connector in connectors)
			netOutflow += connector.GetCheapOutflowRate(this);
		float volume = Mathf.Pow(2 * FlowSimManager.Radius, 2) * voxels.Length;
		avgAtmosphere -= FlowSimManager.CheapAtmoDeltaConstant * netOutflow * Time.fixedDeltaTime / volume;

		flowForceCheap = Vector3.zero;
		foreach (FlowConnector connector in connectors) {
			float magnitude = connector.GetCheapOutflowRate(this) * FlowSimManager.CheapFlowForceConstant;
			flowForceCheap += magnitude * (connector.transform.position - this.transform.position);
		}
	}


	/// <summary>Returns the force vector at a given point in the room.</summary>
	public Vector3 GetForceAt(Vector3 pos) 
	{
		bool success;
		return GetForceAt(pos, out success);
	}
	/// <summary>Returns the force vector at a given point in the room.</summary>
	/// <param name="success">true if pos was in the room's bounds</param> 
	public Vector3 GetForceAt(Vector3 position, out bool success) 
	{
		if (!boxCollider.bounds.Contains(position)) {
			success = false;
			return Vector3.zero;
		}

		Vector3 pos = Quaternion.Inverse(rotationInitial) * (position - transform.position) + transform.position;

		if (simType == SimType.FULL) 
		{
			float voxelSize = FlowSimManager.Radius * 2;
			Vector3 colliderSize = Vector3.Scale(boxCollider.size, transform.lossyScale);
			Vector3 cornerHi = transform.position + 0.5f * colliderSize;
			Vector3 indices = (colliderSize - (cornerHi - pos)) / voxelSize;
			int i = (int)indices.x, j = (int)indices.y, k = (int)indices.z;
			if (i < 0 || j < 0 || k < 0 || i >= voxels.GetLength(0) || j >= voxels.GetLength(1) || k >= voxels.GetLength(2)) {
				success = false;
				return Vector3.zero;
			}
			success = true;
			return voxels[i, j, k].Flow * FlowSimManager.FlowForceConstant;
		}

		else if (simType == SimType.CHEAP)
		{
			success = true;
			return flowForceCheap;
		}

		success = false;
		return Vector3.zero;
	}


	void OnTriggerEnter(Collider other) 
	{
		MoveControl playerComponent = other.gameObject.GetComponent<MoveControl>();
		FlowRoom room;
		if (roomObjectRegistry.TryGetValue(other.gameObject, out room)) {
			room.ownedObjects.Remove(other.gameObject);
			if (playerComponent && other.gameObject == playerComponent.gameObject)
				room.player = null;
		}
		if (playerComponent)
			player = playerComponent;
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
		if (player && other.gameObject == player.gameObject)
			player = null;
	}


	// Switch between full and cheap simulation types
	public SimType SimulationType {
		get 
		{
			return simType;
		}
		set 
		{
			if (simType == value || _forceCheapSim)
				return;
			simType = value;
			if (simType == SimType.FULL) {
				foreach (FlowVoxel voxel in voxels)
					voxel.SetAtmosphere(avgAtmosphere);
				foreach (FlowVoxel voxel in voxelsExtra)
					voxel.SetAtmosphere(avgAtmosphere);
			}
		}
	}


	/// <summary>Returns a random voxel in the room.</summary>
	public FlowVoxel GetRandomVoxel() 
	{ 
		return voxels[Random.Range(0, voxels.GetLength(0)), Random.Range(0, voxels.GetLength(1)), Random.Range(0, voxels.GetLength(2))]; 
	}


	/// <summary>Should be used to track extraneous voxels such as constant voxels generated by connectors linking this room and the outside air/vacuum.</summary>
	public void AddExtraVoxel(FlowVoxel voxel) { voxelsExtra.Add(voxel); }

	public void AddConnector(FlowConnector connector) { connectors.Add(connector); }

	/// <summary>Should only be used for cheap simulation.</summary>
	public Vector3 GetCheapFlow() { return flowForceCheap; }

	/// <summary>If running a full simulation, returns the average flow magnitude of all voxels in the room. 
	/// If running a cheap simulation, returns an approximation of flow magnitude through the room.
	/// Values returned by this may vary greatly between full and cheap simulations.</summary>
	public float AvgFlowMagnitude
	{
		get {
		if (simType == SimType.FULL)
			return avgFlowMagnitude;
		else if (simType == SimType.CHEAP)
			return flowForceCheap.magnitude / voxels.Length;
		return 0;
		}
	}
	public float FlowMagnitude
	{
		get { return AvgFlowMagnitude * voxels.Length; }
	}


	// Display the gizmo in the editor - this doesn't affect the actual game
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, Mathf.Min(1, avgAtmosphere), Mathf.Min(1, avgAtmosphere));
		if (boxCollider)
			Gizmos.DrawWireCube(transform.position, Vector3.Scale(boxCollider.size, transform.lossyScale));
		Gizmos.color = Color.yellow;
		foreach (GameObject obj in ownedObjects) {
			Gizmos.DrawWireCube(obj.transform.position, obj.transform.lossyScale);
			Gizmos.DrawLine(obj.transform.position, obj.transform.position + GetForceAt(obj.transform.position).normalized);
		}
		if (simType == SimType.FULL) {
			foreach (FlowVoxel voxel in voxels)
				voxel.DrawGizmo();
			foreach (FlowVoxel voxel in voxelsExtra)
				voxel.DrawGizmo();
			Gizmos.color = Color.green;
		}
	}

}
