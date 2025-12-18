using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Movement/ForceMoveTriggerTarget")]
[TypeId("4deeabe4c9204704808e07f7716895e2")]
public class ForceMoveTriggerTarget : ForceMoveTrigger, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber
{
	public ActionList Actions;

	protected override void OnTrigger(RulePerformAttack rule)
	{
		if (base.Fact.MaybeContext != null)
		{
			base.Fact.RunActionInContext(Actions, rule.ConcreteTarget.ToITargetWrapper());
		}
		else
		{
			Actions.Run();
		}
	}

	public void OnEventAboutToTrigger(RulePerformAttack rule)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack rule)
	{
		TryTrigger(rule);
	}
}
