using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandMarkPartyOnPlatform")]
[TypeId("3778265854fc4a5daecd35bd6688c2e8")]
public class CommandMarkPartyOnPlatform : CommandBase
{
	private class Data
	{
		public PlatformObjectEntity Platform;
	}

	[AllowedEntityType(typeof(PlatformObjectView))]
	[ValidateNotEmpty]
	public EntityReference PlatformReference;

	[SerializeField]
	private Player.CharactersList m_UnitsList;

	[SerializeReference]
	public AbstractUnitEvaluator[] ExceptThese;

	public override bool IsContinuous => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Platform = PlatformReference.FindData() as PlatformObjectEntity;
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
				characters.GetOrCreate<EntityPartStayOnPlatform>().SetOnPlatform(commandData.Platform);
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
				characters?.GetOrCreate<EntityPartStayOnPlatform>().ReleaseFromPlatform();
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
		return "Mark party <b>on platform</b>";
	}
}
