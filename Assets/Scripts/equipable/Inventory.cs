using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(AudioSource))]
public abstract class Inventory : MonoBehaviour {

	Dictionary<int, int> ammoPool = new Dictionary<int, int>();
	protected List<Equipable> items = new List<Equipable>();
	protected AudioSource audioSource;


	void Start ()
	{
		audioSource = GetComponent<AudioSource>();
	}


	public bool Equip (Equipable item)
	{
		if (!CanEquipItem(item) || !item.Equip(this))
			return false;
		if (item is Gun) {
			Gun gun = (Gun)item;
			// Add its ammo type to the ammo pool if it isn't already there
			if (!ammoPool.ContainsKey(gun.AmmoName.GetHashCode()))
				ammoPool[gun.AmmoName.GetHashCode()] = 0;
			// If we try to pick up a gun that we already have, destroy it and take its ammo
			foreach (Equipable other in items) {
				Gun otherGun = (Gun)other;
				if (otherGun.Name == gun.Name) {
					ammoPool[gun.AmmoName.GetHashCode()] = ammoPool[gun.AmmoName.GetHashCode()] + gun.AmmoInClip;
					Destroy(gun.gameObject);
					return true;
				}
			}
		}
		items.Add(item);
		OnEquipItem(item);
		return true;
	}

	public bool Unequip (Equipable item) 
	{
		if (!CanUnequipItem(item) || !item.Unequip(this))
			return false;
		items.Remove(item);
		OnUnequipItem(item);
		return true;
	}


	public int RequestAmmo (Gun.AmmoType type, int numDesired) { 
		int numGiven = Mathf.Min(numDesired, PeekAmmo(type));
		ammoPool[type.GetHashCode()] -= numGiven;
		return numGiven;
	}
	public int PeekAmmo (Gun.AmmoType type) { 
		if (!ammoPool.ContainsKey(type.GetHashCode()))
			return 0;
		return ammoPool[type.GetHashCode()];
	}


	public virtual bool CanEquipItem (Equipable item) 
	{
		return !(items.Contains(item));
	}

	public virtual bool CanUnequipItem (Equipable item)
	{
		return (items.Contains(item));
	}

	protected abstract void OnEquipItem (Equipable item);

	protected abstract void OnUnequipItem (Equipable item);

	public abstract Equipable GetCurrentItem();


	public void PlayOneShot(AudioClip clip) { audioSource.PlayOneShot(clip); }

}
