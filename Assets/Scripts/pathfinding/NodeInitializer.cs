using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {

	/// <summary> Exists temporarily to construct A* nodes.
	/// This is destroyed upon start </summary>
	public class NodeInitializer : ListComponent<NodeInitializer> {

		public PathManager manager;
		public List<NodeInitializer> neighbors = new List<NodeInitializer>();
		[HideInInspector] public AStar.Node node;


		void Awake ()
		{
			node = new AStar.Node();
			node.position = transform.position;
		}


		NodeInitializer ClosestNeighbor (out float dist)
		{
			NodeInitializer closest = null;
			float closestSqrDist = Mathf.Infinity;
			foreach (NodeInitializer otherNode in neighbors) {
				if ((otherNode.node.position - this.node.position).sqrMagnitude < closestSqrDist) {
					closest = otherNode;
					closestSqrDist = (otherNode.node.position - this.node.position).sqrMagnitude;
				}
			}
			dist = Mathf.Sqrt(closestSqrDist);
			return closest;
		}


		void OnDrawGizmos ()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(transform.position, 0.1f);
			foreach (NodeInitializer n in neighbors)
				Gizmos.DrawLine(transform.position, n.transform.position);
		}



		/// <summary> Connects the cluster such that the graph is strongly connected (if possible). </summary>
		public static bool Connect (NodeInitializer[] cluster)
		{
			bool addedNewPair;
			while (!IsStronglyConnected(cluster)) {
				// Find the distance between the closest pair of unconnected nodes within line of sight of each other
				float closestDist = Mathf.Infinity;
				foreach (NodeInitializer node in cluster) {
					foreach (NodeInitializer otherNode in cluster) {
						if (
							otherNode != node 
							&& !node.neighbors.Contains(otherNode)
							&& Vector3.Distance(node.node.position, otherNode.node.position) < closestDist
							&& !Physics.Raycast(node.node.position, otherNode.node.position - node.node.position, Vector3.Distance(otherNode.node.position, node.node.position), node.manager.ConnectionLayerMask)
							) 
						{
							closestDist = Vector3.Distance(otherNode.node.position, node.node.position);
						}
					}
				}
				// Connect all pairs of nodes within line of sight and within X * closestDist of each other
				addedNewPair = false;
				foreach (NodeInitializer node in cluster) {
					foreach (NodeInitializer otherNode in cluster) {
						if (
							otherNode != node 
							&& Vector3.Distance(node.node.position, otherNode.node.position) <= node.manager.ConnectDistTolerance * closestDist + float.Epsilon
							&& !node.neighbors.Contains(otherNode)
							&& !Physics.Raycast(node.node.position, otherNode.node.position - node.node.position, node.manager.ConnectDistTolerance * Vector3.Distance(otherNode.node.position, node.node.position), node.manager.ConnectionLayerMask)
						) 
						{
							node.neighbors.Add(otherNode);
							otherNode.neighbors.Add(node);
							addedNewPair = true;
						}
					}
				}
				if (!addedNewPair)
					return false;
			}
			return true;
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