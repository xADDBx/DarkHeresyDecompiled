using System.Collections.Generic;
using Code.Framework.Sound.Base.Validation;
using UnityEngine;

namespace Kingmaker.Sound.Base;

public class SoundEventsManager
{
	public SoundEventValidation Validation = new SoundEventValidation();

	private List<uint> m_PlayingIdsToStop = new List<uint>();

	public static SoundEventsManager Instance { get; }

	private bool StopAllEvents { get; set; }

	static SoundEventsManager()
	{
		Instance = new SoundEventsManager();
		Instance.Validation.Init();
	}

	public static bool TryGetVoiceOverEvent(string stringId, string voId, out string eventId)
	{
		return Instance.Validation.TryGetVoiceOverEvent(stringId, voId, out eventId);
	}

	public static uint PostEvent(string eventName, GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie)
	{
		return Instance.PostEventInternal(eventName, gameObject, flags, callback, cookie);
	}

	public static uint PostEvent(string eventName, GameObject gameObject, bool canBeStopped = false)
	{
		return Instance.PostEventInternal(eventName, gameObject, canBeStopped);
	}

	public static uint PostEvent(uint eventId, GameObject gameObject)
	{
		return Instance.PostEventInternal(eventId, gameObject);
	}

	public static uint PostEvent(uint eventId, GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie)
	{
		return Instance.PostEventInternal(eventId, gameObject, flags, callback, cookie);
	}

	public static void StopPlayingById(uint playingId)
	{
		Instance.StopPlayingByIdInternal(playingId);
	}

	public void Update()
	{
		if (!StopAllEvents)
		{
			return;
		}
		foreach (uint item in m_PlayingIdsToStop)
		{
			AkUnitySoundEngine.StopPlayingID(item, 0);
		}
		m_PlayingIdsToStop.Clear();
	}

	public void SetStoppingAllState(bool active)
	{
		StopAllEvents = active;
		if (StopAllEvents)
		{
			AkUnitySoundEngine.PostEvent("TECH_SkipCutscene_StopAudio", null);
		}
		else
		{
			AkUnitySoundEngine.PostEvent("TECH_SkipCutscene_ResumeAudio", null);
		}
		m_PlayingIdsToStop.Clear();
	}

	private uint PostEventInternal(string eventName, GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie)
	{
		uint id = AkUnitySoundEngine.PostEvent(eventName, gameObject, flags, callback, cookie);
		return SaveIfNeeded(id);
	}

	private uint PostEventInternal(string eventName, GameObject gameObject, bool canBeStopped = false)
	{
		uint id = AkUnitySoundEngine.PostEvent(eventName, gameObject);
		return SaveIfNeeded(id, canBeStopped);
	}

	private uint PostEventInternal(uint eventId, GameObject gameObject)
	{
		uint id = AkUnitySoundEngine.PostEvent(eventId, gameObject);
		return SaveIfNeeded(id);
	}

	private uint PostEventInternal(uint eventId, GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie)
	{
		uint id = AkUnitySoundEngine.PostEvent(eventId, gameObject, flags, callback, cookie);
		return SaveIfNeeded(id);
	}

	private uint SaveIfNeeded(uint id, bool canBeStopped = false)
	{
		if (StopAllEvents && canBeStopped)
		{
			m_PlayingIdsToStop.Add(id);
		}
		return id;
	}

	private void StopPlayingByIdInternal(uint playingId)
	{
		AkUnitySoundEngine.StopPlayingID(playingId);
		m_PlayingIdsToStop.RemoveAll((uint x) => x == playingId);
	}
}
