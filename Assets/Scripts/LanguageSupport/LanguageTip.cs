using System;
using System.Collections.Generic;
using System.Reflection;
using GameMain;
using UnityEngine;
using UnityEngine.Events;

namespace LanguageSupport
{
	public class LanguageTip : MonoBehaviour
	{
		public string StringName;
		public Component mono;
		public Dictionary<string, PropertyInfo> propertyInfos = new Dictionary<string, PropertyInfo>();
		[HideInInspector]
		public string infokey;
		public void GetMonoProperties()
		{
			propertyInfos.Clear();
			Type t = mono.GetType();
			foreach (var pi in t.GetProperties())
			{
				if (!pi.IsDefined(typeof(ObsoleteAttribute), true)&&pi.CanWrite)
				{
					var value = pi.GetValue(mono, null);
					if (value != null && value is string)
					{
						if (!propertyInfos.ContainsKey(pi.Name))
							propertyInfos.Add(pi.Name, pi);
					}
				}
			}
		}

		public void Awake()
		{
			EventManager.Instance.AddListener<object,object>(GameEventType.OptionConfirm, (sender, data) =>
				{
				Debug.Log("<color=red>Work</color>");
				GetMonoProperties();
				propertyInfos[infokey].SetValue(mono, LanguageManager.Instance.GetString(StringName), null);
				}
				);
		}
		public void Start()
		{
			GetMonoProperties();
			propertyInfos[infokey].SetValue(mono, LanguageManager.Instance.GetString(StringName), null);
		}
	}
}
