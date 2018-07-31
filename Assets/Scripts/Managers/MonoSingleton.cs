using System;
using UnityEngine;
/// <summary>
/// 单例基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MonoSingletion<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T instance = null;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<T>();
				if (FindObjectsOfType<T>().Length > 1)
				{
					Debug.Log("More than 1 Manager of type " + typeof(T).Name);
					return instance;
				}

				if (instance == null)
				{
					string instanceName = typeof(T).Name;
					GameObject instanceGO = GameObject.Find(instanceName); // Empty GO
					if (instanceGO == null)
						instanceGO = new GameObject(instanceName);
					instanceGO.AddComponent<T>();
				}
			}
			return instance;
		}
	}
}