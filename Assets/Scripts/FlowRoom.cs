using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(BoxCollider))]
public class FlowRoom : MonoBehaviour {

	[SerializeField] [Tooltip("0 is vacuum, 1 is 1 atm")] [Range(0, 1)] float atmosphereStart = 1;

	float atmosphere;
	new BoxCollider collider;
	FlowVoxel[ , , ] voxels = new FlowVoxel[0, 0, 0];

	public float Atmosphere	{ get { return atmosphere; } }


	void Start() 
	{
		collider = (BoxCollider)GetComponent(typeof(BoxCollider));
		atmosphere = atmosphereStart;
		// Construct voxels - fill the collider's bounds
		float voxelSize = FlowVoxelManager.Radius * 2;
		Vector3 colliderSize = Vector3.Scale(collider.size, transform.lossyScale);
		Vector3 cornerLo = transform.position - 0.5f * colliderSize;
		Vector3 cornerHi = transform.position + 0.5f * colliderSize;
		Vector3 numVoxels = (cornerHi - cornerLo) / voxelSize;
		voxels = new FlowVoxel[(int)numVoxels.x, (int)numVoxels.y, (int)numVoxels.z];
		float x, y, z;
		int i, j, k;
		for (i = 0, x = cornerLo.x;   i < voxels.GetLength(0);   i++, x += voxelSize) {
			for (j = 0, y = cornerLo.y;   j < voxels.GetLength(1);   j++, y += voxelSize) {
				for (k = 0, z = cornerLo.z;   k < voxels.GetLength(2);   k++, z += voxelSize) 
				{
					voxels[i, j, k] = new FlowVoxel(new Vector3(x,y,z), atmosphereStart);
					if (i < k)
						voxels[i, j, k] = new FlowVoxel(new Vector3(x,y,z), 0);
					if (i > 0)
						voxels[i, j, k].AddNeighbor(voxels[i-1, j, k], true);
					if (j > 0)
						voxels[i, j, k].AddNeighbor(voxels[i, j-1, k], true);
					if (k > 0)
						voxels[i, j, k].AddNeighbor(voxels[i, j, k-1], true);
				}
			}
		}
	}


	void FixedUpdate()
	{
		atmosphere = 0;
		foreach (FlowVoxel voxel in voxels)
			atmosphere += voxel.Atmosphere;
		atmosphere /= voxels.Length;
	}


	// Display the gizmo in the editor - this doesn't affect the actual game
	void OnDrawGizmos()
	{
		foreach (FlowVoxel voxel in voxels)
			voxel.DrawGizmo(this.transform.position);
		Gizmos.color = new Color(1, Mathf.Min(1, atmosphere), Mathf.Min(1, atmosphere));
		if (collider)
			Gizmos.DrawWireCube(transform.position, Vector3.Scale(collider.size, transform.lossyScale));
	}

}
