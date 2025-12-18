using System;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[TypeId("b584e6818b924d83b4299130eea4e552")]
public class PsychicPhenomenaCasterTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformPsychicPhenomena>, IRulebookHandler<RulePerformPsychicPhenomena>, ISubscriber, IInitiatorRulebookSubscriber
{
	public ActionList Actions;

	public ActionList OnlyPhenomenaActions;

	public ActionList OnlyPerilsActions;

	public void OnEventAboutToTrigger(RulePerformPsychicPhenomena evt)
	{
	}

	public void OnEventDidTrigger(RulePerformPsychicPhenomena evt)
	{
		base.Fact.RunActionInContext(Actions, base.Owner);
		base.Fact.RunActionInContext(evt.ResultIsPerils ? OnlyPerilsActions : OnlyPhenomenaActions, base.Owner);
	}
}
