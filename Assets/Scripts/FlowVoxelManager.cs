using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowVoxelManager : SingletonComponent<FlowVoxelManager> {

	[SerializeField] [Range(0, 1)] float ambientAtmosphere = 0;
	[SerializeField] [Range(0.1f, 4)] float voxelWidth = 1;
	[SerializeField] [Tooltip("linearly scales the flow vector")] 
	float flowVectorConstant = 50;
	[SerializeField] [Tooltip("linearly scales the flow rate. upper limited to number of physics updates per second")] 
	float flowRateConstant = 50;
	[SerializeField] [Tooltip("exponentially scales the flow rate")] 
	float flowRateExponential = 10;

	float radius;
	List<FlowVoxel> flowVoxels = new List<FlowVoxel>();
	FlowVoxel ambientFlowVoxel;

	public static float FlowVectorConstant { get { return FlowVoxelManager.Instance.flowVectorConstant; } }
	public static float FlowRateConstant { get { return FlowVoxelManager.Instance.flowRateConstant; } }
	public static float FlowRateExponential { get { return FlowVoxelManager.Instance.flowRateExponential; } }
	public static float Radius { get { return FlowVoxelManager.Instance.radius; } }
	public static List<FlowVoxel> FlowVoxels { get { return FlowVoxelManager.Instance.flowVoxels; } }


	override protected void Awake()
	{
		base.Awake();	
		radius = voxelWidth / 2;
		ambientFlowVoxel = new FlowVoxelConst(Vector3.zero, ambientAtmosphere);
	}


	void FixedUpdate () 
	{
		radius = voxelWidth / 2;
		foreach (FlowVoxel voxel in FlowVoxels)
			voxel.UpdateNextStep(Time.fixedDeltaTime);
		foreach (FlowVoxel voxel in FlowVoxels)
			voxel.StepToNextStep(Time.fixedDeltaTime);
	}

}
