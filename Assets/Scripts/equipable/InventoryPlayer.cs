using UnityEngine;
using System.Collections;

public class InventoryPlayer : Inventory {

	Equipable current;


	protected override void OnEquipItem (Equipable item)
	{
		if (current)
			current.Unequip(this);
		current = item;
	}

	protected override void OnUnequipItem (Equipable item)
	{
		if (current == item)
			current = null;
	}


	public override Equipable GetCurrentItem() { return current; }

}
