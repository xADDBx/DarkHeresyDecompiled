using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Obsolete("New Elevators")]
[ComponentName("Command/CommandMarkOnElevatorSpan")]
[TypeId("bc6b97d0e8d8be94aa5102b16fe751df")]
public class CommandMarkOnElevatorSpan : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override bool IsContinuous => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = value;
		commandData.Unit?.Features.OnElevator.Retain();
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Unit?.Features.OnElevator.Release();
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "Mark " + Unit?.GetCaptionShort() + " <b>on elevator</b>";
	}
}
