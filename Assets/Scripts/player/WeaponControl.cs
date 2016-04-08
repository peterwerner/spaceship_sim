using UnityEngine;
using System.Collections;

public class WeaponControl : MonoBehaviour {

	[SerializeField] Inventory inventory;
	[SerializeField] LayerMask layermask;
	[SerializeField] KeyCode shootKey;

	
	void Update () 
	{
		if (inventory.GetCurrentItem() is Gun) {
			Gun gun = (Gun)inventory.GetCurrentItem();
			gun.layermask = layermask;
			if (Input.GetKeyDown(shootKey) || (gun.Automatic && Input.GetKey(shootKey)))
				gun.Shoot();
		}
	}

}
