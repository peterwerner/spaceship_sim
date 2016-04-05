using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class FlowRoomCollection : MonoBehaviour {

    BoxCollider boxCollider;
    List<FlowRoom> rooms = new List<FlowRoom>();


    bool started = false;
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


	Vector3 GetForceAt(Vector3 pos) 
	{
		Vector3 force;
		bool success;
		// TODO make this more efficient for large collections of rooms
		foreach (FlowRoom room in rooms) {
			force = room.GetForceAt(pos, out success);
			if (success)
				return force;
		}
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
