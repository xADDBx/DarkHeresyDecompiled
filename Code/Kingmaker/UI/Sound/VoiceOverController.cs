using System;
using System.Collections.Generic;
using Code.Editor;
using JetBrains.Annotations;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Sound.Base;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.UI.Sound;

public class VoiceOverController : IControllerTick, IController, IDialogFinishHandler, ISubscriber, IGameModeHandler, IBarkBanterPlayedHandler
{
	private VoiceOverTracker m_Tracker = new VoiceOverTracker();

	private VoiceOverStatus m_CurrentDialogVoiceOver;

	private VoiceOverStatus m_CurrentStudyVoiceOver;

	public IEnumerable<VoiceOverPlayingEntry> Entries => m_Tracker.Entries;

	public VoiceOverStatus DebugPlayDialogVoiceOver(LocalizedString locString, string voGuid, [CanBeNull] GameObject target = null)
	{
		return PlayDialogVoiceOver(locString, voGuid, target);
	}

	public VoiceOverStatus PlayDialogVoiceOver(LocalizedString localizedString, string voGuid, GameObject target)
	{
		if (localizedString == null)
		{
			PFLog.VO.Error("[VO] Can't play VO on null localizedString!");
			return null;
		}
		if (string.IsNullOrEmpty(voGuid))
		{
			PFLog.VO.Error("[VO] Can't play VO on null VoGUID");
			return null;
		}
		VoiceOverStatus currentDialogVoiceOver = m_CurrentDialogVoiceOver;
		if (currentDialogVoiceOver != null && !currentDialogVoiceOver.IsEnded)
		{
			m_CurrentDialogVoiceOver.Stop();
		}
		m_CurrentDialogVoiceOver = PlayVoiceOver(localizedString, voGuid, VoiceOverType.Dialog, target);
		return m_CurrentDialogVoiceOver;
	}

	[Obsolete]
	public VoiceOverStatus PlayVoiceOver(LocalizedString locString, BlueprintUnit blueprintUnit, VoiceOverType voiceOverType, [CanBeNull] GameObject target = null)
	{
		string voGuidByBlueprintName = VOSettings.Instance.GetVoGuidByBlueprintName(blueprintUnit.name);
		return PlayVoiceOver(locString, voGuidByBlueprintName, voiceOverType, target);
	}

	public bool CanPlayAsk(string voGuid, GameObject target)
	{
		string voIdByGuid = VOSettings.Instance.GetVoIdByGuid(voGuid);
		return m_Tracker.CanPlayAsk(voIdByGuid, target);
	}

	public VoiceOverStatus PlayVoiceOver(LocalizedString locString, string voGuid, VoiceOverType voiceOverType, [CanBeNull] GameObject target = null)
	{
		if (locString == null)
		{
			PFLog.VO.Error("[VO] Can't play VO on null localizedString!");
			return null;
		}
		if (string.IsNullOrEmpty(voGuid))
		{
			PFLog.VO.Error("[VO] Can't play VO on null VoGUID");
			return null;
		}
		string voIdByGuid = VOSettings.Instance.GetVoIdByGuid(voGuid);
		string key = locString.Key;
		if (!SoundEventsManager.TryGetVoiceOverEvent(key, voIdByGuid, out var eventId))
		{
			PFLog.VO.Error("[VO] No event for string " + key + " for VoId: " + voIdByGuid);
			return null;
		}
		return PlayVoiceOver(eventId, voGuid, voiceOverType, target);
	}

	public VoiceOverStatus PlayVoiceOver(string soundEventId, string voGuid, VoiceOverType voiceOverType, [CanBeNull] GameObject target = null)
	{
		if (string.IsNullOrEmpty(voGuid))
		{
			PFLog.VO.Error("[VO] Can't play VO on null VoGUID");
			return null;
		}
		string voIdByGuid = VOSettings.Instance.GetVoIdByGuid(voGuid);
		if (!m_Tracker.CanPlayVoiceOver(voIdByGuid, voiceOverType, target))
		{
			return null;
		}
		VoiceOverStatus voiceOverStatus = m_Tracker.PlayVoiceOver(soundEventId, voIdByGuid, voiceOverType, target);
		if (voiceOverStatus != null)
		{
			OnVoiceOverStarted(voIdByGuid, voiceOverType);
		}
		return voiceOverStatus;
	}

	private void OnVoiceOverStarted(string voId, VoiceOverType type)
	{
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		m_Tracker.Tick();
	}

	public void HandleBarkBanter(BlueprintBarkBanter barkBanter)
	{
		foreach (BlueprintUnit item in barkBanter.AllSpeakers())
		{
			string voIdByBlueprint = VOSettings.Instance.GetVoIdByBlueprint(item);
			if (m_Tracker.CanPlayVoiceOver(voIdByBlueprint, VoiceOverType.Banter, null))
			{
				m_Tracker.StopAllOnVoId(voIdByBlueprint, null);
			}
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Dialog)
		{
			m_Tracker.StopAllVoiceOverExceptOfType(VoiceOverType.Dialog);
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Dialog)
		{
			if (Game.Instance.CurrentGameMode?.Type == GameModeType.Cutscene)
			{
				m_Tracker.SaveInterruptedDialogVoices();
			}
			else
			{
				m_Tracker.SavedDialogEntries.Clear();
			}
			m_Tracker.StopAllDialogVoices();
		}
		else if (gameMode == GameModeType.Cutscene && Game.Instance.CurrentGameMode?.Type == GameModeType.Dialog)
		{
			m_Tracker.ResumeSavedDialogVoices();
		}
	}

	public static string GetVoGuidBySourceAndTarget(IBarkSource source, Entity target)
	{
		string text = "";
		if (target is AbstractUnitEntity abstractUnitEntity)
		{
			text = abstractUnitEntity.VoGuid;
		}
		else if (target is MapObjectEntity mapObjectEntity && mapObjectEntity.View.NeedsVoiceOver)
		{
			text = mapObjectEntity.View.VoId.Guid;
		}
		else if (target is DetectiveTraceRootEntity || target is DetectiveTraceEntity)
		{
			text = VOSettings.Instance.ServoSkull.Blueprint.VoId.Guid;
		}
		if ((source.IsVoIdForced && source.ForcedVoGuids.Contains(text)) || !string.IsNullOrEmpty(text))
		{
			return text;
		}
		PFLog.VO.Error("[VO] Can't decide VoId in " + source.ToString() + " for " + target?.UniqueId + " : " + target?.View?.GameObjectName);
		return "";
	}

	public void HandleDialogFinished(BlueprintDialog dialog, bool success)
	{
		VoiceOverStatus currentDialogVoiceOver = m_CurrentDialogVoiceOver;
		if (currentDialogVoiceOver != null && !currentDialogVoiceOver.IsEnded)
		{
			m_CurrentDialogVoiceOver.Stop();
		}
	}

	public uint PlayAsk(string eventId, string voGuid, GameObject target, Action<object, AkCallbackType, AkCallbackInfo> callback)
	{
		string voIdByGuid = VOSettings.Instance.GetVoIdByGuid(voGuid);
		return m_Tracker.PlayAsk(eventId, voIdByGuid, target, callback);
	}

	public void PlayStudyVoiceOver(BlueprintClueStudy study)
	{
		m_Tracker.StopAllVoiceOver();
		string empty = string.Empty;
		m_CurrentStudyVoiceOver = PlayVoiceOver(voGuid: (!(study.StudyCompanion != null)) ? VOSettings.Instance.ServoSkull.Blueprint.VoId.Guid : study.StudyCompanion.Blueprint.VoId.Guid, locString: study.StudyBark, voiceOverType: VoiceOverType.Dialog);
	}

	public void StopStudyVoiceOver()
	{
		VoiceOverStatus currentStudyVoiceOver = m_CurrentStudyVoiceOver;
		if (currentStudyVoiceOver != null && !currentStudyVoiceOver.IsEnded)
		{
			m_CurrentStudyVoiceOver.Stop();
		}
		m_CurrentStudyVoiceOver = null;
	}

	public void PauseBarks()
	{
		m_Tracker.PauseBarks();
		SoundEventsManager.PostEvent("PauseBarks", null);
	}

	public void ResumeBarks()
	{
		m_Tracker.ResumeBarks();
		SoundEventsManager.PostEvent("ResumeBarks", null);
	}

	public void ScheduleAskAfterBark(AbstractUnitEntity toAbstractUnitEntity, IBarkHandle handle)
	{
		handle.AddCallback(delegate
		{
			ScheduleAskTracesFound(toAbstractUnitEntity);
		});
	}

	public void ScheduleAskTracesFound(AbstractUnitEntity toAbstractUnitEntity)
	{
		toAbstractUnitEntity.View.Asks?.TracesFound.Schedule();
	}
}
