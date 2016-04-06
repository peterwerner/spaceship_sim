using UnityEngine;
using System.Collections;

[RequireComponent (typeof(ParticleSystem))]
public class FlowParticleSystem : MonoBehaviour {

	[SerializeField] FlowRoomCollection roomCollection;
	ParticleSystem particleSys;


	void Awake () 
	{
		particleSys = (ParticleSystem)GetComponent(typeof(ParticleSystem));
	}


	void Update () 
	{
		// Jumps to a random voxel in a random room from the room collection each frame
		// Arguments mean bias towards rooms with higher flow, higher atmosphere
		if (roomCollection.Initialized)
			this.transform.position = roomCollection.GetRandomRoomWeighted(0.5f, 0.3f).transform.position;
		// Alternatively, jump to a purely random voxel once we have chosen a room
		if (roomCollection.Initialized)
			this.transform.position = roomCollection.GetRandomRoomWeighted(0.5f, 0.3f).GetRandomVoxel().Position;
	}
	
}
