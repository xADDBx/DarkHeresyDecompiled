using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[TypeId("40457d87ab694f899cc7523067b09fda")]
public sealed class AbilitySpendChargeModifier : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleSpendAbilityCharge>, IRulebookHandler<RuleSpendAbilityCharge>, ISubscriber, IInitiatorRulebookSubscriber
{
	public enum ModifierType
	{
		NotSpend,
		NotSpendWithChance
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierType Modifier;

	[ShowIf("IsNotSpendWithChance")]
	public ContextValue NotSpendChance = new ContextValue();

	public ActionList ActionsOnApply = new ActionList();

	private bool IsNotSpendWithChance => Modifier == ModifierType.NotSpendWithChance;

	public void OnEventAboutToTrigger(RuleSpendAbilityCharge evt)
	{
		if (Restrictions.IsPassed(base.Context, null, null, evt))
		{
			switch (Modifier)
			{
			case ModifierType.NotSpend:
				evt.NotSpendModifiers.Add(base.Fact);
				break;
			case ModifierType.NotSpendWithChance:
				evt.NotSpendChanceModifiers.Add(NotSpendChance.Calculate(base.Context), base.Fact);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			ActionsOnApply.Run();
		}
	}

	public void OnEventDidTrigger(RuleSpendAbilityCharge evt)
	{
	}
}
