using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

namespace GameMain
{
	public class OptionManager : MonoSingletion<OptionManager>, IManager,ISavable
	{
		public static OptionData optionData=new OptionData();
		private static SaveID saveID;
		public  static Language language = 0;
		public static int gameResolutionX = 1920, gameResolutionY = 1080;
		public static bool isFullScreen = true;
		public static float mainVolume = 1f;
		public static float musicVolume = 1f;
		public static float gameVolume = 1f;
		public void Initial()
		{
			saveID.IDName = "Option";
			saveID.priorityLevel = 1;
			EventManager.Instance.AddListener(GameEventType.RememberGlobalGameData,(object obj1, object obj2) => SaveManager.Instance.CallGlobalDataSave(this, saveID));
			EventManager.Instance.AddListener(GameEventType.ReadGlobalGameData, (object obj1, object obj2) => SaveManager.Instance.CallGlobalDataLoad(this, saveID));
			//SaveManager.Instance.OnRememberGlobalData += () => SaveManager.Instance.CallGlobalDataSave(this, saveID);
			//SaveManager.Instance.OnReadGlobalData+=()=> SaveManager.Instance.CallGlobalDataLoad(this, saveID);
		}
		public void OptionConfirmed()
		{
			optionData.gameResolutionX = gameResolutionX;
			optionData.gameResolutionY = gameResolutionY;
			optionData.language = language;
			optionData.isFullScreen = isFullScreen;
			optionData.mainVolume = mainVolume;
			optionData.musicVolume = musicVolume;
			optionData.gameVolume = gameVolume;
			Debug.Log(gameResolutionX + "," + gameResolutionY);
			EventManager.Instance.PostEvent(GameEventType.OptionConfirm, this,optionData);
		}

		public SaveData OnSave()
		{
			OptionConfirmed();
			Debug.Log(optionData);
			return optionData;
		}

		public void OnLoad(SaveData data)
		{
			OptionData loadData = (OptionData) data;
			Debug.Log(loadData);
			language = loadData.language;
			isFullScreen = loadData.isFullScreen;
			mainVolume = loadData.mainVolume;
			musicVolume = loadData.musicVolume;
			gameVolume = loadData.gameVolume;
			gameResolutionX = loadData.gameResolutionX;
			gameResolutionY = loadData.gameResolutionY;
			OptionConfirmed();
		}

		public void SetResolution(int index)
		{
			switch (index)
			{
				case 0:
					gameResolutionX = 480;
					gameResolutionY = 270;
					break;
				case 1:
					gameResolutionX = 1024;
					gameResolutionY = 576;
					break;
				case 2:
					gameResolutionX = 1280;
					gameResolutionY = 720;
					break;
				case 3:
					gameResolutionX = 1920;
					gameResolutionY = 1080;
					break;
				case 4:
					gameResolutionX = 2560;
					gameResolutionY = 1440;
					break;
				case 5:
					gameResolutionX = 3840;
					gameResolutionY = 2160;
					break;
			}
			OptionConfirmed();
		}

		public void SetLanguage(int index)
		{
			language = (Language)Enum.ToObject(typeof(Language), index);
			OptionConfirmed();
		}
		public void SetFullScreen(bool sta)
		{
				isFullScreen = sta;
			OptionConfirmed();
		}

		public void SetMainVolume(float main)
		{
			mainVolume = main;
			OptionConfirmed();
		}

		public void SetGameVolume(float game)
		{
			gameVolume = game;
			OptionConfirmed();
		}

		public void SetBackgroundVolume(float background)
		{
			musicVolume = background;
			OptionConfirmed();
		}
		public void ConfirmButtonDown()
		{
			SaveManager.Instance.SaveGlobalData();
			OptionConfirmed();
		}

		public void CancelButtonDown()
		{
			SaveManager.Instance.CallGlobalDataLoad(this, saveID);
			OptionConfirmed();
		}
	}
	[Serializable]
	public class OptionData: SaveData,IManagerData
	{
		public Language language = 0;
		public int gameResolutionX = 1920, gameResolutionY = 1080;
		public bool isFullScreen = true;
		public float mainVolume = 1f;
		public float musicVolume = 1f;
		public float gameVolume = 1f;
		public override string ToString()
		{
			return "Language:" + language + "\nResolution:" + gameResolutionX + "," + gameResolutionY + "\nIsFullScreen:" +
			       isFullScreen + "\nMainVolume:" + mainVolume + "\nMusicVolume" + musicVolume + "\nGameVolume" + gameVolume;
		}
	}
}