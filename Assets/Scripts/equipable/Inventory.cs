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


	public int RequestAmmo (int type, int numDesired) { 
		if (!ammoPool.ContainsKey(type))
			return 0;
		int numGiven = Mathf.Min(numDesired, ammoPool[type]);
		ammoPool[type] -= numGiven;
		return numGiven;
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
