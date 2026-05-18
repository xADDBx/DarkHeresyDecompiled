using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("8dd4b186d1504ef0a4c0165d3ee1c287")]
public class ConcentratedFireInitiator : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	private int? m_CountTargets;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (!(evt.Ability == null) && m_Restrictions.IsPassed(base.Context, null, null, evt) && evt.Initiator is BaseUnitEntity)
		{
			int num = m_CountTargets ?? ((ContextData<EnemyTargetsInPatternData>.Current != null) ? ContextData<EnemyTargetsInPatternData>.Current.EnemyTargetsInPattern : 0);
			int num2 = ((num > 0) ? (50 + 10 * GetBallisticSkillBonus(evt.ConcreteInitiator) / num) : 0);
			if (num2 != 0)
			{
				evt.Modifiers.Add(ModifierType.PctAdd, num2, base.Fact);
			}
		}
	}

	private static int GetBallisticSkillBonus(Entity entity)
	{
		return (entity as MechanicEntity)?.Actor.GetStatBonus(StatType.BallisticSkill) ?? 0;
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		if (!(context.Ability == null) && m_Restrictions.IsPassed(context, base.Owner) && context.Caster is BaseUnitEntity caster)
		{
			OrientedPatternData orientedPatternData = context.Pattern;
			m_CountTargets = GetEnemyTargetCountInPattern(in orientedPatternData, caster);
		}
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		if (!(context.Ability == null) && m_Restrictions.IsPassed(context, base.Owner) && context.Caster is BaseUnitEntity)
		{
			m_CountTargets = null;
		}
	}

	private static int GetEnemyTargetCountInPattern(in OrientedPatternData orientedPatternData, BaseUnitEntity caster)
	{
		int num = 0;
		foreach (GridNodeBase node in orientedPatternData.Nodes)
		{
			BaseUnitEntity firstUnit = node.GetFirstUnit();
			if (firstUnit != null && firstUnit != caster && !firstUnit.IsAlly(caster))
			{
				num++;
			}
		}
		return num;
	}
}
