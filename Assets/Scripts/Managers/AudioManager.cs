using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GameMain
{
	public class AudioManager : MonoSingletion<AudioManager>, IManager
	{
		public static float MainVolume = 1f;
		public AudioMixer audioMixer;
		public void Initial()
		{
			EventManager.Instance.AddListener<object, object>(GameEventType.OptionConfirm, (sender, data) =>
			{
				MainVolume = OptionManager.mainVolume;
				SetAllAudiosVolume();
			});
		}

		public void SetAllAudiosVolume()
		{
			audioMixer.SetFloat("MainVolume", OptionManager.mainVolume*80-80);
			audioMixer.SetFloat("MusicVolume", OptionManager.musicVolume * 80 - 80);
			audioMixer.SetFloat("GameVolume", OptionManager.gameVolume * 80 - 80);
		}
	}
}
