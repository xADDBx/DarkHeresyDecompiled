using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandMarkUntargetableSpan")]
[TypeId("4516700b4f7648ad8e4346cbaa3caa63")]
public class CommandMarkUntargetableSpan : CommandBase
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
		commandData.Unit?.Features.IsUntargetable.Retain();
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Unit?.Features.IsUntargetable.Release();
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
		return "Mark " + Unit?.GetCaptionShort() + " <b>untargetable</b>";
	}
}
