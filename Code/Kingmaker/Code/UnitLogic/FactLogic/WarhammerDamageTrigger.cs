using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic.FactLogic;

[Serializable]
[Obsolete("Use DamageTrigger instead")]
[TypeId("e0ecefa49eeb4f80a63dba55e4f9dfd8")]
public abstract class WarhammerDamageTrigger : UnitFactComponentDelegate
{
	private static readonly HashSet<EntityFactComponent> TriggeringNow = new HashSet<EntityFactComponent>();

	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool TriggersForDamageOverTime;

	protected void TryTrigger(RuleDealDamage rule)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Context, null, null, rule))
			{
				return;
			}
		}
		BlueprintScriptableObject blueprintScriptableObject = rule.Reason.Context?.Blueprint;
		if ((!(blueprintScriptableObject is BlueprintBuff) && !(blueprintScriptableObject is BlueprintAreaEffect)) || TriggersForDamageOverTime)
		{
			if (TriggeringNow.Contains(base.Runtime))
			{
				throw new Exception($"Cycled trigger: {base.Fact}.{name}");
			}
			try
			{
				TriggeringNow.Add(base.Runtime);
				OnTrigger(rule);
			}
			finally
			{
				TriggeringNow.Remove(base.Runtime);
			}
			base.ExecutesCount++;
		}
	}

	protected abstract void OnTrigger(RuleDealDamage rule);
}
