using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AtmoFlowSim {
	
/// <summary>Singleton object used to give in-editor control over various constants.</summary>
public class FlowSimManager : SingletonComponent<FlowSimManager> {

	static float voxelWidth = 1.3f;

	[SerializeField] [Range(0, 1)] float ambientAtmosphere = 0;
	[SerializeField] [Tooltip("linearly scales the flow force in full simulation")] 
	float flowForceConstant = 5;
	[SerializeField] [Tooltip("linearly scales the flow rate in full simulation. upper limited to number of physics updates per second")] 
	float flowRateConstant = 50;
	[SerializeField] [Tooltip("linearly scales the flow force in cheap simulation")] 
	float cheapFlowForceConstant = 5;
	[SerializeField] [Tooltip("linearly scales the rate of atmosphere change in cheap simulation")] 
	float cheapAtmoDeltaConstant = 1;

	float flowVectorConstant = 1000;	// Scales the flow vector and flow force in full simulation

	public static float FlowVectorConstant { get { return FlowSimManager.Instance.flowVectorConstant; } }
	public static float FlowForceConstant { get { return FlowSimManager.Instance.flowForceConstant; } }
	public static float FlowRateConstant { get { return FlowSimManager.Instance.flowRateConstant; } }
	public static float CheapFlowForceConstant { get { return FlowSimManager.Instance.cheapFlowForceConstant; } }
	public static float CheapAtmoDeltaConstant { get { return FlowSimManager.Instance.cheapAtmoDeltaConstant; } }
	public static float Radius { get { return voxelWidth * 0.5f; } }
	public static float AmbientAtmosphere { get { return FlowSimManager.Instance.ambientAtmosphere; } }

}

}