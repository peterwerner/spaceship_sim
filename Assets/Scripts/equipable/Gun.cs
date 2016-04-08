using UnityEngine;
using System.Collections;
using System;

public class Gun : Equipable {

	/* DEBUG TEMP CODE PLS REPLACE ME LATER */
	void OnGUI()
	{
		if (state == State.EQUIPPED_FPS)
			GUI.Label(new Rect(Vector2.zero, new Vector2(200, 50)), "Ammo: " + numShotsLoaded + " | " + inventory.PeekAmmo(ammoType));
	}

	static string animShoot = "Shoot", animReload = "Reload";

	public enum GunName {};
	public enum AmmoType {};

	[Serializable]
	protected class GunStats {
		[SerializeField] public float impactDamage = 10f;
		[SerializeField] public float impactForce = 10f;
		[SerializeField] public float timeBetweenShots = 0.05f;
		[SerializeField] public int bulletsPerShot = 1;
		[SerializeField] public float spreadAngleDegrees = 4f;
		[SerializeField] public float maxRange = 500f;
		[SerializeField] public bool automatic = false;
		[SerializeField] public int clipSize = 1;
		[SerializeField] public float reloadTimeClipFull = 1f;
		[SerializeField] public float reloadTimeCanShoot = 0.2f;
	}

	[HideInInspector] public LayerMask layermask;
	[SerializeField] GunName uniqueName;
	[SerializeField] AmmoType ammoType;
	[SerializeField] GunStats stats;
	[SerializeField] AudioClip audioShoot; 
	int numShotsLoaded = 0;
	float timeSinceLastShot = 0, timeSinceStartedReload = 0;
	bool reloading = false;


	void Start ()
	{
		numShotsLoaded = stats.clipSize;
	}


	void Update ()
	{
		timeSinceLastShot += Time.deltaTime;
		if (reloading) {
			if (timeSinceStartedReload >= stats.reloadTimeClipFull)
				ReloadFinish();
			if (timeSinceStartedReload >= stats.reloadTimeClipFull + stats.reloadTimeCanShoot)
				reloading = false;
			else
				timeSinceStartedReload += Time.deltaTime;
		}
	}


	public bool Shoot ()
	{
		if (reloading || timeSinceLastShot < stats.timeBetweenShots || numShotsLoaded <= 0) 
			return false;

		for (int i = 0; i < stats.bulletsPerShot; i++)
		{
			RaycastHit hit = new RaycastHit();	
			Quaternion randomRot = Quaternion.Euler(UnityEngine.Random.insideUnitSphere * stats.spreadAngleDegrees);
			Ray ray = new Ray(transform.position, randomRot * transform.forward);
			if (Physics.Raycast(ray, out hit, stats.maxRange, layermask)) 
			{
				// Debug.DrawRay(ray.origin + new Vector3(0, -0.01f, 0), ray.direction * hit.distance, Color.white, 0.1f);

				Rigidbody rigidbodyHit = hit.collider.GetComponent<Rigidbody>();
				if (rigidbodyHit)
					rigidbodyHit.AddForceAtPosition(stats.impactForce * hit.normal * -1, hit.point);
			}
		}
		if (audioShoot)
			inventory.PlayOneShot(audioShoot);
		fpsView.Anim.SetTrigger(animShoot);
		timeSinceLastShot = 0;
		numShotsLoaded--;
		return true;
	}


	public bool Reload ()
	{
		if(reloading || numShotsLoaded >= stats.clipSize || inventory.PeekAmmo(ammoType) <= 0)
			return false;
		fpsView.Anim.SetTrigger(animReload);
		timeSinceStartedReload = 0;
		reloading = true;
		return true;
	}
	void ReloadFinish ()
	{
		numShotsLoaded += inventory.RequestAmmo(ammoType, stats.clipSize - numShotsLoaded);
	}


	protected override void OnEquip (Inventory inventory)
	{
	}

	protected override void OnUnequip (Inventory inventory)
	{
		reloading = false;
	}


	void OnDisable ()
	{
		reloading = false;
	}


	public bool Automatic { get { return stats.automatic; } }
	public GunName Name { get { return uniqueName; } }
	public AmmoType AmmoName { get { return ammoType; } }
	public int AmmoInClip { get { return numShotsLoaded; } }

}
