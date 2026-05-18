using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("ce34afee78ed4a3590f7d3d9ebd9fa1d")]
public class ContextActionAttackPriorityTarget : ContextAction
{
	public enum PriorityTargetAttackSelectType
	{
		HighestCost,
		LowestCost
	}

	public BpRef<BlueprintBuff> TargetBuff;

	public PriorityTargetAttackSelectType AttackSelectType;

	public override string GetCaption()
	{
		return "Command to attack priority target";
	}

	protected override void RunAction()
	{
		if (!(base.Context.ClickedTarget?.Entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		BaseUnitEntity baseUnitEntity2 = base.Context.Caster?.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
		if (baseUnitEntity2 != null)
		{
			Ability ability = SelectAttackAbility(baseUnitEntity, baseUnitEntity2, AttackSelectType);
			if (ability != null)
			{
				Game.Instance.Controllers.TurnController.ScheduleInterruptTurn(baseUnitEntity, base.Context.Caster, new InterruptionData());
				baseUnitEntity.Commands.AddToQueue(new UnitUseAbilityParams(ability.Data, baseUnitEntity2)
				{
					IgnoreCooldown = true,
					FreeAction = true
				});
				baseUnitEntity.Commands.AddToQueue(new UnitEndTurnParams());
			}
		}
	}

	public static Ability SelectAttackAbility(MechanicEntity target, BaseUnitEntity priorityTarget, PriorityTargetAttackSelectType attackSelectType)
	{
		IEnumerable<Ability> all = target.Facts.GetAll(delegate(Ability i)
		{
			if (i.Blueprint.GetComponent<AbilityAttackDelivery>() == null)
			{
				return false;
			}
			if (i.Data.GetPatternSettings() != null)
			{
				return false;
			}
			if (!(i.SourceItem is ItemEntityWeapon))
			{
				return false;
			}
			return !i.Data.IsRestricted && i.Data.RangeCells >= target.DistanceToInCells(priorityTarget);
		});
		return attackSelectType switch
		{
			PriorityTargetAttackSelectType.HighestCost => all.MaxBy((Ability p) => p.Data.CalculateActionPointCost()), 
			PriorityTargetAttackSelectType.LowestCost => all.MinBy((Ability p) => p.Data.CalculateActionPointCost()), 
			_ => all.FirstOrDefault(), 
		};
	}
}
