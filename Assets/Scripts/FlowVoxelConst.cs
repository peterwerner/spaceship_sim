using UnityEngine;
using System.Collections;

/**
 *	Like a normal flow voxel but it has a constant atmosphere
 *	Can represent a vacuum (constant atmosphere 0) or a vent (constant source of atmosphere)
 */
public class FlowVoxelConst : FlowVoxel {


	public FlowVoxelConst(Vector3 position, float atmosphere) : base(position, atmosphere) { }


	public override void UpdateNextStep(float timeStep) { /* Do nothing */ }
	public override void StepToNextStep(float timeStep) { /* Do nothing */ }


	public override bool AddNeighbor(FlowVoxel neighbor, bool reciprocate)
	{
		if (reciprocate)
			return neighbor.AddNeighbor(this, false);
		return true;
	}

	public override bool RemoveNeighbor(FlowVoxel neighbor, bool reciprocate)
	{
		if (reciprocate)
			return neighbor.RemoveNeighbor(this, false);
		return true;
	}


	public override void DrawGizmo(Vector3 roomPos) { /* Do nothing */ }

}
