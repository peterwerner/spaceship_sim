using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Animator))]
public class EquipableFpsView : MonoBehaviour {

	Animator animator;

	public Animator Anim { get { return animator; } }


	void Awake () {
		animator = GetComponentInChildren<Animator>();
	}

}
