using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomViewer : MonoBehaviour
{
	public string resourcesPath;
	public float gap = 1.6f;
	public GameObject[] roomBaseLayer;
	public GameObject[] roomItems;
	public Dictionary<string, List<GameObject>> Tiles = new Dictionary<string, List<GameObject>>();
	public void Awake()
	{
		Refresh();
	}
	public void Refresh()
	{
		resourcesPath ="MapElement";
		roomBaseLayer =Resources.LoadAll<GameObject>(resourcesPath + "/roomBaseLayer");
		roomItems = Resources.LoadAll<GameObject>(resourcesPath + "/roomItems");
		Tiles = new Dictionary<string, List<GameObject>>();
		for (int i = 0; i < 12; i++)
		{
			Tiles.Add(i.ToString("D2"), new List<GameObject>());
			Tiles[i.ToString("D2")] = Resources.LoadAll<GameObject>(resourcesPath + "/roomBaseLayer/" + i.ToString("D2")).ToList();
			Tiles[i.ToString("D2")].Union(Resources.LoadAll<GameObject>(resourcesPath + "/roomItems/" + i.ToString("D2")).ToList());
		}
	}

	public GameObject CreateRoomObject(Room room)
	{
		Debug.Log(resourcesPath + "/roomBaseLayer");
		GameObject roomObj =new GameObject(room.roomName);
		GameObject roomBaseLayerObj = new GameObject("RoomBaseLayer");
		roomBaseLayerObj.transform.parent = roomObj.transform;
		GameObject roomItemsObj = new GameObject("RoomItems");
		roomItemsObj.transform.parent = roomObj.transform;
		Debug.Log("<color=green>" + room.roomBaseLayer.Count + "</color>");
		int k = 0;
		foreach (var i in room.roomBaseLayer)
		{
			GameObject block=null;
			foreach (var b in roomBaseLayer)
			{
				if (b.GetComponent<RoomBaseLayer>().roomData.ID == i.Value.ID)
					block = b;
			}

			if (block != null)
			{
				switch (i.Value.Rotation)
				{
					case 0:
						Instantiate(block, new Vector3(i.Key.x * gap, i.Key.y * gap, 0f), Quaternion.identity,
							roomBaseLayerObj.transform);
						break;
					case 1:
						Instantiate(block, new Vector3(i.Key.x * gap, i.Key.y * gap, 0f), Quaternion.Euler(0, 0, 90),
							roomBaseLayerObj.transform);
						break;
					case 2:
						Instantiate(block, new Vector3(i.Key.x * gap, i.Key.y * gap, 0f), Quaternion.Euler(0, 0, 180),
							roomBaseLayerObj.transform);
						break;
					case 3:
						Instantiate(block, new Vector3(i.Key.x * gap, i.Key.y * gap, 0f), Quaternion.Euler(0, 0, 270),
							roomBaseLayerObj.transform);
						break;
				}
			}
		}
		return roomObj;
	}
}
