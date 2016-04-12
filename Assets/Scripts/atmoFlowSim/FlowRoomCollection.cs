using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AtmoFlowSim {

	[RequireComponent(typeof(BoxCollider))]
	public class FlowRoomCollection : ListComponent<FlowRoomCollection> {

	    BoxCollider boxCollider;
	    List<FlowRoom> rooms = new List<FlowRoom>();


	    void Start()
	    {
	        boxCollider = (BoxCollider)GetComponent(typeof(BoxCollider));
	        foreach (FlowRoom room in FlowRoom.InstanceList)
	            if (room.Collider.bounds.Intersects(boxCollider.bounds))
	                rooms.Add(room);
	    }


		/// <summary>Returns the force vector at a given point in the room collection.</summary>
		/// <param name="onlyFullSimRooms">Should rooms running cheap simulations be ignored?</param>
		/// /// <param name="success">True IFF the point belonged to one of the rooms.</param>
		public Vector3 GetForceAt(Vector3 pos, bool onlyFullSimRooms, out bool success) {
			FlowRoom outRoom = null;
			return GetForceAt(pos, onlyFullSimRooms, out success, out outRoom);
		}
		public Vector3 GetForceAt(Vector3 pos, bool onlyFullSimRooms, out bool success, out FlowRoom outRoom) 
		{
			Vector3 force;
			success = false;
			outRoom = null;
			float atmo;
			foreach (FlowRoom room in rooms) {
				if (onlyFullSimRooms && room.SimulationType == FlowRoom.SimType.FULL) {
					force = room.GetForceAt(pos, out success, out atmo);
					if (success) {
						outRoom = room;
						return force;
					}
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
				if (room.SimulationType == FlowRoom.SimType.FULL && baseline + room.FlowMagnitude > maxFlow) 
					maxFlow = baseline + room.FlowMagnitude;
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
			return flowBias * ((baseline + room.FlowMagnitude) / maxFlow) 
				+ atmoBias * (baseline + room.FlowMagnitude)
				+ Mathf.Min(0, 1 - (flowBias + atmoBias));
		}


		public float GetTotalAtmosphere ()
		{
			float total = 0;
			foreach (FlowRoom room in rooms) {
				if (room.SimulationType == FlowRoom.SimType.FULL) {
					total += room.Atmosphere;
				}
			}
			return total;
		}

		public float GetTotalFlowMagnitude ()
		{
			float total = 0;
			foreach (FlowRoom room in rooms) {
				if (room.SimulationType == FlowRoom.SimType.FULL) {
					total += room.FlowMagnitude;
				}
			}
			return total;
		}


		public static Vector3 GlobalGetForceAt(Vector3 point, bool onlyFullSimRooms, out FlowRoom room)
		{
			bool success = false;
			Vector3 force;
			foreach (FlowRoomCollection collection in FlowRoomCollection.InstanceList) {
				force = collection.GetForceAt(point, onlyFullSimRooms, out success, out room);
				if (success) {
					return force;
				}
			}
			room = null;
			return Vector3.zero;
		}


	    // Display the gizmo in the editor - this doesn't affect the actual game
	    void OnDrawGizmos()
	    {
	        Gizmos.color = Color.black;
	        if (boxCollider)
	            Gizmos.DrawWireCube(transform.position, Vector3.Scale(boxCollider.size, transform.lossyScale));
	    }

	}

}