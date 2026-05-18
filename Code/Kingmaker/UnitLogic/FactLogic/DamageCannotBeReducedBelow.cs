using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Mechanics.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Damage/DamageCannotBeReducedBelow")]
[TypeId("43b9759cb68c49349492cdde1e29ec7b")]
public class DamageCannotBeReducedBelow : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber
{
	private enum ValueType
	{
		AsIs,
		PercentOfMaxDamage
	}

	private class Scope : ContextFlag<Scope>
	{
	}

	public RestrictionCalculator Restriction = new RestrictionCalculator();

	[SerializeField]
	private ValueType m_ValueType;

	public PropertyCalculator Value;

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if ((bool)ContextData<Scope>.Current)
		{
			return;
		}
		using (ContextData<Scope>.Request())
		{
			if (Restriction.IsPassed(base.Context, null, null, evt))
			{
				int num = ((m_ValueType == ValueType.PercentOfMaxDamage) ? Mathf.RoundToInt((float)Value.GetValue(base.Owner, base.Context, null, evt) / 100f * (float)evt.ResultDamage.GetMaxValueWithoutPenalties()) : Value.GetValue(base.Owner, base.Context, null, evt));
				int num2 = Math.Max(0, num - evt.ResultValue);
				if (num2 > 0)
				{
					IntermediateDamage damage = DamageType.Direct.CreateDamage(num2);
					Rulebook.Trigger(new RuleDealDamage(base.Owner, evt.Target, damage));
				}
			}
		}
	}
}
