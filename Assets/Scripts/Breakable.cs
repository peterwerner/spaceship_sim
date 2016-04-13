using UnityEngine;
using System.Collections;

public class Breakable : Affectable {

	[SerializeField] float damageThreshold = 20f;
	[SerializeField] float forceThreshold = 20f;
	[SerializeField] AtmoFlowSim.FlowConnector connector;


	public override void Damage(float damage, float force, RaycastHit hit, Vector3 direction)
	{
		if (damage >= damageThreshold || force >= forceThreshold)
			Break();
	}


	void Break() 
	{
		connector.Open();
		Destroy(this.gameObject);
	}

}
