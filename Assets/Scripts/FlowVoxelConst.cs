using UnityEngine;
using System.Collections;

/**
 *	Like a normal flow voxel but it has a constant atmosphere
 *	Can represent a vacuum (constant atmosphere 0) or a vent (constant source of atmosphere)
 */
public class FlowVoxelConst : FlowVoxel {


	public FlowVoxelConst(Vector3 position, float atmosphere) : base(position, atmosphere) { }

	public override void StepToNextStep(float timeStep) { /* Do nothing - never update atmosphere value */ }

}
