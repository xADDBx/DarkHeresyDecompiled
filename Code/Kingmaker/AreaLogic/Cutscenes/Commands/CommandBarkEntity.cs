using System.Collections.Generic;
using System.Linq;
using Code.Editor;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Signals;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandBarkEntity")]
[TypeId("9b492d9943834d39ac4333d13cdb42a2")]
public class CommandBarkEntity : CommandBase, IBarkSource
{
	private class Data
	{
		internal bool Finished;

		internal IBarkHandle BarkHandle;

		internal SignalWrapper StopPlaySignal;
	}

	private const int CaptionTextLength = 15;

	[ValidateNotNull]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public LocalizedString SharedText;

	public bool ForceVoId;

	[ShowIf("ForceVoId")]
	public List<VoIdField> ForcedVoIds = new List<VoIdField>();

	public bool IsSpammable;

	public VoiceOverActAs ActAs;

	[SerializeReference]
	public EntityEvaluator Entity;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText = true;

	[Tooltip("Wait until bark disappears before starting next command")]
	public bool AwaitFinish;

	[Tooltip("Allow set exact playback time. Can't be less than VoiceOver duration")]
	public bool OverrideBarkDuration;

	[Tooltip("Exact playback time. Can't be less than VoiceOver duration")]
	[ShowIf("OverrideBarkDuration")]
	public float BarkDuration;

	[Tooltip("If true, override bark and broadcast it to subtitle")]
	public bool IsSubText;

	[SerializeField]
	[ShowIf("IsSubText")]
	[CanBeNull]
	private LocalizedString m_SpeakerName;

	public IEnumerable<LocalizedString> Barks => new LocalizedString[1] { SharedText };

	public bool IsVoIdForced => ForceVoId;

	public List<string> ForcedVoGuids => ForcedVoIds.Select((VoIdField f) => f.Guid).ToList();

	public bool Spammable => IsSpammable;

	protected override bool StopPlaySignalIsReady(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		return SignalService.Instance.CheckReadyOrSend(ref commandData.StopPlaySignal);
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).BarkHandle?.InterruptBark();
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).BarkHandle?.InterruptBark();
		return CommandResult.Success;
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		float duration = UtilityBark.DefaultBarkTime;
		if (BarkDurationByText)
		{
			duration = UtilityBark.GetBarkDuration(SharedText);
		}
		if (OverrideBarkDuration)
		{
			duration = BarkDuration;
		}
		Entity value = null;
		if (Entity != null && !Entity.TryGetValue(out value))
		{
			return CommandResult.Fail("Cant find entity");
		}
		if (value is AbstractUnitEntity { IsDead: not false })
		{
			return CommandResult.Fail("Cant bark on dead unit");
		}
		string voGuid = ((!IsSubText || value != null || !IsVoIdForced || ForcedVoGuids.Count <= 0) ? VoiceOverController.GetVoGuidBySourceAndTarget(this, value) : ForcedVoGuids[0]);
		if (IsSubText)
		{
			commandData.BarkHandle = BarkPlayer.BarkSubtitle(value, SharedText, (VoiceOverType)ActAs, voGuid, duration, m_SpeakerName);
		}
		else
		{
			commandData.BarkHandle = BarkPlayer.Bark(value, SharedText, (VoiceOverType)ActAs, voGuid, duration);
		}
		commandData.StopPlaySignal = SignalService.Instance.RegisterNext();
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Finished = true;
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!AwaitFinish)
		{
			commandData.Finished = true;
			return CommandResult.Success;
		}
		if (commandData.BarkHandle == null)
		{
			if (time > (double)UtilityBark.DefaultBarkTime)
			{
				commandData.Finished = true;
			}
		}
		else if (!commandData.BarkHandle.IsPlayingBark())
		{
			commandData.Finished = true;
		}
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		string text = SharedText;
		text = ((text.Length > 15) ? text.Substring(0, 15) : text);
		text = " <b>bark</b> " + text;
		if (Entity != null)
		{
			return Entity.GetCaptionShort() + text;
		}
		if (!IsSubText)
		{
			return "No entity" + text;
		}
		return "Subtitle" + text;
	}

	public override string GetWarning()
	{
		if (Entity == null)
		{
			return "No entity";
		}
		if (!Entity.CanEvaluate())
		{
			return "No entity";
		}
		return null;
	}

	public override void OnValidate()
	{
		base.OnValidate();
	}
}
