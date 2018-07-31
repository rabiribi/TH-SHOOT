using System;
using LanguageSupport;
using UnityEngine;

namespace GameMain
{
	public class GameManager : MonoSingletion<GameManager>, IManager
	{
		public static string GameName;
		public static string GameVersion = "1.0.0";
		public static string GameInfo ="";
	//	public bool i;
		public void Awake()
		{
			Initial();
		}

		public void Initial()
		{
			//订阅设置更改事件
			EventManager.Instance.AddListener<object, object>(GameEventType.OptionConfirm, (sender, data) =>
				{
					Screen.SetResolution(OptionManager.gameResolutionX, OptionManager.gameResolutionY, OptionManager.isFullScreen);
				});
			//加载语言
			LanguageManager.Instance.Initial();
			//加载管理器
			SaveManager.Instance.Initial();
			OptionManager.Instance.Initial();
			AudioManager.Instance.Initial();
			//加载全局数据
//			if(i)
				//SaveManager.Instance.SaveGlobalData();
			SaveManager.Instance.LoadGlobalData();
			//加载游戏信息
			GameName = LanguageManager.Instance.GetString("GameName");
		}
	}
}
