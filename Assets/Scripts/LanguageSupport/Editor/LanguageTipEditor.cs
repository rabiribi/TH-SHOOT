using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LanguageSupport;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(LanguageTip))]
public class LanguageTipEditor : Editor
{
	private int index = 0;
	private string[] keys;
	private LanguageTip lastTip = null;
	public override void OnInspectorGUI()
	{

		DrawDefaultInspector();
		LanguageTip tip = (LanguageTip) target;
		if (tip.mono == null)
		{
			keys = tip.propertyInfos.Keys.ToArray();
			index = EditorGUILayout.Popup(index, keys);
			return;
		}
		tip.GetMonoProperties();
		keys = tip.propertyInfos.Keys.ToArray();
		index = EditorGUILayout.Popup(index, keys);
		tip.infokey = keys[index];
	}
}
