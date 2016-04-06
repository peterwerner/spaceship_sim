using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class FlowRoomCollection : MonoBehaviour {

    BoxCollider boxCollider;
    List<FlowRoom> rooms = new List<FlowRoom>();


    bool started = false;
	public bool Initialized { get { return started; } }
    void Update()
    {
        if (!started)
        {
            started = true;
            LateStart();
        }
    }


    void LateStart()
    {
        boxCollider = (BoxCollider)GetComponent(typeof(BoxCollider));
        foreach (FlowRoom room in FlowRoom.InstanceList)
            if (room.Collider.bounds.Intersects(boxCollider.bounds))
                rooms.Add(room);
    }


	/// <summary>Returns the force vector at a given point in the room collection.</summary>
	public Vector3 GetForceAt(Vector3 pos) 
	{
		return GetForceAt(pos, false);
	}
	/// <summary>Returns the force vector at a given point in the room collection.</summary>
	/// <param name="onlyFullSimRooms">Should rooms running cheap simulations be ignored?</param>
	public Vector3 GetForceAt(Vector3 pos, bool onlyFullSimRooms) 
	{
		Vector3 force;
		bool success;
		foreach (FlowRoom room in rooms) {
			if (onlyFullSimRooms && room.SimulationType == FlowRoom.SimType.FULL) {
				force = room.GetForceAt(pos, out success);
				if (success)
					return force;
			}
		}
		return Vector3.zero;
	}


	/// <summary>Returns a room in the collection via a weighted random selection. Higher weight is assigned to rooms with higher average flow rate and atmosphere.</summary>
	/// <param name="flowBias">range [0, 1]; determines how much influence the average flow rate has</param>
	/// <param name="atmoBias">range [0, 1]; determines how much influence the atmosphere level has</param>
	public FlowRoom GetRandomRoomWeighted(float flowBias, float atmoBias)
	{
		if (flowBias < 0 || atmoBias < 0 || flowBias > 1 || atmoBias > 1)
			Debug.LogError("GetRandomRoomWeighted: bias arguments outside of acceptable range [0, 1]");
		float baseline = 0.001f, sumWeights = 0f, maxFlow = Mathf.NegativeInfinity;
		foreach (FlowRoom room in rooms)
			if (room.SimulationType == FlowRoom.SimType.FULL && baseline + room.GetFlowMagnitude() > maxFlow) 
				maxFlow = baseline + room.GetFlowMagnitude();
		foreach (FlowRoom room in rooms)
			if (room.SimulationType == FlowRoom.SimType.FULL)
				sumWeights += GetRoomWeight(room, flowBias, atmoBias, baseline, maxFlow);
		float random = Random.Range(0f, sumWeights);
		foreach (FlowRoom room in rooms) {
			if (room.SimulationType == FlowRoom.SimType.FULL) {
				random -= GetRoomWeight(room, flowBias, atmoBias, baseline, maxFlow);
				if (random - 0.0001f < 0)
					return room;
			}
		}
		return null;
	}
	private float GetRoomWeight(FlowRoom room, float flowBias, float atmoBias, float baseline, float maxFlow)
	{
		return flowBias * ((baseline + room.GetFlowMagnitude()) / maxFlow) 
			+ atmoBias * (baseline + room.GetFlowMagnitude())
			+ Mathf.Min(0, 1 - (flowBias + atmoBias));
	}


    // Display the gizmo in the editor - this doesn't affect the actual game
    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        if (boxCollider)
            Gizmos.DrawWireCube(transform.position, Vector3.Scale(boxCollider.size, transform.lossyScale));
    }

}
