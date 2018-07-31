using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileToggle : MonoBehaviour
{
	public IRoomElement roomElement;
	public TilesToggleUI ui;
	public Image contentImage;
	public event Action<TileToggle> OnTileChangeEvent; 
	public void LoadTexture(Sprite sprite)
	{
		if (roomElement is RoomBaseLayer)
		{
			contentImage.sprite = sprite;
		}
	}
	public void OnChoose()
	{
		if (GetComponent<Toggle>().isOn)
		{
			if (OnTileChangeEvent != null) OnTileChangeEvent(this);
			ui.roomElement = roomElement;
		}
	}
}
