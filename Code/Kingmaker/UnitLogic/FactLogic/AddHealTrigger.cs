using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Heal/AddHealTrigger")]
[TypeId("ebb2957e468e6594c9b7ae0005338984")]
public class AddHealTrigger : UnitFactComponentDelegate, ITargetRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>, ISubscriber, ITargetRulebookSubscriber
{
	public ActionList Action;

	public ActionList HealerAction;

	public bool EvenOnZeroHeal;

	private void RunAction(RulebookEvent evt)
	{
		base.Fact.RunActionInContext(Action);
		if (HealerAction.HasActions && evt.Reason.Context != null)
		{
			using (evt.Reason.Context.SetScope(base.OwnerTargetWrapper))
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
		if (evt.Value > 0 || EvenOnZeroHeal)
		{
			RunAction(evt);
		}
	}
}
