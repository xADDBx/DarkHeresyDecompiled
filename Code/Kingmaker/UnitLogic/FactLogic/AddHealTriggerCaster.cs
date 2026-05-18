using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Heal/AddHealTriggerCaster")]
[TypeId("3630e15006f937a4c8e3f7c461613b89")]
public class AddHealTriggerCaster : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>, ISubscriber, IInitiatorRulebookSubscriber
{
	public RestrictionCalculator Restrictions;

	public ActionList Action;

	public bool EvenOnZeroHeal;

	public void OnEventAboutToTrigger(RuleHealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleHealDamage evt)
	{
		if ((evt.Value > 0 || EvenOnZeroHeal) && Restrictions.IsPassed(base.Context, null, null, evt))
		{
			base.Fact.RunActionInContext(Action);
		}
	}
}
