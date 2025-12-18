using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.GameModes;
using Kingmaker.Settings;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Sound.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.UI.Sound;

public class VoiceOverTracker
{
	public List<VoiceOverPlayingEntry> Entries = new List<VoiceOverPlayingEntry>();

	public List<VoiceOverPlayingEntry.StaticData> SavedDialogEntries = new List<VoiceOverPlayingEntry.StaticData>();

	private Dictionary<string, List<VoiceOverPlayingEntry>> m_VoIdToEntryMap = new Dictionary<string, List<VoiceOverPlayingEntry>>();

	private Dictionary<string, List<VoiceOverPlayingEntry.StaticData>> m_Schedulled = new Dictionary<string, List<VoiceOverPlayingEntry.StaticData>>();

	public void Tick()
	{
		for (int num = Entries.Count - 1; num >= 0; num--)
		{
			VoiceOverPlayingEntry entry = Entries[num];
			if (entry.Status.IsEnded)
			{
				m_VoIdToEntryMap[entry.Data.VoId].Add(entry);
				Entries.Remove(entry);
				if (entry.Data.Type == VoiceOverType.Ask && entry.Status.IsEnded && !entry.Status.Stopped)
				{
					entry.Status.Stop();
				}
				if (m_Schedulled.TryGetValue(entry.Data.VoId, out var value) && value.HasItem((VoiceOverPlayingEntry.StaticData x) => x.Type == entry.Data.Type))
				{
					VoiceOverPlayingEntry.StaticData staticData = value.Find((VoiceOverPlayingEntry.StaticData x) => x.Type == entry.Data.Type);
					value.Remove(staticData);
					if (value.Count == 0)
					{
						m_Schedulled.Remove(entry.Data.VoId);
					}
					PlayVoiceOver(staticData);
				}
			}
		}
	}

	private void PlayVoiceOver(VoiceOverPlayingEntry.StaticData data)
	{
		PlayVoiceOver(data.EventId, data.VoId, data.Type, data.Target);
	}

	public void StopAllOnVoId(string voId, GameObject target)
	{
		if (!m_VoIdToEntryMap.TryGetValue(voId, out var value))
		{
			return;
		}
		if (target != null)
		{
			value = value.Where((VoiceOverPlayingEntry x) => x.Data.Target == target).ToList();
		}
		foreach (VoiceOverPlayingEntry entry in value)
		{
			if (entry.Data.Type == VoiceOverType.Banter && !entry.Status.IsEnded)
			{
				Game.Instance.Controllers.Get<BarkBanterController>().InterruptBanter();
			}
			entry.Status.Stop();
			Entries.Remove(entry);
			if (m_Schedulled.TryGetValue(voId, out var value2))
			{
				value2.Remove((VoiceOverPlayingEntry.StaticData x) => x.VoId == entry.Data.VoId && x.Target == entry.Data.Target);
			}
			if (m_VoIdToEntryMap.TryGetValue(voId, out var value3))
			{
				value3.Remove(entry);
			}
			PFLog.VO.Log("[VO] Stopping VO on " + voId + ", " + entry.Data.VoId);
		}
	}

	public void StopAllVoiceOver()
	{
		foreach (KeyValuePair<string, List<VoiceOverPlayingEntry>> item in m_VoIdToEntryMap)
		{
			foreach (VoiceOverPlayingEntry item2 in item.Value)
			{
				if (item2.Data.Type == VoiceOverType.Banter && !item2.Status.IsEnded)
				{
					Game.Instance.Controllers.Get<BarkBanterController>().InterruptBanter();
				}
				item2.Status.Stop();
				Entries.Remove(item2);
				PFLog.VO.Log("[VO] Force stopping VO on " + item2.Data.EventId + " " + item2.Data.VoId);
			}
		}
		Entries.Clear();
		m_Schedulled.Clear();
	}

	public void StopAllVoiceOverExceptOfType(VoiceOverType type)
	{
		foreach (KeyValuePair<string, List<VoiceOverPlayingEntry>> item in m_VoIdToEntryMap)
		{
			foreach (VoiceOverPlayingEntry item2 in item.Value)
			{
				if (item2.Data.Type != type)
				{
					if (item2.Data.Type == VoiceOverType.Banter && !item2.Status.IsEnded)
					{
						Game.Instance.Controllers.Get<BarkBanterController>().InterruptBanter();
					}
					item2.Status.Stop();
					Entries.Remove(item2);
					PFLog.VO.Log("[VO] Force stopping VO on " + item2.Data.EventId + " " + item2.Data.VoId);
				}
			}
		}
		Entries.Clear();
		m_Schedulled.Clear();
	}

	public void SaveInterruptedDialogVoices()
	{
		for (int num = Entries.Count - 1; num >= 0; num--)
		{
			VoiceOverPlayingEntry voiceOverPlayingEntry = Entries[num];
			if (voiceOverPlayingEntry.Data.Type == VoiceOverType.Dialog)
			{
				SavedDialogEntries.Add(voiceOverPlayingEntry.Data);
			}
		}
	}

	public void ResumeSavedDialogVoices()
	{
		foreach (VoiceOverPlayingEntry.StaticData savedDialogEntry in SavedDialogEntries)
		{
			PlayVoiceOver(savedDialogEntry);
		}
		SavedDialogEntries.Clear();
	}

	public void StopAllDialogVoices()
	{
		for (int num = Entries.Count - 1; num >= 0; num--)
		{
			VoiceOverPlayingEntry voiceOverPlayingEntry = Entries[num];
			if (voiceOverPlayingEntry.Data.Type == VoiceOverType.Dialog)
			{
				voiceOverPlayingEntry.Status.Stop();
				Entries.Remove(voiceOverPlayingEntry);
				if (m_VoIdToEntryMap.TryGetValue(voiceOverPlayingEntry.Data.VoId, out var value))
				{
					value.Remove(voiceOverPlayingEntry);
					PFLog.VO.Log("[VO] Stopping VO " + voiceOverPlayingEntry.Data.VoId);
				}
				if (m_Schedulled.TryGetValue(voiceOverPlayingEntry.Data.VoId, out var value2))
				{
					value2.Remove(voiceOverPlayingEntry.Data);
					PFLog.VO.Log("[VO] Stopping schedulled dialog VO " + voiceOverPlayingEntry.Data.VoId);
				}
			}
		}
	}

	public uint PlayAsk(string eventId, string voId, GameObject target, Action<object, AkCallbackType, AkCallbackInfo> callback)
	{
		if ((float)SettingsRoot.Sound.VolumeVoices == 0f)
		{
			return 0u;
		}
		if (string.IsNullOrEmpty(eventId))
		{
			return 0u;
		}
		StopAllOnVoId(voId, target);
		uint num = SoundEventsManager.PostEvent(eventId, target, 1u, callback.Invoke, null);
		VoiceOverStatus voiceOverStatus = new VoiceOverStatus(Game.Instance.Player.RealTime);
		PFLog.VO.Log($"[VO] Playing Ask {eventId} on {voId}. Target: {target}");
		voiceOverStatus.PlayingSoundId = num;
		AddEntry(voId, eventId, VoiceOverType.Ask, voiceOverStatus, target);
		return num;
	}

	[CanBeNull]
	public VoiceOverStatus PlayVoiceOver(string soundEventId, string voId, VoiceOverType type, GameObject target)
	{
		if ((float)SettingsRoot.Sound.VolumeVoices == 0f)
		{
			return null;
		}
		if (string.IsNullOrEmpty(soundEventId))
		{
			return null;
		}
		GameObject gameObject = ((target == null) ? SoundState.Get2DSoundObject() : target);
		if (!gameObject)
		{
			return null;
		}
		StopAllOnVoId(voId, gameObject);
		SoundUtility.SetGenderFlags(gameObject);
		SoundUtility.SetRaceFlags(gameObject);
		VoiceOverStatus voiceOverStatus = new VoiceOverStatus(Game.Instance.Player.RealTime);
		uint num = SoundEventsManager.PostEvent(soundEventId, gameObject, 8u, voiceOverStatus.HandleCallback, null);
		if (num == 0)
		{
			PFLog.VO.Warning("[VO] Playing Error VO [AK_INVALID_PLAYING_ID] " + soundEventId);
			return null;
		}
		PFLog.VO.Log($"[VO] Playing {soundEventId} on {voId} as {type}. Target: {target}");
		voiceOverStatus.PlayingSoundId = num;
		AddEntry(voId, soundEventId, type, voiceOverStatus, gameObject);
		return voiceOverStatus;
	}

	private void AddEntry(string voId, string eventId, VoiceOverType type, VoiceOverStatus status, GameObject target)
	{
		VoiceOverPlayingEntry voiceOverPlayingEntry = new VoiceOverPlayingEntry
		{
			Status = status,
			Data = new VoiceOverPlayingEntry.StaticData
			{
				VoId = voId,
				EventId = eventId,
				Type = type,
				Target = target
			}
		};
		m_VoIdToEntryMap.TryAdd(voiceOverPlayingEntry.Data.VoId, new List<VoiceOverPlayingEntry>());
		m_VoIdToEntryMap[voiceOverPlayingEntry.Data.VoId].Add(voiceOverPlayingEntry);
		Entries.Add(voiceOverPlayingEntry);
	}

	public bool CanPlayAsk(string voId, GameObject target)
	{
		if (Game.Instance.CurrentGameMode?.Type == GameModeType.Dialog)
		{
			return false;
		}
		if (!m_VoIdToEntryMap.TryGetValue(voId, out var value) || !value.Any())
		{
			return true;
		}
		List<VoiceOverPlayingEntry> source = value.ToList();
		if (target != null)
		{
			source = source.Where((VoiceOverPlayingEntry x) => x.Data.Target == target).ToList();
		}
		if (!source.Any())
		{
			return true;
		}
		if (source.HasItem((VoiceOverPlayingEntry x) => x.Data.Type > VoiceOverType.Ask && !x.Status.IsEnded))
		{
			return false;
		}
		return true;
	}

	public bool CanPlayVoiceOver(string voId, VoiceOverType voiceOverType, GameObject target)
	{
		if (Game.Instance.CurrentGameMode?.Type == GameModeType.Dialog && voiceOverType != VoiceOverType.Dialog)
		{
			return false;
		}
		if (!m_VoIdToEntryMap.TryGetValue(voId, out var value) || !value.Any())
		{
			return true;
		}
		List<VoiceOverPlayingEntry> source = value.ToList();
		if (target != null)
		{
			source = source.Where((VoiceOverPlayingEntry x) => x.Data.Target == target).ToList();
		}
		if (!source.Any())
		{
			return true;
		}
		if (source.HasItem((VoiceOverPlayingEntry x) => x.Data.Type == VoiceOverType.Dialog && !x.Status.IsEnded))
		{
			return voiceOverType == VoiceOverType.Dialog;
		}
		if (source.HasItem((VoiceOverPlayingEntry x) => x.Data.Type == VoiceOverType.Banter && !x.Status.IsEnded))
		{
			return voiceOverType >= VoiceOverType.Banter;
		}
		if (source.HasItem((VoiceOverPlayingEntry x) => x.Data.Type == VoiceOverType.Ask && !x.Status.IsEnded))
		{
			return voiceOverType > VoiceOverType.Ask;
		}
		return true;
	}

	public bool TrySchedule(string voId, string eventId, VoiceOverType type, GameObject target)
	{
		if (type != VoiceOverType.Ask)
		{
			return false;
		}
		if (!m_VoIdToEntryMap.TryGetValue(voId, out var value))
		{
			return false;
		}
		if (value.HasItem((VoiceOverPlayingEntry x) => x.Data.Type == VoiceOverType.Ask && !x.Status.IsEnded))
		{
			Schedule(voId, eventId, type, target);
			return true;
		}
		return false;
	}

	private void Schedule(string voId, string eventId, VoiceOverType type, GameObject target)
	{
		m_Schedulled.TryAdd(voId, new List<VoiceOverPlayingEntry.StaticData>());
		m_Schedulled[voId].Add(new VoiceOverPlayingEntry.StaticData
		{
			VoId = voId,
			EventId = eventId,
			Type = type,
			Target = target
		});
		PFLog.VO.Log("[VO] Запланирован VO на " + voId + " " + eventId);
	}

	public void PauseBarks()
	{
		foreach (VoiceOverPlayingEntry entry in Entries)
		{
			if (!entry.Status.IsEnded)
			{
				entry.Status.Pause();
			}
		}
	}

	public void ResumeBarks()
	{
		foreach (VoiceOverPlayingEntry entry in Entries)
		{
			if (!entry.Status.IsEnded)
			{
				entry.Status.Resume();
			}
		}
	}
}
