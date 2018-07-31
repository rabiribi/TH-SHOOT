using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManagerRelative;
using UnityEngine;

namespace GameMain
{
	[Serializable]
	public class SaveData
	{

	}

	public interface ISavable
	{
		SaveData OnSave();
		void OnLoad(SaveData data);
	}

	[Serializable]
	public struct SaveID
	{
		public string IDName;
		public int priorityLevel;
	}

	public class SaveManager : MonoSingletion<SaveManager>, IManager
	{
		public Dictionary<SaveID, SaveData> GlobalGameData = new Dictionary<SaveID, SaveData>();
		public Dictionary<string,Record> Records = new Dictionary<string, Record>();
		public Record currentRecord;
		public static string GAME_GLOBAL_DATA_PATH;
		public static string GAME_RECORD_DATA_PATH;
		public static string _GAME_RECORD_DATA_PATH;
		public void Initial()
		{
			GAME_GLOBAL_DATA_PATH = Application.dataPath + "/GameData/Global/";
			GAME_RECORD_DATA_PATH = Application.dataPath + "/GameData/Records/";
			_GAME_RECORD_DATA_PATH = Application.dataPath + "/GameData/Records";
		}
		public void RemoveFromGlobalData(SaveID saveID)
		{
			GlobalGameData.Remove(saveID);
		}
		public void RemoveFromRecordData(Record record,SaveID saveID)
		{
			record.RecordGameData.Remove(saveID);
		}
		public void RememberGlobalData()
		{
			EventManager.Instance.PostEvent<SaveManager, object>(GameEventType.RememberGlobalGameData, this);
		}
		public void RememberRecordData(Record record)
		{
			EventManager.Instance.PostEvent<SaveManager, Record>(GameEventType.RememberRecordData, this,record);
		}
		public void ReadGlobalGameData()
		{
			EventManager.Instance.PostEvent<SaveManager, object>(GameEventType.ReadGlobalGameData, this);
		}
		public void ReadRecordData(Record record)
		{
			EventManager.Instance.PostEvent<SaveManager, Record>(GameEventType.ReadRecordData, this,record);
		}
		public void CallGlobalDataSave(ISavable savable, SaveID Id)
		{
			if (GlobalGameData.ContainsKey(Id))
				GlobalGameData[Id] = savable.OnSave();
			else
				GlobalGameData.Add(Id, savable.OnSave());
		}
		public void CallRecordSave(Record record, ISavable savable, SaveID Id)
		{
			if (record.RecordGameData.ContainsKey(Id))
				record.RecordGameData[Id] = savable.OnSave();
			else
				record.RecordGameData.Add(Id, savable.OnSave());
		}
		public void CallGlobalDataLoad(ISavable savable, SaveID Id)
		{
			if (GlobalGameData.ContainsKey(Id))
				savable.OnLoad(GlobalGameData[Id]);
		}
		public void CallRecordDataLoad(Record record, ISavable savable, SaveID Id)
		{
			if (record.RecordGameData.ContainsKey(Id))
				savable.OnLoad(record.RecordGameData[Id]);
		}
		public void SaveGlobalData()
		{
			RememberGlobalData();
			SaveUtility.Save(GlobalGameData, GAME_GLOBAL_DATA_PATH + "GameData.data");
		}
		public void LoadGlobalData()
		{
			GlobalGameData = SaveUtility.Load<Dictionary<SaveID, SaveData>>(GAME_GLOBAL_DATA_PATH + "GameData.data");
			ReadGlobalGameData();
		}
		public void LoadRecordList()
		{
			Records.Clear();
			string[] paths = Directory.GetFiles(_GAME_RECORD_DATA_PATH);
			foreach (var recordPath in paths)
			{
				if (Path.GetExtension(recordPath) == ".data")
				{
					Record theRecord= SaveUtility.Load<Record>(recordPath);
					theRecord.recordName = Path.GetFileNameWithoutExtension(recordPath);
					Records.Add(theRecord.recordName,theRecord);
				}
			}
		}
		public void EnterRecord(Record record)
		{
			LoadRecordList();
			currentRecord = record;
			ReadRecordData(record);
		}
		public void SaveRecord(Record record)
		{
			RememberRecordData(record);
			SaveUtility.Save<Record>(record, GAME_RECORD_DATA_PATH + record.recordName + ".data");
		}

		public void CreateNewRecord()
		{
			LoadRecordList();
			for (int i = 0;; i++)
			{
				string theName = "Record" + i.ToString("x8");
				if (!Records.ContainsKey(theName))
				{
					Record record = new Record
					{
						recordName = theName
					};
					SaveUtility.Save<Record>(record, GAME_RECORD_DATA_PATH + record.recordName + ".data");
					break;
				}
			}
			LoadRecordList();
		}
	}
}