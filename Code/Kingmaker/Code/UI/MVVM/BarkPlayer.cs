using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Sound;
using Kingmaker.UI.Sound.Base;

namespace Kingmaker.Code.UI.MVVM;

public static class BarkPlayer
{
	public const int CallbackTimeout = 60;

	public static IBarkHandle Bark(Entity entity, LocalizedString text, VoiceOverType voiceOverType, string voGuid, float duration = -1f, BaseUnitEntity interactUser = null, bool synced = true, bool forceShow = false)
	{
		if (text == null)
		{
			return null;
		}
		using (GameLogContext.Scope)
		{
			if (entity is MechanicEntity mechanicEntity)
			{
				GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)mechanicEntity;
			}
			string text2 = text.Text;
			if (string.IsNullOrEmpty(text2))
			{
				return null;
			}
			return Bark(entity, text2, voiceOverType, voGuid, duration, text, interactUser, synced, forceShow);
		}
	}

	public static IBarkHandle Bark(Entity entity, string text, VoiceOverType voiceOverType, string voGuid, float duration = -1f, LocalizedString localizedString = null, BaseUnitEntity interactUser = null, bool synced = true, bool forceShow = false)
	{
		if (entity == null)
		{
			return null;
		}
		if (!entity.IsInGame || (!forceShow && !entity.IsVisibleForPlayer))
		{
			return null;
		}
		if (Game.Instance.Controllers.BarkController.IsSuppressed)
		{
			return null;
		}
		VoiceOverStatus voiceOverStatus = null;
		if (localizedString != null)
		{
			string text2 = voGuid;
			if (string.IsNullOrEmpty(text2))
			{
				if (entity is AbstractUnitEntity abstractUnitEntity)
				{
					text2 = abstractUnitEntity.VoGuid;
				}
				else if (entity is MapObjectEntity mapObjectEntity && mapObjectEntity.View.NeedsVoiceOver)
				{
					text2 = mapObjectEntity.View.VoId.Guid;
				}
			}
			voiceOverStatus = Game.Instance.Controllers.VoiceOverController.PlayVoiceOver(localizedString, text2, voiceOverType, entity.View?.GO);
		}
		EventBus.RaiseEvent((IEntity)entity, (Action<ICombatLogBarkHandler>)delegate(ICombatLogBarkHandler h)
		{
			h.HandleOnShowBark(text);
		}, isCheckRuntime: true);
		return new BarkHandle(entity, text, duration, voiceOverStatus, synced);
	}

	public static IBarkHandle BarkSubtitle([CanBeNull] Entity entity, LocalizedString text, VoiceOverType voiceOverType, string voGuid, float duration = -1f, LocalizedString speakerName = null)
	{
		if (Game.Instance.Controllers.BarkController.IsSuppressed)
		{
			return null;
		}
		using (GameLogContext.Scope)
		{
			if (entity is MechanicEntity mechanicEntity)
			{
				GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)mechanicEntity;
			}
			string subtitleText = GetSubtitleText(entity, speakerName, text.Text);
			VoiceOverStatus voiceOverStatus = null;
			string voGuid2 = voGuid;
			if (string.IsNullOrEmpty(voGuid))
			{
				if (entity is AbstractUnitEntity abstractUnitEntity)
				{
					voGuid2 = abstractUnitEntity.VoGuid;
				}
				else if (entity is MapObjectEntity mapObjectEntity && mapObjectEntity.View.NeedsVoiceOver)
				{
					voGuid2 = mapObjectEntity.View.VoId.Guid;
				}
			}
			voiceOverStatus = Game.Instance.Controllers.VoiceOverController.PlayVoiceOver(text, voGuid2, voiceOverType, entity?.View?.GO);
			return new SubtitleBarkHandle(subtitleText, duration, voiceOverStatus);
		}
	}

	private static string GetSubtitleText([CanBeNull] Entity entity, [CanBeNull] LocalizedString speakerName, string text)
	{
		if (speakerName != null && !speakerName.IsEmpty())
		{
			return speakerName.Text + ": " + text;
		}
		if (entity == null || !(entity is MechanicEntity mechanicEntity))
		{
			return text;
		}
		return mechanicEntity.Name + ": " + text;
	}

	private static string GetSubtitleText(LocalizedString speakerName, string text)
	{
		if (speakerName != null && !speakerName.IsEmpty())
		{
			return speakerName.Text + ": " + text;
		}
		return text;
	}

	private static IBarkHandle BarkExploration(Entity entity, LocalizedString text, string voGuid, float duration = -1f)
	{
		VoiceOverStatus voiceOverStatus = null;
		if (entity is AbstractUnitEntity abstractUnitEntity)
		{
			string voGuid2 = (string.IsNullOrEmpty(voGuid) ? abstractUnitEntity.VoGuid : voGuid);
			voiceOverStatus = Game.Instance.Controllers.VoiceOverController.PlayVoiceOver(text, voGuid2, VoiceOverType.Bark, entity.View?.GO);
		}
		return new BarkHandle(entity, text, duration, voiceOverStatus);
	}

	public static IBarkHandle BarkExploration(Entity entity, LocalizedString text, VoiceOverType voiceOverType, string voGuid, string encyclopediaLink, float duration = -1f)
	{
		VoiceOverStatus voiceOverStatus = null;
		if (entity is AbstractUnitEntity abstractUnitEntity)
		{
			string voGuid2 = (string.IsNullOrEmpty(voGuid) ? abstractUnitEntity.VoGuid : voGuid);
			voiceOverStatus = Game.Instance.Controllers.VoiceOverController.PlayVoiceOver(text, voGuid2, voiceOverType, entity?.View?.GO);
		}
		return new BarkHandle(entity, text, encyclopediaLink, duration, voiceOverStatus);
	}
}
