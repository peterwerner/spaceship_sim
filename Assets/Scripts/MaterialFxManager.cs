using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent (typeof(AudioSource))]
public class MaterialFxManager : SingletonComponent<MaterialFxManager> {

	[Serializable]
	public class MaterialFxInstance {
		[SerializeField] ParticleSystem bulletHitParticleSysPrefab;
		[HideInInspector] public ParticleSystem bulletHitParticleSys;
		public int bulletHitParticleCount;
		public AudioClip[] bulletHitSounds, physHitSoundsSoft, physHitSoundsHard, footstepSounds, jumpSounds, landSounds;
		public void Init() {
			bulletHitParticleSys = Instantiate(bulletHitParticleSysPrefab);
			bulletHitParticleSys.simulationSpace = ParticleSystemSimulationSpace.World;
		}
		public static AudioClip RandomAudio(AudioClip[] options) { 
			return options.Length > 0 ? options[UnityEngine.Random.Range(0, options.Length - 1)] : null; 
		}
	}
	[Serializable]
	public class MaterialFxInstanceMapping {
		public PhysicMaterial[] materials;
		public MaterialFxInstance effects;
	}

	[SerializeField] float particleSpeedConstant = 0.5f;
	[SerializeField] MaterialFxInstance defaultEffects;
	[SerializeField] MaterialFxInstanceMapping[] materialSpecificEffects;
	Dictionary<PhysicMaterial, MaterialFxInstance> map = new Dictionary<PhysicMaterial, MaterialFxInstance>();
	ParticleSystem[] allParticleSystems;
	AudioSource audioSrc;

	protected override void Awake() 
	{
		base.Awake();
		audioSrc = GetComponent<AudioSource>();
		allParticleSystems = new ParticleSystem[materialSpecificEffects.Length + 1];
		int i = 0;
		foreach (MaterialFxInstanceMapping mapping in materialSpecificEffects) {
			foreach (PhysicMaterial mat in mapping.materials)
				map.Add(mat, mapping.effects);
			mapping.effects.Init();
			allParticleSystems[i++] = mapping.effects.bulletHitParticleSys;
		}
		defaultEffects.Init();
		allParticleSystems[i] = defaultEffects.bulletHitParticleSys;
		StartCoroutine("UpdateParticleSystems");
	}


	/// <summary>Update one particle system each frame, taking into account simulation forces</summary>
	IEnumerator UpdateParticleSystems()
	{
		int index = 0;
		while(true){
			index = index++ % allParticleSystems.Length;

			ParticleSystem.Particle[] p = new ParticleSystem.Particle[allParticleSystems[index].particleCount];
			int k = allParticleSystems[index].GetParticles(p);
			AtmoFlowSim.FlowRoom room = null;
			float tolerance = 4f;	// If two particles are this within this distance, don't bother recalculating force
			Vector3 velocityTarget = Vector3.zero, posLastCalculated = Vector3.zero;
			for (int i = 0; i < p.Length; i++) {
				if (i <= 1 || Vector3.Distance(p[i].position, posLastCalculated) > tolerance) {
					velocityTarget = particleSpeedConstant * AtmoFlowSim.FlowRoomCollection.GlobalGetForceAt(p[i].position, true, out room) * allParticleSystems.Length;
					posLastCalculated = p[i].position;
				}
				if (room) {
					velocityTarget += room.Gravity * Time.deltaTime;
					p[i].velocity = Vector3.Lerp(p[i].velocity, velocityTarget, Time.deltaTime);
				}
			}
			allParticleSystems[index].SetParticles(p, k);
			yield return null;
		}
	}


	public void DoBulletImpact (RaycastHit hit)
	{
		MaterialFxInstance effects = EffectsFromMaterial(hit.collider.sharedMaterial);
		audioSrc.transform.position = hit.point;
		audioSrc.PlayOneShot(MaterialFxInstance.RandomAudio(effects.bulletHitSounds));
		effects.bulletHitParticleSys.transform.position = hit.point;
		effects.bulletHitParticleSys.transform.forward = hit.normal;
		effects.bulletHitParticleSys.Emit(effects.bulletHitParticleCount);
	}


	MaterialFxInstance EffectsFromMaterial(PhysicMaterial mat)
	{
		if (!mat)
			return defaultEffects;
		MaterialFxInstance effects;
		if (map.TryGetValue(mat, out effects))
			return effects;
		return defaultEffects;
	}

}
