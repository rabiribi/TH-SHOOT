using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVReadBase{
	public string[,] message;
	public void GetCSVData(string path)
	{
		string[] line = System.IO.File.ReadAllLines(path, Encoding.Default);
		int rowCount = line[0].Split(',').Length;
		message = new string[line.Length, rowCount];
		for (int i = 0; i < line.Length; i++)
		{
			string[] lineMessage = line[i].Split(',');
			for (int j = 0; j < rowCount; j++)
			{
				message[i, j] = lineMessage[j];
			}
		}
		for (int i = 3; i < line.Length; i++)
		{
			for (int j = 0; j < rowCount; j++)
			{
				WriteData(i, j, message[i, j]);
			}
		}
	}
	public virtual void WriteData(int line, int row,string data)
	{

	}

	public T GetValueResult<T>(string value)
	{
		return (T)GetValue<T>(value);
	}
	private object GetValue<T>(string value)

	{
		if (typeof(T) == typeof(string))
		{
			return value;
		}
		if (typeof(T) == typeof(int))
		{
			return int.Parse(value);
		}
		if (typeof(T) == typeof(float))
		{
			return float.Parse(value);
		}
		if (typeof(T) == typeof(Vector2))
		{
			string[] v = value.Split(',');
			return new Vector2(float.Parse(v[0]), float.Parse(v[1]));
		}
		if (typeof(T) == typeof(Vector3))
		{
			string[] v = value.Split(',');
			return new Vector3(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
		}
		if (typeof(T) == typeof(Vector2Int))
		{
			string[] v = value.Split(',');
			return new Vector2Int(int.Parse(v[0]), int.Parse(v[1]));
		}
		if (typeof(T) == typeof(Vector3Int))
		{
			string[] v = value.Split(',');
			return new Vector3Int(int.Parse(v[0]), int.Parse(v[1]), int.Parse(v[2]));
		}
		if (typeof(T) == typeof(Enum))
		{
			return Enum.Parse(typeof(T), value);
		}
		return null;
	}
}
