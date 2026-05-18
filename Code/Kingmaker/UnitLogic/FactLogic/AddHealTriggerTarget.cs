using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Heal/AddHealTriggerTarget")]
[TypeId("ebb2957e468e6594c9b7ae0005338984")]
[ContextRoleForField("Action", ContextField.Caster, "buff applier", Note = "reads buff's stored caster")]
[ContextRoleForField("HealerAction", ContextField.Caster, "the healer", Note = "reads heal rule's reason context")]
public class AddHealTriggerTarget : UnitFactComponentDelegate, ITargetRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>, ISubscriber, ITargetRulebookSubscriber
{
	public RestrictionCalculator Restrictions;

	public ActionList Action;

	public ActionList HealerAction;

	public bool EvenOnZeroHeal;

	private void RunAction(RulebookEvent evt)
	{
		base.Fact.RunActionInContext(Action);
		if (HealerAction.HasActions && evt.Reason.Context != null)
		{
			using (EvalContext.PushContext(evt.Reason.Context, base.Owner))
			{
				HealerAction.Run();
			}
		}
	}

	public void OnEventAboutToTrigger(RuleHealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleHealDamage evt)
	{
		if ((evt.Value > 0 || EvenOnZeroHeal) && Restrictions.IsPassed(base.Context, null, null, evt))
		{
			RunAction(evt);
		}
	}
}
