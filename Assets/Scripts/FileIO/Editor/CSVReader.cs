using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using GameMain;
using UnityEditor;
using UnityEngine;
public class CSVReader : EditorWindow {
	public static string DATA_PATH;
	private TextAsset csvFile;
	[MenuItem("Data/DecodeCVS")]
	private static void AddWindow()
	{
		//创建窗口
		Rect wr = new Rect(0, 0, 300, 100);
		CSVReader window = (CSVReader)EditorWindow.GetWindowWithRect(typeof(CSVReader), wr, true, "widow name");
		window.Show();
	}
	private void OnGUI()
	{
		GUILayout.Space(20);
		csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV", csvFile,typeof(TextAsset));
		if (GUILayout.Button("Confirm"))
		{
			if (csvFile == null)
				return;

			string[] line = csvFile.text.Split('\n');
			int rowCount = line[0].Trim().Split(',').Length;
			string[,] message=new string[line.Length,rowCount];
			for (int i = 0; i < line.Length-1; i++)
			{
				string[] lineMessage = line[i].Trim().Split(',');
				for (int j = 0; j < rowCount; j++)
				{
					message[i, j] = lineMessage[j];
				}
			}
			List<string> names = new List<string>();
			for (int i = 0; i < message.GetLength(1);i++)
			{
				names.Add(message[0, i]);
				Debug.Log("<color=yellow>" + message[0, i] + "</color>");

			}
			List<string> structor = new List<string>();
			for (int i = 0; i < message.GetLength(1); i++)
			{
				structor.Add(message[1, i]);
			}
			List<string> type = new List<string>();
			for (int i = 0; i < message.GetLength(1); i++)
			{
				type.Add(message[2, i]);
			}

			string code = "";
			string className = csvFile.name+"Reader";
			//引用
			code += "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n";
			//类声明
			code += "public class " + className + ":CSVReadBase{\n";
			bool dic = false;
			int keyindex = 0;
			for (int i = 0; i < structor.Count; i++)
			{
				//字典
				if (structor[i] == "key")
				{
					keyindex = i;
					dic = true;
					break;
				}
			}
			if (dic)
			{
				string dataClassCode = "";
				string dataClassName = csvFile.name + "Data";
				dataClassCode += "public class " + dataClassName + "{\n";
				int j = 0;
				for (int k = 0; k < structor.Count; k++)
				{
					Debug.Log("<color=blue>" + names[k] + "</color>");
					if (k != keyindex)
					{
						Debug.Log("<color=purple>" + names[k] + "</color>");
						dataClassCode += "public " + type[k] + " " + names[k] + ";\n";
					}
				}
				dataClassCode += "}\n";
				code += dataClassCode;
				string datasType = "Dictionary<" + type[keyindex] + "," + dataClassName + ">";
				code += "public " + datasType + " " + csvFile.name + "Datas=new " + datasType + "();\n";
				string DicName = csvFile.name + "Datas";
				string GetT = "";
				for (int k = 0; k < message.GetLength(1); k++)
				{
					if (k != keyindex)
						GetT += "if(" + k + "==row)" + DicName + "[message[line," + keyindex.ToString() + "]]." + names[k] + "=" + "GetValueResult<" + type[k] + ">(message[line,row]);\n";
				}
				code += "public override void WriteData(int line, int row,string data)\n" +
				        "{\n" +
				        "if(row==" + keyindex.ToString() + "){" +
				        "if(!" + DicName + ".ContainsKey(message[line,row])){\n" +
				        DicName + ".Add(message[line,row],new " + dataClassName + "());\n" +
				        "}\n" +
				        "}\n" +
				        "else{\n" +
				        GetT +
				        "}\n" +
				        "}\n";
			}
			else
			{
				string dataClassCode = "";
				string dataClassName = csvFile.name + "Data";
				dataClassCode += "public class " + dataClassName + "{\n";
				int j = 0;
				for (int k = 0; k < structor.Count; k++)
				{
					Debug.Log("<color=purple>" + names[k] + "</color>");
					dataClassCode += "public " + type[k] + " " + names[k] + ";\n";
				}
				dataClassCode += "}\n";
				code += dataClassCode;
				string datasType = "List<" + dataClassName + ">";
				code += "public " + datasType + " " + csvFile.name + "Datas=new " + datasType + "();\n";
				string ListName = csvFile.name + "Datas";
				string GetT = "";
				for (int k = 0; k < message.GetLength(1); k++)
				{
					GetT += "if(" + k + "==row)" + ListName + "[line-3]." + names[k] + "=" + "GetValueResult<" + type[k]+">(message[line,row]);\n";
				}
				code += "public override void WriteData(int line, int row,string data)\n" +
				        "{\n" +
				        "if(!(" + ListName + ".Count>=(line-3))){\n" +
				        ListName + ".Add(new " + dataClassName + "());\n" +
				        "}\n" +
				        "else{\n" +
				        GetT +
				        "}\n" +
				        "}\n";
			}
			//类结束
			code += "}\n";
			File.WriteAllText(Application.dataPath+"/Scripts/FileIO/CSVRead/"+className+".cs", code);
		}
	}
}

