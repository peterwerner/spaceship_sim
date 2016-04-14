using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {

	[ExecuteInEditMode]
	public class PathManager : MonoBehaviour {

		[Tooltip ("Used to determine line of sight between nodes upon start.")]
		[SerializeField] LayerMask layermask;
		[Tooltip ("A tolerance of X means that a node will connect to all nodes within X * the distance between the node and its closest neighbor.")]
		[SerializeField] float connectDistTolerance = 1.5f;
		[Tooltip ("The maximum number of operations performed in each frame when building the navigation graph")]
		[SerializeField] int buildOpsPerFrame = 1000;
		bool isBuilding = false;

		public LayerMask ConnectionLayerMask { get { return layermask; } }
		public float ConnectDistTolerance { get { return connectDistTolerance; } }
		public int BuildOpsPerFrame { get { return buildOpsPerFrame; } }
		public Color GizmoColor { get { return isBuilding ? Color.red : Color.blue; } }

		public List<AStar.Node> nodes = new List<AStar.Node>();
		IEnumerator buildRoutine = null;



		// Hacky way of updating the build routine each time the editor re-renders
		void OnRenderObject ()
		{
			if (buildRoutine != null) {
				isBuilding = true;
				if (!buildRoutine.MoveNext()) {
					isBuilding = false;
					buildRoutine = null;
				}
			}
		}


		/// <summary> Used in edit mode to build the navigation graph </summary>
		public void Build ()
		{
			// Connect the initializers
			NodeInitializer[] cluster = NodeInitializer.InstanceList.ToArray();
			if (cluster.Length > 0) {
				buildRoutine = cluster[0].ConnectRoutine(cluster);
			}
		}

		/// <summary> Used in edit mode to destroy the navigation graph </summary>
		public void UnBuild ()
		{
			if (buildRoutine != null)
				StopCoroutine(buildRoutine);
			foreach (NodeInitializer nodeInit in NodeInitializer.InstanceList) {
				nodeInit.node = null;
				nodeInit.neighbors.Clear();
			}
			nodes.Clear();
			isBuilding = false;
		}
			


		/// <summary> Returns the shortest path from start (exclusive) to destination (inclusive) </summary>
		public List<Vector3> GetShortestPath (Vector3 start, Vector3 destination) 
		{
			return AStar.GetShortestPath(GetClosestNode(start), GetClosestNode(destination));
		}


		AStar.Node GetClosestNode (Vector3 point)
		{
			AStar.Node closest = null;
			float sqrDist, closestSqrDist = Mathf.Infinity;
			foreach (AStar.Node node in nodes) {
				sqrDist = (node.position - point).sqrMagnitude;
				if (sqrDist < closestSqrDist) {
					closestSqrDist = sqrDist;
					closest = node;
				}
			}
			return closest;
		}


		void OnDrawGizmos ()
		{
			Color color = Color.cyan;
			color.a = 0.2f;
			Gizmos.color = color;
			foreach (AStar.Node node in nodes) {
				Gizmos.DrawSphere(node.position, 0.1f);
				foreach (AStar.Connection n in node.neighbors)
					Gizmos.DrawLine(node.position, n.otherNode.position);
			}
		}

	}

}