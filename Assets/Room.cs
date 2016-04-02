using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Collider))]
public class Room : MonoBehaviour {

	// Static registry of objects -> rooms, where each object is currently owned by that room
	static Dictionary<GameObject, Room> roomObjectRegistry = new Dictionary<GameObject, Room>();
	static float flowForceConstant = 0.5f;	// Force = flow rate * flow force constant

	[SerializeField] ForceApplierBase gravity;
	[SerializeField] [Tooltip("0 is vacuum, 1 is 1 atm")] [Range(0, 1)] float atmosphere = 1;
	List<GameObject> ownedObjects = new List<GameObject>();
	List<RoomConnector> connectors = new List<RoomConnector>();
	float volume;


	void Start()
	{
		Vector3 size = GetComponent<Collider>().bounds.size;
		volume = size.x * size.y * size.z;
	}


	void FixedUpdate()
	{
		// Update atmosphere
		float netOutflow = 0;
		foreach (RoomConnector connector in connectors)
			netOutflow += connector.GetOutflowRate(this);
		atmosphere -= netOutflow * Time.fixedDeltaTime / volume;
		// Apply gravity + atmospheric flow forces
		foreach (GameObject obj in ownedObjects) {
			gravity.ApplyTo(obj);
			foreach (RoomConnector connector in connectors) {
				float flowForce = connector.GetOutflowRate(this) * flowForceConstant;
				ForceApplierPoint.ApplyTo(obj, connector.transform.position, flowForce);
			}
		}
	}


	void OnTriggerEnter(Collider other) 
	{
		Room room;
		if (roomObjectRegistry.TryGetValue(other.gameObject, out room))
			room.ownedObjects.Remove(other.gameObject);
		roomObjectRegistry.Remove(other.gameObject);
		roomObjectRegistry.Add(other.gameObject, this);
		ownedObjects.Add(other.gameObject);	
	}

	void OnTriggerExit(Collider other) 
	{
		Room room;
		if (roomObjectRegistry.TryGetValue(other.gameObject, out room) && room == this)
			roomObjectRegistry.Remove(other.gameObject);
		ownedObjects.Remove(other.gameObject);
	}


	public void AddConnector(RoomConnector connector)
	{
		connectors.Add(connector);
	}


	public float GetAtmosphere() { return atmosphere; }


	// Display the gizmo in the editor - this doesn't affect the actual game
	void OnDrawGizmos() 
	{
		Gizmos.color = new Color(1, Mathf.Min(1, atmosphere), Mathf.Min(1, atmosphere));
		MeshFilter mesh = GetComponent<MeshFilter>();
		Gizmos.DrawWireCube(transform.position, Vector3.Scale(mesh.sharedMesh.bounds.size, transform.lossyScale));
	}

}
