using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(OptionManager))]
public class OptionManagerEditor : Editor {
	private Language language;
	private Vector2Int gameResolution;
	private bool fullScreen;
	private float mainVolume;
	private float musicVolume;
	private float gameVolume;
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		language = (Language)EditorGUILayout.EnumPopup("Language", language, GUILayout.ExpandWidth(true));
		gameResolution = EditorGUILayout.Vector2IntField("Resolution", gameResolution, GUILayout.ExpandWidth(true));
		fullScreen = EditorGUILayout.Toggle("UseFullScreen", fullScreen, GUILayout.ExpandWidth(true));
		mainVolume = EditorGUILayout.FloatField("mainVolume", mainVolume, GUILayout.ExpandWidth(true));
		musicVolume = EditorGUILayout.FloatField("musicVolume", musicVolume, GUILayout.ExpandWidth(true));
		gameVolume = EditorGUILayout.FloatField("gameVolume", gameVolume, GUILayout.ExpandWidth(true));
		if (GUILayout.Button("ConfirmOption"))
		{
			OptionManager.Instance.OptionConfirmed();
		}
		if (GUILayout.Button("SaveOption"))
		{
			SaveManager.Instance.SaveGlobalData();
		}
		OptionManager.language = language;
		OptionManager.gameResolutionX = gameResolution.x;
		OptionManager.gameResolutionY = gameResolution.y;
		OptionManager.isFullScreen = fullScreen;
		OptionManager.mainVolume = mainVolume;
		OptionManager.musicVolume = musicVolume;
		OptionManager.gameVolume = gameVolume;
	}
}
