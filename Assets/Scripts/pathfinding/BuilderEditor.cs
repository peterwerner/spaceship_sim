using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Pathfinding {

	[CustomEditor(typeof(PathManager))]
	public class BuilderEditor : Editor {

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();
	
			PathManager pathManager = (PathManager)target;

			GUILayout.Label("Build a navigation graph. This may take a long time for graphs with a lot of nodes.");
			if (GUILayout.Button("Build navigation graph")) {
				pathManager.Build();
			}

			GUILayout.Label("Destroy the current navigation graph");
			if (GUILayout.Button("Destroy navigation graph")) {
				pathManager.UnBuild();
			}

		}
			
	}

}