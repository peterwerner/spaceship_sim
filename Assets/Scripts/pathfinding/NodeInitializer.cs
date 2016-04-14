using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {

	/// <summary> Exists temporarily to construct A* nodes.
	/// This is destroyed upon start </summary>
	[ExecuteInEditMode]
	public class NodeInitializer : ListComponent<NodeInitializer> {

		public PathManager manager;
		public List<NodeInitializer> neighbors = new List<NodeInitializer>();
		[HideInInspector] public AStar.Node node;



		void OnDrawGizmos ()
		{
			Gizmos.color = manager.GizmoColor;
			Gizmos.DrawSphere(transform.position, 0.1f);
			foreach (NodeInitializer n in neighbors) 
				Gizmos.DrawLine(transform.position, n.transform.position);
		}



		/// <summary> Connects the nodes to form a navigation graph / navmesh. Returns true iff the graph is strongly connected. </summary>
		public IEnumerator ConnectRoutine (NodeInitializer[] cluster)
		{
			int opCount = 0;
			bool addedNewPair = true;
			while (!IsStronglyConnected(cluster) && addedNewPair) {
				// Find the distance between the closest pair of unconnected nodes within line of sight of each other
				float closestDist = Mathf.Infinity;
				foreach (NodeInitializer node in cluster) {
					foreach (NodeInitializer otherNode in cluster) {
						if (
							otherNode != node 
							&& !node.neighbors.Contains(otherNode)
							&& Vector3.Distance(node.transform.position, otherNode.transform.position) < closestDist
							&& !Physics.Raycast(node.transform.position, otherNode.transform.position - node.transform.position, Vector3.Distance(otherNode.transform.position, node.transform.position), node.manager.ConnectionLayerMask)
							) 
						{
							closestDist = Vector3.Distance(otherNode.transform.position, node.transform.position);
						}
						// Wait for the next frame if we have exceeeded our operations per frame quota
						opCount++;
						if (opCount >= manager.BuildOpsPerFrame) {
							opCount = 0;
							yield return null;
						}
					}
				}
				// Connect all pairs of nodes within line of sight and within X * closestDist of each other
				addedNewPair = false;
				foreach (NodeInitializer node in cluster) {
					foreach (NodeInitializer otherNode in cluster) {
						if (
							otherNode != node 
							&& Vector3.Distance(node.transform.position, otherNode.transform.position) <= node.manager.ConnectDistTolerance * closestDist + float.Epsilon
							&& !node.neighbors.Contains(otherNode)
							&& !Physics.Raycast(node.transform.position, otherNode.transform.position - node.transform.position, node.manager.ConnectDistTolerance * Vector3.Distance(otherNode.transform.position, node.transform.position), node.manager.ConnectionLayerMask)
						) 
						{
							node.neighbors.Add(otherNode);
							otherNode.neighbors.Add(node);
							addedNewPair = true;
						}
						// Wait for the next frame if we have exceeeded our operations per frame quota
						opCount++;
						if (opCount >= manager.BuildOpsPerFrame) {
							opCount = 0;
							yield return null;
						}
					}
				}
			}
			// Once the graph is connected, construct the cheap a* nodes corresponding to each initializer node
			foreach (NodeInitializer nodeInit in cluster) {
				nodeInit.node = new AStar.Node();
				nodeInit.node.position = nodeInit.transform.position;
				manager.nodes.Add(nodeInit.node);
				// Wait for the next frame if we have exceeeded our operations per frame quota
				opCount++;
				if (opCount >= manager.BuildOpsPerFrame) {
					opCount = 0;
					yield return null;
				}
			}
			foreach (NodeInitializer nodeInit in cluster) {
				nodeInit.node.neighbors = new AStar.Connection[nodeInit.neighbors.Count];
				for (int i = 0; i < nodeInit.neighbors.Count; i++) {
					nodeInit.node.neighbors[i] = new AStar.Connection(nodeInit.neighbors[i].node);
				}
				// Wait for the next frame if we have exceeeded our operations per frame quota
				opCount++;
				if (opCount >= manager.BuildOpsPerFrame) {
					opCount = 0;
					yield return null;
				}
			}
		}


		/// <summary> Returns true if every node has a path to every other node.
		/// Assumes all connections are undirected. </summary>
		static bool IsStronglyConnected (NodeInitializer[] cluster)
		{
			if (cluster.Length <= 1)
				return true;
			// Run breadth-first-search either until we have seen every node or
			// until we run out of nodes to explore
			List<NodeInitializer> notYetFound = new List<NodeInitializer>(cluster);
			List<NodeInitializer> frontier = new List<NodeInitializer>(), frontierNext;
			frontier.Add(cluster[0]);
			while (frontier.Count > 0) {
				foreach (NodeInitializer node in frontier)
					notYetFound.Remove(node);
				if (notYetFound.Count == 0)
					return true;
				frontierNext = new List<NodeInitializer>();
				foreach (NodeInitializer node in frontier) 
					foreach (NodeInitializer neighbor in node.neighbors) 
						if (notYetFound.Contains(neighbor))
							frontierNext.Add(neighbor);
				frontier = frontierNext;
			}
			return false;
		}

	}

}