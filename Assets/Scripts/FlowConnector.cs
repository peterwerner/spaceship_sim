using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowConnector : MonoBehaviour {

	public bool isOpen = false; 

	FlowVoxel[][] pairs;


	public bool Connect(Room A, Room B)
	{
		return false;
	}


	public void Open() 
	{ 
		isOpen = true;
	}

	public void Close()
	{
		isOpen = false;
	}

}
