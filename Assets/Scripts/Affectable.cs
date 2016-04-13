using UnityEngine;
using System.Collections;

/// <summary>
/// Apply various effects, including damage
/// </summary>
public class Affectable : MonoBehaviour {

	public virtual void Damage(float damage, float force, RaycastHit hit, Vector3 direction) { }

	public virtual void Use() { }

}
