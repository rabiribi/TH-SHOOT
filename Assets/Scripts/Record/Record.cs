using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
[System.Serializable]
public class Record
{
	public string recordName = "";
	public string date = "";
	public Dictionary<SaveID, SaveData> RecordGameData = new Dictionary<SaveID, SaveData>();
	public Record()
	{
		date = System.DateTime.Now.ToString();
	}
}
