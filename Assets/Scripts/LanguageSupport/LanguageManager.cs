using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameMain;

namespace LanguageSupport
{
	public class LanguageManager : MonoSingletion<LanguageManager>,IManager
	{
		public static LanguageReader reader;
		public static string languageFilePath="";
		public void Initial()
		{
			reader = new LanguageReader();
			languageFilePath = Application.dataPath + "/GameData/OtherData/Language.csv";
			reader.GetCSVData(languageFilePath);
		}

		public string GetString(string key)
		{
			switch (OptionManager.optionData.language)
			{
				case Language.English:
					return reader.LanguageDatas[key].English;
				case Language.日本語:
					return reader.LanguageDatas[key].Japanese;
				case Language.简体中文:
					return reader.LanguageDatas[key].Chinese;
			}
			return "";
		}
		
	}
}
