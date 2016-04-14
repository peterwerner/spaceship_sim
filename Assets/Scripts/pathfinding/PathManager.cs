using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {

	public class PathManager : MonoBehaviour {

		[Tooltip ("Used to determine line of sight between nodes upon start.")]
		[SerializeField] LayerMask layermask;
		[Tooltip ("A tolerance of X means that a node will connect to all nodes within X * the distance between the node and its closest neighbor.")]
		[SerializeField] float connectDistTolerance = 1.5f;

		public LayerMask ConnectionLayerMask { get { return layermask; } }
		public float ConnectDistTolerance { get { return connectDistTolerance; } }

		List<AStar.Node> nodes = new List<AStar.Node>();


		void Start ()
		{
			// Connect the initializers
			NodeInitializer.Connect(NodeInitializer.InstanceList.ToArray());
			// Convert initializers to cheaper nodes and destroy the initializers
			foreach (NodeInitializer nodeInit in NodeInitializer.InstanceList) {
				nodeInit.node.neighbors = new AStar.Connection[nodeInit.neighbors.Count];
				for (int i = 0; i < nodeInit.neighbors.Count; i++)
					nodeInit.node.neighbors[i] = new AStar.Connection(nodeInit.neighbors[i].node);
				nodes.Add(nodeInit.node);
			}
			NodeInitializer.DestroyAll();
		}


		/// <summary>Returns the shortest path from start (exclusive) to destination (inclusive)</summary>
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