using UnityEngine;
using System.Collections;

[RequireComponent (typeof(ParticleSystem))]
public class FlowParticleSystem : MonoBehaviour {

	[SerializeField] float rateConstant = 0.001f;
	[SerializeField] float speedConstant = 0.5f;
	[Tooltip ("Once a particle reaches this velocity, it is fully translucent. Particles below this value are partially transparent.")]
	[SerializeField] float velocityMaxAlpha = 10f;
	[SerializeField] FlowRoomCollection roomCollection;
	ParticleSystem particleSys;
	ParticleSystem.EmissionModule emission;
	ParticleSystem.MinMaxCurve rate;


	void Awake () 
	{
		particleSys = (ParticleSystem)GetComponent(typeof(ParticleSystem));
		emission = particleSys.emission;
	}


	void Update () 
	{
		// Jumps to a random voxel in a random room from the room collection each frame
		// Bias towards rooms with higher flow, higher atmosphere
		this.transform.position = roomCollection.GetRandomRoomWeighted(0.6f, 0.2f).GetRandomVoxel().Position;

		rate = emission.rate;
		rate.constantMax = roomCollection.GetTotalFlowMagnitude() * roomCollection.GetTotalAtmosphere() * rateConstant;
		rate.constantMin = rate.constantMax;
		emission.rate = rate;

		ParticleSystem.Particle[] p = new ParticleSystem.Particle[particleSys.particleCount];
		int k = particleSys.GetParticles(p);
		bool isInRoom = false;
		Color color;
		Vector3 velocityTarget;
		for (int i = 0; i < p.Length; i++) {
			velocityTarget = speedConstant * roomCollection.GetForceAt(p[i].position, true, out isInRoom);
			if (isInRoom)
				p[i].velocity = Vector3.Lerp(p[i].velocity, velocityTarget, Time.deltaTime);
			color = p[i].startColor;
			color.a = Mathf.Min(velocityMaxAlpha, p[i].velocity.magnitude) / velocityMaxAlpha;
			p[i].startColor = color;
		}
		particleSys.SetParticles(p, k);
	}
	
}
