using System.Collections;
using System.Collections.Generic;
using ManagerRelative;
using UnityEngine;
using S.Serialize;
public class RoomController : MonoBehaviour {
	public void SaveRoom(Room room, string path)
	{
		SaveUtility.Save<Room>(room, path);
	}

	public Room LoadRoom(string path)
	{
		return SaveUtility.Load<Room>(path);
	}

	public Room CreateRoom(Vector2Int roomSize,RoomType roomType,string roomName)
	{
		Room room = new Room();
		room.roomSize = roomSize;
		room.roomName = roomName;
		room.roomType = roomType;
		room.Initial();
		Debug.Log("<color=red>" + room.roomBaseLayer.Count + "</color>");
		return room;
	}

	public void AddBlock(Room room, Vector2Int pos, Room.RoomElementData blockData, bool replace = true)
	{
		if (room.roomBaseLayer.ContainsKey(pos))
		{
			if (replace || room.roomBaseLayer[pos].ID == 0)
			{
				room.roomBaseLayer[pos] = blockData;
				Debug.Log(blockData.ID + "\n" + blockData.Rotation);
			}
		}
	}

	public void RemoveBlock(Room room, Vector2Int pos)
	{
		if (room.roomBaseLayer.ContainsKey(pos))
		{
				room.roomBaseLayer[pos] = new Room.RoomElementData(0,0);
		}
	}


}
