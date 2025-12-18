using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("a4fd87c241554f3f8e5e1a10af29b5e2")]
[ComponentName("Combat/InitiativeBonus")]
public class InitiativeBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleRollInitiative>, IRulebookHandler<RuleRollInitiative>, ISubscriber, IInitiatorRulebookSubscriber
{
	public ContextValue Value = 0;

	public void OnEventAboutToTrigger(RuleRollInitiative evt)
	{
		evt.Modifiers.Add(Value.Calculate(base.Context), base.Fact);
	}

	public void OnEventDidTrigger(RuleRollInitiative evt)
	{
	}
}
