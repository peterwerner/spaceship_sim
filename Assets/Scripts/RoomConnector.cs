using UnityEngine;
using System.Collections;

/**
 * Connects two rooms A and B (or one room A to the 'outside')
 * Calculates flow rate of atmosphere between the two rooms
 * Flow (A -> B) = flow constant * (atmo A - atmo B) * area
 */
[RequireComponent (typeof(ParticleSystem))]
public class RoomConnector : MonoBehaviour {

	static float ambientAtmosphere = 0;	// Atmosphere of 'null' rooms
	static float flowConstant = 10;		// Flow rate is directly proportional to this constant
	static float particleDensityConstant = 0.1f;

	public bool isOpen = false;

	[Tooltip("Surface area in square units of the face of the 'window' between the two rooms")]
	[SerializeField] float area;

	float connectDist = 0.5f;
	float flow = 0;
	Room roomA = null, roomB = null;
	ParticleSystem particles = null;
	float particleAlpha = 0;

	public ParticleSystem Particles	{ get { return particles; } 	private set { particles = value; } }
	public float ParticleAlpha		{ get { return particleAlpha; } private set { particleAlpha = value; } }


	void Start()
	{
		particles = GetComponent<ParticleSystem>();
		// Connect to the rooms that this is currently touching. This should connect to either 1 or 2 rooms.
		RaycastHit[] hits = Physics.SphereCastAll(transform.position, connectDist, transform.forward, Mathf.Infinity);
		foreach (RaycastHit hit in hits) {
			Room room = hit.transform.GetComponent<Room>();
			if (room) {
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
		// Notify the room(s)
		if (roomA)
			roomA.AddConnector(this);
		if (roomB)
			roomB.AddConnector(this);
	}


	void FixedUpdate()
	{
		float atmoDiff = 0;
		if (isOpen) {
			if (roomA) {
				if (roomB)
					atmoDiff = roomA.GetAtmosphere() - roomB.GetAtmosphere();
				else
					atmoDiff = roomA.GetAtmosphere() - ambientAtmosphere;
			}
		}
		flow = flowConstant * atmoDiff * area;
		// Update particle transparency and density based on flow rate
		particleAlpha = Mathf.Min(1, Mathf.Abs(atmoDiff));
		Color color = particles.startColor;
		color.a = particleAlpha;
		particles.startColor = color;
		ParticleSystem.EmissionModule em = particles.emission;
		ParticleSystem.MinMaxCurve curve = em.rate;
		curve.constantMax = particleDensityConstant * Mathf.Abs(flow);
		curve.constantMin = curve.constantMax;
	}


	/**
	 *	Returns positive if flow is out of the room, negative if flow is into the room
	 */
	public float GetOutflowRate(Room room)
	{
		if (room == roomA)
			return flow;
		if (room == roomB)
			return -1 * flow;
		return 0;
	}


	public Room GetOtherRoom(Room room)
	{
		if (room == roomA)
			return roomB;
		if (room == roomB)
			return roomA;
		return null;
	}


	// Display the gizmo in the editor - this doesn't affect the actual game
	void OnDrawGizmos() 
	{
		Gizmos.color = isOpen ? Color.cyan : Color.magenta;
		Gizmos.DrawWireCube(this.transform.position, connectDist * 2 * Vector3.one);
		if (roomA)
			Gizmos.DrawLine(this.transform.position, roomA.transform.position);
		if (roomB)
			Gizmos.DrawLine(this.transform.position, roomB.transform.position);
	}

}
