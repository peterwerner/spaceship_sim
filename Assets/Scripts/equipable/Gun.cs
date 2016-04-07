using UnityEngine;
using System.Collections;

public class Gun : Equipable {

	[SerializeField] protected float impactDamage = 10f;
	[SerializeField] protected float impactForce = 10f;
	[SerializeField] protected float shotDelay = 0.05f; 	// Min delay between shots (controls rate of fire)
	[SerializeField] protected float maxRange = 500f;		// Max range a bullet can travel
	[SerializeField] protected bool automatic = false;		// Automatic (click and hold) vs Semi-automatic (click per shot)
	[SerializeField] protected AudioClip audioShoot;


	protected override void OnEquip (Inventory inventory)
	{
	}

	protected override void OnUnequip (Inventory inventory)
	{
	}

}
