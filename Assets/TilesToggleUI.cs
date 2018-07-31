using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TilesToggleUI : MonoBehaviour {
	public GameObject TileTogglePrefab;
	public ToggleGroup toggleGroup;
	public RoomViewer roomViewer;
	public Transform Content;
	public IRoomElement roomElement;
	public event Action<TileToggle> OnTileChangeEvent;
	public void ClearContent()
	{
		foreach (Transform t in Content)
		{
			Destroy(t.gameObject);
		}
	}
	public void SummonList(string key)
	{
		foreach (var o in roomViewer.Tiles[key])
		{
			
			TileToggle tt= Instantiate(TileTogglePrefab, Content).GetComponent<TileToggle>();
			tt.GetComponent<Toggle>().group = toggleGroup;
			tt.roomElement = o.GetComponent<IRoomElement>();
			tt.ui = this;
			tt.LoadTexture(o.GetComponent<SpriteRenderer>().sprite);
			tt.OnTileChangeEvent+=OnTileChange;
		}
	}

	public void OnTileChange(TileToggle tileToggle)
	{
		if (OnTileChangeEvent != null) OnTileChangeEvent(tileToggle);
	}
	public void Refresh(string key)
	{
		ClearContent();
		SummonList(key);
	}
}
