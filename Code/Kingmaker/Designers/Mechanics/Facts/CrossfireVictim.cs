using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[ComponentName("Custom/Crossfire/CrossfireVictim")]
[TypeId("4859177b499340209476ac31d0f16345")]
public class CrossfireVictim : UnitBuffComponentDelegate, IRoundStartHandler, ISubscriber, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ITargetRulebookSubscriber
{
	public int ProvokeRange;

	public ContextActionAttackPriorityTarget.PriorityTargetAttackSelectType AttackSelectType;

	private bool m_OnCooldown;

	void IRoundStartHandler.HandleRoundStart(bool turnBased)
	{
		if (turnBased)
		{
			m_OnCooldown = false;
		}
	}

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		if (m_OnCooldown || !base.Owner.IsConscious)
		{
			return;
		}
		List<BaseUnitEntity> source = EntityBoundsHelper.FindUnitsInRange(base.Owner.Position, ProvokeRange.Cells().Meters);
		if (source.Empty())
		{
			return;
		}
		IEnumerable<BaseUnitEntity> enumerable = source.Where((BaseUnitEntity u) => u.Facts.HasComponent<CrossfireAgressor>() && u != evt.InitiatorUnit);
		if (enumerable.Empty())
		{
			return;
		}
		foreach (BaseUnitEntity item in enumerable)
		{
			Ability ability = SelectAttackAbility(item, base.Owner, AttackSelectType);
			if (ability != null)
			{
				m_OnCooldown = true;
				UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability.Data, base.Owner)
				{
					IgnoreCooldown = true,
					FreeAction = true
				};
				item.Commands.AddToQueue(cmdParams);
			}
		}
	}

	public static Ability SelectAttackAbility(MechanicEntity target, BaseUnitEntity priorityTarget, ContextActionAttackPriorityTarget.PriorityTargetAttackSelectType attackSelectType)
	{
		IEnumerable<Ability> all = target.Facts.GetAll(delegate(Ability p)
		{
			if (p.Blueprint.GetComponent<AbilityAttackDelivery>() == null)
			{
				return false;
			}
			if (p.Data.GetPatternSettings() != null)
			{
				return false;
			}
			ItemEntity itemEntity = (ItemEntity)p.SourceItem;
			if (itemEntity != null && !(itemEntity.Blueprint is BlueprintItemWeapon))
			{
				return false;
			}
			return !p.Data.IsRestricted && p.Data.CanTarget(priorityTarget);
		});
		return attackSelectType switch
		{
			ContextActionAttackPriorityTarget.PriorityTargetAttackSelectType.HighestCost => all.MaxBy((Ability p) => p.Data.CalculateActionPointCost()), 
			ContextActionAttackPriorityTarget.PriorityTargetAttackSelectType.LowestCost => all.MinBy((Ability p) => p.Data.CalculateActionPointCost()), 
			_ => all.FirstOrDefault(), 
		};
	}
}
