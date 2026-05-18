using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandMarkPartyHiddenSpan")]
[TypeId("41eebb0155a1c8b4da2a075f27e56850")]
public class CommandMarkPartyHiddenSpan : CommandBase
{
	[SerializeField]
	private Player.CharactersList m_UnitsList;

	public bool NoFadeOut;

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
				characters?.Features.Hidden.Retain();
				if (NoFadeOut && characters?.View?.Fader != null)
				{
					characters.View.Fader.Visible = false;
					characters.View.Fader.FastForward();
				}
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
		}
		foreach (BaseUnitEntity characters in Game.Instance.Player.GetCharactersList(m_UnitsList))
		{
			if (!list.Contains(characters))
			{
				characters?.Features.Hidden.Release();
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
		return "Mark party <b>hidden</b>";
	}
}
