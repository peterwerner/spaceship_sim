using UnityEngine;
using System.Collections;

public class UseKey : MonoBehaviour {

	[SerializeField] KeyCode key;
	[SerializeField] float useDist = 1.5f;
	[SerializeField] Inventory inventory;
	[SerializeField] LayerMask layerMask;

	
	void Update () 
	{
		if (Input.GetKeyDown(key)) 
		{
			RaycastHit hit;
			Ray ray = new Ray(transform.position, transform.forward);
			if (Physics.Raycast(ray, out hit, useDist, layerMask)) {
				Transform objHit = hit.transform;

				Equipable equipable = objHit.GetComponent<Equipable>();
				if (equipable)
					inventory.Equip(equipable);
			}
		}
	}

}
