using UnityEngine;
using System.Collections;

/**
 *	Like a normal flow voxel but it has a constant atmosphere
 *	Can represent a vacuum (constant atmosphere 0) or a vent (constant source of atmosphere)
 */
public class FlowVoxelConst : FlowVoxel {

	float constantAtmo = 0;
	bool useAmbientAtmo = false;


	public FlowVoxelConst(Vector3 position, float atmosphere) : base(position, atmosphere) 
	{ 
		constantAtmo = atmosphere;
	}

	/// <summary>Assumes the ambient atmosphere value set in the FlowSimManager.</summary>
	public FlowVoxelConst(Vector3 position) : base(position, FlowSimManager.AmbientAtmosphere) 
	{
		useAmbientAtmo = true;
	}


	public override void StepToNextStep(float timeStep) { /* Do nothing - never update atmosphere value */ }


	public override float GetAtmosphere() { return useAmbientAtmo ? FlowSimManager.AmbientAtmosphere : constantAtmo; }

	public override void SetAtmosphere(float value) { /* Do nothing */ }

}
