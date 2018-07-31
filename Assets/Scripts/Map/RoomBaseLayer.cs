using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRoomElement
{
	Room.RoomElementData GetRoomData();
	void SetRoomData(int id, int rotation);
	void SetID(int id);
	void SetRotation( int rotation);
}
public class RoomBaseLayer : MonoBehaviour,IRoomElement
{
	public Room.RoomElementData roomData;

	public string folder;
	public Room.RoomElementData GetRoomData()
	{
		return roomData;
	}

	public void SetRoomData(int id, int rotation)
	{
		roomData.ID = id;
		roomData.Rotation = rotation;
	}

	public void SetID(int id)
	{
		roomData.ID = id;
	}

	public void SetRotation(int rotation)
	{
		roomData.Rotation = rotation;
	}
}
