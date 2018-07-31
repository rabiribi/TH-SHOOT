using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
public class ChinarMessage
{
	[DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	public static extern int MessageBox(IntPtr handle, String message, String title, int type);//具体方法
}
public partial class NewRoomUI : MonoBehaviour {
	public InputField Name;
	public Dropdown RoomType;
	public InputField SizeX, SizeY;
	public MapEditorMenuUI mapEditorMenu;
}
public partial class NewRoomUI : MonoBehaviour
{
	public void OnOK()
	{
		int x = 0, y = 0;
		try
		{
			x = int.Parse(SizeX.text);
			y = int.Parse(SizeY.text);
		}
		catch
		{
			ChinarMessage.MessageBox(IntPtr.Zero,"尺寸大小必须为整数","尺寸未识别",0);
			return;
		}

		mapEditorMenu.ClearRoom();
		mapEditorMenu.CreateRoom(new Vector2Int(x, y), Name.text,(RoomType) Enum.ToObject(typeof(RoomType), RoomType.value));
	}
}
