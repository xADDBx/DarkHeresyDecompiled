using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.UnitLogic.FactLogic;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Serializable]
[Obsolete("Use DamageTriggerInitiator instead")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("a5cbfd1546727ec418590630a6ea2400")]
public class WarhammerDamageTriggerInitiator : WarhammerDamageTrigger, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber
{
	public ActionList Actions;

	public ActionList ActionsOnAttacker;

	public ContextPropertyName ContextPropertyName;

	public KillTrigger.PropertyParameter PropertyToSave;

	void IRulebookHandler<RuleDealDamage>.OnEventAboutToTrigger(RuleDealDamage rule)
	{
	}

	void IRulebookHandler<RuleDealDamage>.OnEventDidTrigger(RuleDealDamage rule)
	{
		TryTrigger(rule);
	}

	protected override void OnTrigger(RuleDealDamage rule)
	{
		if (base.Fact.MaybeContext != null)
		{
			if (PropertyToSave != 0)
			{
				if (PropertyToSave == KillTrigger.PropertyParameter.EnemyDifficulty)
				{
					EvalContext.Current[ContextPropertyName] = ((int?)(rule.Target as UnitEntity)?.Blueprint.DifficultyType).GetValueOrDefault();
				}
				if (PropertyToSave == KillTrigger.PropertyParameter.Damage)
				{
					EvalContext.Current[ContextPropertyName] = rule.ResultValue;
				}
				if (PropertyToSave == KillTrigger.PropertyParameter.DamageOverflow)
				{
					int hPBeforeDamage = rule.HPBeforeDamage;
					EvalContext.Current[ContextPropertyName] = Math.Max(rule.ResultValue - hPBeforeDamage, 0);
				}
			}
			ActionList actions = Actions;
			if (actions != null && actions.HasActions)
			{
				base.Fact.RunActionInContext(Actions, rule.ConcreteTarget.ToITargetWrapper());
			}
			actions = ActionsOnAttacker;
			if (actions != null && actions.HasActions)
			{
				base.Fact.RunActionInContext(ActionsOnAttacker, rule.ConcreteInitiator.ToITargetWrapper());
			}
		}
		else
		{
			Actions?.Run();
		}
	}
}
