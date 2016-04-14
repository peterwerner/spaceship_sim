using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Pathfinding {

	[CustomEditor(typeof(PathManagerEditor))]
	public class BuilderEditor : Editor {

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();
	
			PathManagerEditor pathManager = (PathManagerEditor)target;

			GUILayout.Label("Build a navigation graph. This may take a long time for graphs with a lot of nodes.");
			if (GUILayout.Button("Build navigation graph")) {
				pathManager.Build();
			}

			GUILayout.Label("Stops the current build but does not destroy the graph.");
			if (GUILayout.Button("Stop building navigation graph")) {
				pathManager.PauseBuild();
			}

			GUILayout.Label("Destroy the current navigation graph");
			if (GUILayout.Button("Destroy navigation graph")) {
				pathManager.UnBuild();
			}

		}
			
	}

}