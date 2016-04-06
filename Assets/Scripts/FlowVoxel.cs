using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowVoxel {

	float atmosphere, atmosphereNext;
	protected Vector3 flow, position;
	protected List<FlowVoxel> neighbors = new List<FlowVoxel>();

	public Vector3 Flow	{ get { return flow; } set { flow = value; } }
	public Vector3 Position	{ get { return position; } }


	public FlowVoxel(Vector3 position, float atmosphere)
	{
		this.atmosphere = atmosphere;
		this.atmosphereNext = atmosphere;
		this.flow = Vector3.zero;
		this.position = position;
	}

	~FlowVoxel()
	{
		foreach (FlowVoxel neighbor in neighbors) 
			neighbor.RemoveNeighbor(this, false);
	}


	/// <summary>Calculates this' Atmosphere value for the next time step, but does not store it in Atmosphere.</summary>
	public void UpdateNextStep(float timeStep)
	{
		float weightBaseline = 0.0001f;
		float weightedNetAtmo = 0;
		float sumWeights = 0;
		Vector3 weightedNetFlow = Vector3.zero;
		foreach (FlowVoxel neighbor in neighbors) 
		{
			float weight = weightBaseline + neighbor.flow.magnitude;
			weightedNetAtmo += weight * neighbor.GetAtmosphere();
			sumWeights += weight;
			float diff = neighbor.GetAtmosphere() - atmosphere;	// positive diff = inflow
			weightedNetFlow += weight * diff * (position - neighbor.position);
		}
		flow = FlowSimManager.FlowVectorConstant * timeStep * weightedNetFlow / sumWeights;
		float targetAtmo = weightedNetAtmo / sumWeights;
		if (float.IsNaN(flow.x)) {		// Edge case: no neighbors results in NaN's
			flow = Vector3.zero;		//			  default to 0 vector
			targetAtmo = atmosphere;	//			  default to self
		}
		float m = Mathf.Min(1, FlowSimManager.FlowRateConstant * timeStep);
		atmosphereNext = (1-m) * atmosphere + m * targetAtmo;
	}
	/// <summary>Stores the last value calculated by UpdateNextStep in Atmosphere.</summary>
	public virtual void StepToNextStep(float timeStep)
	{
		atmosphere = atmosphereNext;
	}


	/// <summary>Neighbor voxels are used to calculate atmosphere flow</summary>
	public virtual bool AddNeighbor(FlowVoxel neighbor, bool reciprocate)
	{
		if (neighbor == null || neighbor == this || neighbors.Contains(neighbor))
			return false;
		neighbors.Add(neighbor);
		if (reciprocate)
			return neighbor.AddNeighbor(this, false);
		return true;
	}
	/// <summary>Neighbor voxels are used to calculate atmosphere flow</summary>
	public virtual bool RemoveNeighbor(FlowVoxel neighbor, bool reciprocate)
	{
		if (!neighbors.Remove(neighbor))
			return false;
		if (reciprocate)
			return neighbor.RemoveNeighbor(this, false);
		return true;
	}


	public virtual float GetAtmosphere() { return atmosphere; }

	public virtual void SetAtmosphere(float value) { atmosphere = value; }


	// Display the gizmo in the editor - this doesn't affect the actual game
	public void DrawGizmo() 
	{
		Gizmos.color = new Color(1, Mathf.Min(1, this.GetAtmosphere()), Mathf.Min(1, this.GetAtmosphere()));
		//Gizmos.DrawWireCube(position, FlowVoxelManager.Radius * 2 * Vector3.one);
		Gizmos.DrawWireCube(position, FlowSimManager.Radius * 0.2f * Vector3.one);
		Gizmos.DrawLine(position, position + flow);
	}

}
