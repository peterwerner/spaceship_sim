using UnityEngine;
using System.Collections;

namespace AtmoFlowSim {
		
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
			float tolerance = 1f;	// If two particles are this within this distance, don't bother recalculating force
			Vector3 velocityTarget = Vector3.zero, posLastCalculated = Vector3.zero;
			for (int i = 0; i < p.Length; i++) {
				if (i <= 1 || Vector3.Distance(p[i].position, posLastCalculated) > tolerance) {
					velocityTarget = speedConstant * roomCollection.GetForceAt(p[i].position, true, out isInRoom);
					posLastCalculated = p[i].position;
				}
				if (isInRoom)
					p[i].velocity = Vector3.Lerp(p[i].velocity, velocityTarget, Time.deltaTime);
				color = p[i].startColor;
				color.a = Mathf.Min(velocityMaxAlpha, p[i].velocity.magnitude) / velocityMaxAlpha;
				p[i].startColor = color;
			}
			particleSys.SetParticles(p, k);
		}
		
	}

}