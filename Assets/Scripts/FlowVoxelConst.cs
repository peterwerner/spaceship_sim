using UnityEngine;
using System.Collections;

/**
 *	Like a normal flow voxel but it has a constant atmosphere
 *	Can represent a vacuum (constant atmosphere 0) or a vent (constant source of atmosphere)
 */
public class FlowVoxelConst : FlowVoxel {


	public FlowVoxelConst(Vector3 position, float atmosphere) : base(position, atmosphere) { }


	public override void UpdateNextStep(float timeStep)
	{
		Vector3 netDiff = Vector3.zero;
		foreach (FlowVoxel neighbor in neighbors) 
		{
			float diff = neighbor.Atmosphere - atmosphere;	// positive diff = inflow
			netDiff += diff * (position - neighbor.Position);
		}
		if (neighbors.Count > 0)
			flow = FlowVoxelManager.FlowVectorConstant * timeStep * netDiff / neighbors.Count;
	}

	public override void StepToNextStep(float timeStep) { /* Do nothing */ }

}
