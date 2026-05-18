using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandAddFact")]
[TypeId("0d949fdd9cb63b94db868d35dcd1fec7")]
public class CommandAddFact : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;

		public EntityFact Fact;
	}

	[SerializeField]
	[FormerlySerializedAs("Fact")]
	private BlueprintUnitFactReference m_Fact;

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Unit not found");
		}
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = value;
		using (ContextData<CommandAction.PlayerData>.Request().Setup(player))
		{
			commandData.Fact = value.AddFact(Fact);
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.Unit == null || commandData.Fact == null)
		{
			return CommandResult.Fail("Unit or fact not found");
		}
		commandData.Unit.Facts.Remove(commandData.Fact);
		commandData.Unit = null;
		commandData.Fact = null;
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
}
