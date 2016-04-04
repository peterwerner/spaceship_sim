﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowVoxel {

	protected float atmosphere, atmosphereNext;
	protected Vector3 flow, position;
	protected List<FlowVoxel> neighbors = new List<FlowVoxel>();

	public float Atmosphere	{ get { return atmosphere; } }
	public Vector3 Flow	{ get { return flow; } }
	public Vector3 Position	{ get { return position; } }


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
		float weightBaseline = 0.0001f;
		float weightedNetAtmo = 0;
		float sumWeights = 0;
		Vector3 netDiff = Vector3.zero;
		foreach (FlowVoxel neighbor in neighbors) 
		{
			float weight = weightBaseline + neighbor.flow.magnitude;
			weightedNetAtmo += weight * neighbor.atmosphere;
			sumWeights += weight;
			float diff = neighbor.atmosphere - atmosphere;	// positive diff = inflow
			netDiff += diff * (position - neighbor.position);
		}
		flow = FlowVoxelManager.FlowVectorConstant * timeStep * netDiff / neighbors.Count;
		float targetAtmo = weightedNetAtmo / sumWeights;
		if (float.IsNaN(flow.x)) {		// Edge case: no neighbors results in NaN's
			flow = Vector3.zero;		//			  default to 0 vector
			targetAtmo = atmosphere;	//			  default to self
		}
		float m = Mathf.Min(1, FlowVoxelManager.FlowRateConstant * timeStep);
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
	public void DrawGizmo(Vector3 roomPos) 
	{
		Gizmos.color = new Color(1, Mathf.Min(1, atmosphere), Mathf.Min(1, atmosphere));
		//Gizmos.DrawWireCube(position, FlowVoxelManager.Radius * 2 * Vector3.one);
		Gizmos.DrawWireCube(position, FlowVoxelManager.Radius * 0.2f * Vector3.one);
		Gizmos.DrawLine(position, position + flow);
	}

}
