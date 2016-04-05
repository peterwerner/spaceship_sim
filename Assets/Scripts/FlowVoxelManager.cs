using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowVoxelManager : SingletonComponent<FlowVoxelManager> {

	[SerializeField] [Range(0, 1)] float ambientAtmosphere = 0;
	[SerializeField] [Range(0.5f, 4)] float voxelWidth = 1;
	[SerializeField] [Tooltip("linearly scales the flow vector")] 
	float flowVectorConstant = 1000;
	[SerializeField] [Tooltip("linearly scales the flow force")] 
	float flowForceConstant = 5;
	[SerializeField] [Tooltip("linearly scales the flow rate. upper limited to number of physics updates per second")] 
	float flowRateConstant = 50;

	float radius;
	List<FlowVoxel> flowVoxels = new List<FlowVoxel>();

	public static float FlowVectorConstant { get { return FlowVoxelManager.Instance.flowVectorConstant; } }
	public static float FlowForceConstant { get { return FlowVoxelManager.Instance.flowForceConstant; } }
	public static float FlowRateConstant { get { return FlowVoxelManager.Instance.flowRateConstant; } }
	public static float Radius { get { return FlowVoxelManager.Instance.radius; } }
	public static float AmbientAtmosphere { get { return FlowVoxelManager.Instance.ambientAtmosphere; } }
	public static List<FlowVoxel> FlowVoxels { get { return FlowVoxelManager.Instance.flowVoxels; } }



	override protected void Awake()
	{
		base.Awake();	
		radius = voxelWidth / 2;
	}


	void FixedUpdate () 
	{
		radius = voxelWidth / 2;
	}


	void OnDrawGizmos ()
	{
		foreach (FlowVoxel voxel in flowVoxels)
			voxel.DrawGizmo();
	}

}
