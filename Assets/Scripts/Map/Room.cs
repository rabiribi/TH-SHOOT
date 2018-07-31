using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
	Normal,
	Chest,
	Boss,
	Hiden,
	Shop
}
[Serializable]
public class Room
{
	[Serializable]
	public struct RoomElementData
	{
		public int ID;
		public int Rotation;

		public RoomElementData(int id, int rotation)
		{
			ID = id;
			Rotation = rotation;
		}
		public static bool operator !=(RoomElementData a1, RoomElementData a2)
		{
			return !(a1.ID == a2.ID && a1.Rotation == a2.Rotation);
		}

		public static bool operator ==(RoomElementData a1, RoomElementData a2)
		{
			return (a1.ID == a2.ID && a1.Rotation == a2.Rotation);
		}
	}
	public string roomName;
	public RoomType roomType;
	public SVector2Int roomSize;
	public Dictionary<SVector2Int, RoomElementData> roomBaseLayer=new Dictionary<SVector2Int, RoomElementData>();
	public Dictionary<SVector2Int, RoomElementData> roomItems=new Dictionary<SVector2Int, RoomElementData>();
	public void Initial()
	{
		for (int x = 0; x < roomSize.x; x++)
		{
			for (int y = 0; y < roomSize.y; y++)
			{
				roomBaseLayer.Add(new Vector2Int(x, y),new RoomElementData(0,0));
			}
		}
	}
}
