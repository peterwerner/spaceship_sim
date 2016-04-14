using UnityEngine;
using System.Collections.Generic;

public abstract class ListComponent<T> : MonoBehaviour where T : MonoBehaviour
{
	public static List<T> InstanceList = new List<T>();

	protected virtual void OnEnable()
	{
		InstanceList.Add( this as T );
	}

	protected virtual void OnDisable()
	{
		InstanceList.Remove( this as T );
	}

	public static void DestroyAll()
	{
		T[] ListClone = new T[InstanceList.Count];
		InstanceList.CopyTo(ListClone);
		foreach (T instance in ListClone)
			Destroy(instance.gameObject);
	}

}