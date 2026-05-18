using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandAttachBuffSpan")]
[TypeId("acf6eb0f9cce4f05a8b81368de1c5f38")]
public class CommandAttachBuffSpan : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;

		public Buff Buff;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public BlueprintBuffReference Buff;

	public override bool IsContinuous => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Unit not found");
		}
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = value;
		commandData.Buff = value.Buffs.Add(Buff.Get(), commandData.Unit);
		commandData.Buff?.AddSource(player);
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.Buff != null)
		{
			commandData.Unit?.Buffs.Remove(commandData.Buff);
		}
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
		return "<b>Buff</b> " + Unit?.GetCaptionShort() + " <b>with</b> " + Buff?.Get().NameSafe();
	}
}
