using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.UI;

public class OptionPanelUIManager : MonoBehaviour {
	public Dropdown resolution,language;
	public Toggle fullscreen;
	public Slider main, bgm, se;
	public Button Confirm, Cancel;
	public void OnEnable()
	{
		if(OptionManager.gameResolutionX==480&& OptionManager.gameResolutionY==270)
				resolution.value = 0;
		if (OptionManager.gameResolutionX == 1024 && OptionManager.gameResolutionY == 576)
			resolution.value = 1;
		if (OptionManager.gameResolutionX == 1280 && OptionManager.gameResolutionY == 720)
			resolution.value = 2;
		if (OptionManager.gameResolutionX == 1920 && OptionManager.gameResolutionY == 1080)
			resolution.value = 3;
		if (OptionManager.gameResolutionX == 2560 && OptionManager.gameResolutionY == 1440)
			resolution.value = 4;
		if (OptionManager.gameResolutionX == 3840 && OptionManager.gameResolutionY == 2160)
			resolution.value = 5;
		fullscreen.isOn = OptionManager.isFullScreen;
		language.value = (int) OptionManager.language;
		main.value = OptionManager.mainVolume;
		bgm.value = OptionManager.musicVolume;
		se.value = OptionManager.gameVolume;
	}
	public void SendResolutionOptionChange()
	{
		OptionManager.Instance.SetResolution(resolution.value);
	}
	public void SendLanguageOptionChange()
	{
		OptionManager.Instance.SetLanguage(language.value);
	}
	public void SendFullScreenOptionChange()
	{
		OptionManager.Instance.SetFullScreen(fullscreen.isOn);
	}

	public void SendMainVolumeChange()
	{
		OptionManager.Instance.SetMainVolume(main.value);
	}

	public void SendGameVolumeChange()
	{
		OptionManager.Instance.SetGameVolume(se.value);
	}

	public void SendBGMVolumeChange()
	{
		OptionManager.Instance.SetBackgroundVolume(bgm.value);
	}

	public void ConfirmDown()
	{
		OptionManager.Instance.ConfirmButtonDown();
	}

	public void CancelDown()
	{
		OptionManager.Instance.CancelButtonDown();
	}
}
