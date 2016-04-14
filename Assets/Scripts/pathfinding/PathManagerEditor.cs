using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {

	[ExecuteInEditMode]
	public class PathManagerEditor : MonoBehaviour {

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
			PauseBuild();
			foreach (NodeInitializer nodeInit in NodeInitializer.InstanceList) {
				nodeInit.node = null;
				nodeInit.neighbors.Clear();
			}
		}

		public void PauseBuild ()
		{
			if (buildRoutine != null)
				buildRoutine = null;
			isBuilding = false;
		}

	}

}