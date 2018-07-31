using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.UI;

public partial class MapEditorMenuUI : MonoBehaviour
{
	public InputField nameInput;
	public RoomController roomController;
	public RoomViewer roomViewer;
	private Room room;
	private GameObject roomObject;
	public GameObject Camera;
	public TilesToggleUI tileToggle;
	public string TileFolderNum = "00";
}
public partial class MapEditorMenuUI : MonoBehaviour
{
	public void Start()
	{
		Refresh();
	}
	#region 房间函数
	public void CreateRoom( Vector2Int roomSize,string RoomName,RoomType roomType)
	{
		room=roomController.CreateRoom(roomSize, roomType, RoomName);
		roomObject=roomViewer.CreateRoomObject(room);
		Camera.transform.position = new Vector3(roomViewer.gap*(float) roomSize.x / 2, roomViewer.gap * (float) roomSize.y / 2, -10);
	}

	public void RefreshRoom()
	{
		if (roomObject != null) ;
		Destroy(roomObject);
		roomObject=roomViewer.CreateRoomObject(room);
		Camera.transform.position = new Vector3(roomViewer.gap * (float)room.roomSize.x / 2, roomViewer.gap * (float)room.roomSize.y / 2, -10);
	}
	public Room GetRoom()
	{
		return room;
	}
	public void ClearRoom()
	{
		if (roomObject != null) ;
		Destroy(roomObject);
		Camera.transform.position = new Vector3(0, 0, -10);
	}
	#endregion
	#region Tile函数

	public void SetTileFolder(string num)
	{
		TileFolderNum = num;
		Refresh();
	}
	public void Refresh()
	{
		tileToggle.Refresh(TileFolderNum);
	}
	#endregion
}
public partial class MapEditorMenuUI : MonoBehaviour
{
	#region 保存 读取
	public void OpenProject()
	{
		OpenFileDlg pth = new OpenFileDlg();
		pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
		pth.filter = "房间(*.room)\0*.room\0\0";
		pth.file = new string(new char[256]);
		pth.maxFile = pth.file.Length;
		pth.fileTitle = new string(new char[64]);
		pth.maxFileTitle = pth.fileTitle.Length;
		pth.initialDir = Application.dataPath+ @"/GameData/Rooms";
		pth.title = "打开项目";
		pth.defExt = "room";
		pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
		if (OpenFileDialog.GetOpenFileName(pth))
		{
			string filepath = pth.file;//选择的文件路径;  
			Debug.Log(filepath);
			room = roomController.LoadRoom(filepath);
			ClearRoom();
			roomObject = roomViewer.CreateRoomObject(room);
			Camera.transform.position = new Vector3(roomViewer.gap * (float)room.roomSize.x / 2, roomViewer.gap * (float)room.roomSize.y / 2, -10);
		}
	}
	public void SaveProject()
	{
		SaveFileDlg pth = new SaveFileDlg();
		pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
		pth.filter = "房间(*.room)\0*.room\0\0";
		pth.file = new string(new char[256]);
		pth.maxFile = pth.file.Length;
		pth.fileTitle = new string(new char[64]);
		pth.file = nameInput.text;
		pth.maxFileTitle = pth.fileTitle.Length;
		pth.initialDir = Application.dataPath+@"/GameData/Rooms";
		pth.title = "保存项目";
		pth.defExt = "room";
		pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
		if (SaveFileDialog.GetSaveFileName(pth))
		{
			string filepath = pth.file;//选择的文件路径;  
			Debug.Log(filepath);
			roomController.SaveRoom(room, filepath.Replace('\\','/'));
		}
	}
	#endregion
}

public partial class MapEditorMenuUI : MonoBehaviour
{
}
