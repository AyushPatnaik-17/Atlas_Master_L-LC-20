using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class hasComponent
{
	/// <summary>
	/// Custom extension function to checke if a gaemObejct has a particular component attached to it
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="flag"></param>
	/// <returns></returns>
	public static bool HasComponent<T>(this GameObject flag) where T : Component
	{
		return flag.GetComponent<T>() != null;
	}
}
