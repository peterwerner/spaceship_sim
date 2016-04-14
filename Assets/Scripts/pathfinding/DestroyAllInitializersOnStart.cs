using UnityEngine;
using System.Collections;

namespace Pathfinding {

	public class DestroyAllInitializersOnStart : MonoBehaviour {

		void Start ()
		{
			// Destroy the initializers. Assumes the navigation graph has already been buit in the editor.
			NodeInitializer.DestroyAll();
		}
	}

}