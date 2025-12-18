using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.UnitLogic.FactLogic;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Serializable]
[Obsolete("Use DamageTriggerTarget instead")]
[AllowMultipleComponents]
[TypeId("03db0cc2e8cca5f4ea4e29fd197ff65c")]
public class WarhammerDamageTriggerTarget : WarhammerDamageTrigger, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber
{
	public ActionList Actions;

	public ActionList ActionsOnAttacker;

	public ContextPropertyName ContextPropertyName;

	public KillTrigger.PropertyParameter PropertyToSave;

	public void OnEventAboutToTrigger(RuleDealDamage rule)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage rule)
	{
		TryTrigger(rule);
	}

	protected override void OnTrigger(RuleDealDamage rule)
	{
		if (PropertyToSave != 0)
		{
			if (PropertyToSave == KillTrigger.PropertyParameter.EnemyDifficulty)
			{
				base.Context[ContextPropertyName] = ((int?)(rule.Target as UnitEntity)?.Blueprint.DifficultyType).GetValueOrDefault();
			}
			if (PropertyToSave == KillTrigger.PropertyParameter.Damage)
			{
				base.Context[ContextPropertyName] = rule.ResultValue;
			}
			if (PropertyToSave == KillTrigger.PropertyParameter.DamageOverflow)
			{
				int hPBeforeDamage = rule.HPBeforeDamage;
				base.Context[ContextPropertyName] = Math.Max(rule.ResultValue - hPBeforeDamage, 0);
			}
		}
		if (base.Fact.MaybeContext != null)
		{
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
			Actions.Run();
		}
	}
}
