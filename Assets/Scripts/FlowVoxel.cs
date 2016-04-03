using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowVoxel {

	protected float atmosphere, atmosphereNext;
	protected Vector3 flow, position;
	List<FlowVoxel> neighbors = new List<FlowVoxel>();

	public float Atmosphere	{ get { return atmosphere; } }


	public FlowVoxel(Vector3 position, float atmosphere)
	{
		this.atmosphere = atmosphere;
		this.atmosphereNext = atmosphere;
		this.flow = Vector3.zero;
		this.position = position;
		FlowVoxelManager.FlowVoxels.Add(this);
	}

	~FlowVoxel()
	{
		foreach (FlowVoxel neighbor in neighbors) 
			neighbor.RemoveNeighbor(this, false);
		FlowVoxelManager.FlowVoxels.Remove(this);
	}


	public virtual void UpdateNextStep(float timeStep)
	{
		float netAtmo = 0;
		Vector3 netFlow = Vector3.zero;
		foreach (FlowVoxel neighbor in neighbors) 
		{
			netAtmo += neighbor.Atmosphere;
			float diff = neighbor.Atmosphere - atmosphere;	// positive diff = inflow
			netFlow += diff * (position - neighbor.position);
		}
		flow = FlowVoxelManager.FlowVectorConstant * timeStep * netFlow / neighbors.Count;
		float targetAtmo = netAtmo / neighbors.Count;
		float m = Mathf.Min(1, FlowVoxelManager.FlowRateConstant * timeStep) 
				* (1 - Mathf.Pow(0.5f, FlowVoxelManager.FlowRateExponential));
		atmosphereNext = (1-m) * atmosphere + m * targetAtmo;
	}
	public virtual void StepToNextStep(float timeStep)
	{
		atmosphere = atmosphereNext;
	}


	public virtual bool AddNeighbor(FlowVoxel neighbor, bool reciprocate)
	{
		if (neighbor == null || neighbor == this || neighbors.Contains(neighbor))
			return false;
		neighbors.Add(neighbor);
		if (reciprocate)
			return neighbor.AddNeighbor(this, false);
		return true;
	}

	public virtual bool RemoveNeighbor(FlowVoxel neighbor, bool reciprocate)
	{
		if (!neighbors.Remove(neighbor))
			return false;
		if (reciprocate)
			return neighbor.RemoveNeighbor(this, false);
		return true;
	}


	// Display the gizmo in the editor - this doesn't affect the actual game
	public virtual void DrawGizmo(Vector3 roomPos) 
	{
		Gizmos.color = new Color(1, Mathf.Min(1, atmosphere), Mathf.Min(1, atmosphere));
		//Gizmos.DrawWireCube(position, FlowVoxelManager.Radius * 2 * Vector3.one);
		Gizmos.DrawWireCube(position, FlowVoxelManager.Radius * 0.2f * Vector3.one);
		Gizmos.DrawLine(position, position + flow);
	}

}
