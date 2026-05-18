using System;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Signals;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Obsolete]
[TypeId("de5b317dc48ef484193a356ca566cb7e")]
public class CommandBarkUnit : CommandBase
{
	private class Data
	{
		internal float Delay;

		internal bool Finished;

		internal IBarkHandle BarkHandle;

		internal SignalWrapper StopPlaySignal;
	}

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	[ValidateNotNull]
	public LocalizedString SharedText;

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText = true;

	[Tooltip("Wait until bark disappears before starting next command")]
	public bool AwaitFinish;

	[Tooltip("Extra delay time before starting next command. Can be negative.")]
	[FormerlySerializedAs("DelayTime")]
	public float CommandDurationShift;

	[Tooltip("If true, speaker is considered controlled by the cutscene")]
	public bool ControlsUnit = true;

	[Tooltip("If true, override unit bark and broadcast it to subtitle")]
	public bool IsSubText;

	[Tooltip("Allow set exact playback time")]
	public bool OverrideBarkDuration;

	[Tooltip("Exact playback time")]
	[ShowIf("OverrideBarkDuration")]
	public float BarkDuration;

	public bool IsUnitOptional;

	[SerializeField]
	[ShowIf("IsSubText")]
	[CanBeNull]
	private LocalizedString m_SpeakerName;

	public override bool ShouldHaveControlledUnit => ControlsUnit;

	protected override bool StopPlaySignalIsReady(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		return SignalService.Instance.CheckReadyOrSend(ref commandData.StopPlaySignal);
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.BarkHandle?.InterruptBark();
		commandData.Delay = 0f;
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
		AbstractUnitEntity value = null;
		if (IsUnitOptional)
		{
			Unit?.TryGetValue(out value);
		}
		else
		{
			value = Unit?.GetValue();
		}
		if (IsSubText)
		{
			commandData.BarkHandle = BarkPlayer.BarkSubtitle(value, SharedText, VoiceOverType.Bark, "", duration, m_SpeakerName);
		}
		else if (value != null && value.LifeState.IsConscious)
		{
			commandData.BarkHandle = BarkPlayer.Bark(value, SharedText, VoiceOverType.Bark, "", duration);
		}
		commandData.Delay = CommandDurationShift;
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
		if (commandData.BarkHandle == null)
		{
			commandData.Finished = true;
		}
		else if (time >= (double)commandData.Delay && (!AwaitFinish || !commandData.BarkHandle.IsPlayingBark()))
		{
			commandData.Finished = true;
		}
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return Unit?.GetCaptionShort() + "<b> bark</b> " + SharedText;
	}

	public override string GetWarning()
	{
		if (Unit == null)
		{
			if (!IsSubText)
			{
				return "No unit";
			}
			return null;
		}
		if (!Unit.CanEvaluate())
		{
			return "No unit";
		}
		return null;
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!ControlsUnit || !Unit || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}
}
