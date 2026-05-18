using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Obsolete("New Elevators")]
[ComponentName("Command/CommandMarkPartyOnElevatorSpan")]
[TypeId("6e990ca6d8d542743ac9c572e869c9d7")]
public class CommandMarkPartyOnElevatorSpan : CommandBase
{
	[SerializeField]
	private Player.CharactersList m_UnitsList;

	[SerializeReference]
	public AbstractUnitEvaluator[] ExceptThese;

	public override bool IsContinuous => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (AbstractUnitEvaluator item2 in ElementExtendAsObject.Valid(ExceptThese))
		{
			if (item2.TryGetValue(out var value) && value is BaseUnitEntity item)
			{
				list.Add(item);
			}
			else
			{
				PFLog.Cutscene.Error($"[IS NOT BASE UNIT ENTITY] Cutscene command {this}, {item2} is not BaseUnitEntity");
			}
		}
		foreach (BaseUnitEntity characters in Game.Instance.Player.GetCharactersList(m_UnitsList))
		{
			if (!list.Contains(characters))
			{
				characters?.Features.OnElevator.Retain();
			}
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (AbstractUnitEvaluator item2 in ElementExtendAsObject.Valid(ExceptThese))
		{
			if (item2.TryGetValue(out var value) && value is BaseUnitEntity item)
			{
				list.Add(item);
			}
			else
			{
				PFLog.Cutscene.Error($"[IS NOT BASE UNIT ENTITY] Cutscene command {this}, {item2} is not BaseUnitEntity");
			}
		}
		foreach (BaseUnitEntity characters in Game.Instance.Player.GetCharactersList(m_UnitsList))
		{
			if (!list.Contains(characters))
			{
				characters?.Features.OnElevator.Release();
			}
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
		return "Mark party <b>on elevator</b>";
	}
}
