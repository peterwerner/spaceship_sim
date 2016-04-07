using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(Collider))]
public abstract class Equipable : MonoBehaviour {

	/// <summary>Is the equipable in the player's inventory, an NPC's inventory, or free to be picked up?</summary>
	public enum State { EQUIPPED_FPS, EQUIPPED_WORLD, FREE_WORLD };

	[Tooltip ("World model. Enabled when freely in the world or when held by an NPC.")]
	[SerializeField] EquipableFpsView viewmodelPrefab;
	[Tooltip ("View model. Enabled only when held by the player.")]
	[SerializeField] Transform worldView;
	protected State state = State.FREE_WORLD;
	protected Inventory inventory = null;
	Rigidbody rigidBody;
	new Collider collider;
	EquipableFpsView fpsView = null;

	protected virtual void Awake () 
	{
		rigidBody = (Rigidbody)GetComponent(typeof(Rigidbody));
		collider = (Collider)GetComponent(typeof(Collider));
	}
		

	public bool Equip (Inventory inventory)
	{
		if (!IsEquipable() || !inventory.CanEquipItem(this))
			return false;
		this.inventory = inventory;
		this.transform.parent = inventory.transform;
		this.transform.localPosition = Vector3.zero;
		this.transform.localRotation = Quaternion.identity;

		if (inventory is InventoryPlayer) {
			state = State.EQUIPPED_FPS;
			collider.enabled = false;
			rigidBody.isKinematic = true;
			rigidBody.detectCollisions = false;
			worldView.gameObject.SetActive(false);
			fpsView = (EquipableFpsView)Instantiate(viewmodelPrefab);
			fpsView.transform.parent = this.transform;
			fpsView.transform.localPosition = Vector3.zero;
			fpsView.transform.localRotation = Quaternion.identity;
		}
		else {
			state = State.EQUIPPED_WORLD;
			rigidBody.isKinematic = true;
		}

		OnEquip(inventory);
		return true;
	}

	public bool Unequip (Inventory inventory) 
	{
		if (!inventory.CanUnequipItem(this))
			return false;
		this.inventory = null;
		this.transform.parent = null;

		state = State.FREE_WORLD;
		collider.enabled = true;
		rigidBody.isKinematic = false;
		rigidBody.detectCollisions = true;
		worldView.gameObject.SetActive(true);
		if (fpsView)
			Destroy(fpsView.gameObject);
		fpsView = null;

		OnUnequip(inventory);
		return true;
	}


	protected abstract void OnEquip (Inventory inventory);

	protected abstract void OnUnequip (Inventory inventory);


	public virtual bool IsEquipable () { return state == State.FREE_WORLD; }

}
