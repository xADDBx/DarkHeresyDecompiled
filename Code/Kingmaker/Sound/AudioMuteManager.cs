namespace Kingmaker.Sound;

public static class AudioMuteManager
{
	private static bool s_MusicMute;

	private static bool s_AllSoundMute;

	private static void SetNoneState()
	{
		AkUnitySoundEngine.SetState("AudioStatus", "None");
	}

	private static void SetAllAudioMuteState()
	{
		AkUnitySoundEngine.SetState("AudioStatus", "AllAudioMute");
	}

	private static void SetMusicMuteState()
	{
		AkUnitySoundEngine.SetState("AudioStatus", "MusicMute");
	}

	public static void ToggleAllMute()
	{
		s_AllSoundMute = !s_AllSoundMute;
		UpdateState();
	}

	public static void ToggleMusicMute()
	{
		s_MusicMute = !s_MusicMute;
		UpdateState();
	}

	private static void UpdateState()
	{
		if (s_AllSoundMute)
		{
			SetAllAudioMuteState();
		}
		else if (s_MusicMute)
		{
			SetMusicMuteState();
		}
		else
		{
			SetNoneState();
		}
	}
}
