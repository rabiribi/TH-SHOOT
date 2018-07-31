using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditorPointer : MonoBehaviour {
	public Vector2 mousePosition;
	public Vector2Int mouseMapPosition;
	public RoomViewer roomViewer;
	public RoomController roomController;
	public TilesToggleUI tilesToggleUI;
	public SpriteRenderer spriteRenderer;
	public MapEditorMenuUI mapEditorMenuUI;
	private Vector2Int mouseMapPointFollower=new Vector2Int(-1,-1);
	public int rotationIndex = 0;

	public void RotationL()
	{
		if (rotationIndex < 3)
			rotationIndex++;
		else
			rotationIndex = 0;
		UpdateTexture();
	}
	public void RotationR()
	{
		if (rotationIndex > 0)
			rotationIndex--;
		else
			rotationIndex = 3;
		UpdateTexture();

	}
	public void Start()
	{
		tilesToggleUI.OnTileChangeEvent += UpdateTexture;
	}
	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.E))
			RotationR();
		if (Input.GetKeyDown(KeyCode.Q))
			RotationL();
		mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mouseMapPosition = new Vector2Int((int) (mousePosition.x / roomViewer.gap + 0.5f),
			(int) (mousePosition.y / roomViewer.gap + 0.5f));
		transform.position = roomViewer.gap * (Vector2) mouseMapPosition;
		if (Input.GetMouseButton(0))
		{
			if (mouseMapPointFollower != mouseMapPosition)
			{
				mouseMapPointFollower = mouseMapPosition;
				OnClick();
			}
		}
	}
	public void OnClick()
	{
			if (mapEditorMenuUI.GetRoom() != null && !mapEditorMenuUI.GetRoom().roomBaseLayer.ContainsKey(mouseMapPosition))
				return;
			if (tilesToggleUI.roomElement is RoomBaseLayer)
			{
				tilesToggleUI.roomElement.SetRotation(rotationIndex);
				roomController.AddBlock(mapEditorMenuUI.GetRoom(), mouseMapPosition, tilesToggleUI.roomElement.GetRoomData());
				//roomController.AddBlock(mapEditorMenuUI.GetRoom(), mouseMapPosition,new Room.RoomElementData(2,rotationIndex));
				mapEditorMenuUI.RefreshRoom();
			}
	}
	public void UpdateTexture(TileToggle tileToggle=null)
	{
		if(tileToggle!=null)
		spriteRenderer.sprite = tileToggle.contentImage.sprite;
		switch (rotationIndex)
		{
			case 0:
				transform.rotation = Quaternion.identity;
				break;
			case 1:
				transform.rotation = Quaternion.Euler(0,0,90);
				break;
			case 2:
				transform.rotation = Quaternion.Euler(0, 0, 180);
				break;
			case 3:
				transform.rotation = Quaternion.Euler(0, 0, 270);

				break;
		}
	}
}
