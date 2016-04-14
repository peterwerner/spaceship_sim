using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Pathfinding {

	public class AStar : MonoBehaviour {

		public class Node {
			public Vector3 position;
			public Node predecessor = null;
			public float f = 0, g = 0;
			public Connection[] neighbors = new Connection[0];
		}
		public class Connection {
			public bool isEnabled = true;
			public Node otherNode;
			public Connection (Node otherNode) { this.otherNode = otherNode; }
		}


		/// <summary>Returns the shortest path from start (exclusive) to destination (inclusive)</summary>
		public static List<Vector3> GetShortestPath (Node start, Node destination) 
		{
			if (start == destination)
				return new List<Vector3>();
			List<Node> open = new List<Node>();
			List<Node> closed = new List<Node>();
			open.Add(start);
			Node current = null, successor = null;
			while (open.Count > 0) 
			{
				// Take the least-cost open node
				current = null;
				foreach (Node prospective in open)
					if (current == null || prospective.f < current.f)
						current = prospective;
				// Recontruct the path and return if we have reached destination
				if (current == destination)
					return GetPathList(start, destination);
				// Update all successor nodes
				foreach (Connection successorConnection in current.neighbors) {
					if (!successorConnection.isEnabled)
						continue;
					successor = successorConnection.otherNode;
					// If successor is already fully evaluated, skip it
					if (closed.Contains(successor))
						continue;
					// Calculate but do not store successor's prior cost
					float g_prospective = current.g + Vector3.Distance(successor.position, current.position);
					// If the node has not been discovered, add it to open
					if (!open.Contains(successor))
						open.Add(successor);
					// If successor has been discovered but not fully evaluated, update it IFF we can improve its cost
					else if (g_prospective >= successor.g) 
						continue;
					// This is the current best path, update successor accordingly
					successor.predecessor = current;
					successor.g = g_prospective;
					successor.f = successor.g + Vector3.Distance(successor.position, destination.position);
				}
				open.Remove(current);
				closed.Add(current);
			}
			return null;
		}
		static List<Vector3> GetPathList (Node start, Node destination)
		{
			List<Vector3> path = new List<Vector3>();
			Node current = destination;
			while (current != start) {
				path.Add(current.position);
				current = current.predecessor;
			}
			path.Reverse();
			return path;
		}

	}

}