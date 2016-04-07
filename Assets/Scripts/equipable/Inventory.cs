using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Inventory : MonoBehaviour {

	protected List<Equipable> items = new List<Equipable>();


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

}
