using UnityEngine;
using System.Collections;

/**
 *	Like a normal flow voxel but it has a constant atmosphere
 *	Can represent a vacuum (constant atmosphere 0) or a vent (constant source of atmosphere)
 */
public class FlowVoxelConst : FlowVoxel {

	float constantAtmo = 0;
	bool useAmbientAtmo = false;
	public override float GetAtmosphere() { return useAmbientAtmo ? FlowVoxelManager.AmbientAtmosphere : constantAtmo; }


	public FlowVoxelConst(Vector3 position, float atmosphere) : base(position, atmosphere) 
	{ 
		constantAtmo = atmosphere;
	}

	// If no constant atmosphere value is provided, use the ambient atmosphere
	public FlowVoxelConst(Vector3 position) : base(position, FlowVoxelManager.AmbientAtmosphere) 
	{
		useAmbientAtmo = true;
	}


	public override void StepToNextStep(float timeStep) { /* Do nothing - never update atmosphere value */ }


}
