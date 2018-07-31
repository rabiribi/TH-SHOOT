using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
	public Dictionary<SVector2Int, int> Background = new Dictionary<SVector2Int, int>();
	public Dictionary<SVector2Int, int> Blocks = new Dictionary<SVector2Int, int>();
	public Dictionary<SVector2Int, int> Others = new Dictionary<SVector2Int, int>();
}
[Serializable]
public struct SVector2Int
{
	public int x, y;

	public SVector2Int(int x, int y)
	{
		this.x = x;
		this.y = y;
	}
	public static implicit operator Vector2Int(SVector2Int sv)
	{
		return new Vector2Int(sv.x, sv.y);
	}
	public static implicit operator SVector2Int(Vector2Int v)
	{
		return new SVector2Int(v.x, v.y);
	}
}
[Serializable]
public struct SVector2
{
	public float x, y;

	public SVector2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}
	public static implicit operator Vector2(SVector2 sv)
	{
		return new Vector2(sv.x, sv.y);
	}
	public static implicit operator SVector2(Vector2 v)
	{
		return new SVector2(v.x, v.y);
	}
}

