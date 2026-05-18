using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Obsolete]
[TypeId("89daaef26f568254eb67814e4d11b872")]
public class CommandMarkPassiveSpan : CommandBase
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
			return CommandResult.Fail("Unit not found");
		}
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = value;
		commandData.Unit?.Passive.Retain();
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Unit?.Passive.Release();
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
		return "Mark " + Unit?.GetCaptionShort() + " <b>passive</b>";
	}
}
