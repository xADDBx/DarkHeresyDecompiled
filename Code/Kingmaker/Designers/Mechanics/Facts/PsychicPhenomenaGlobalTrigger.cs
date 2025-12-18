using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[TypeId("635dc8ac062d4b6b911d8a84c7ce7832")]
public class PsychicPhenomenaGlobalTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformPsychicPhenomena>, IRulebookHandler<RulePerformPsychicPhenomena>, ISubscriber, IGlobalRulebookSubscriber, IForcedPsychicPhenomenaHandler, ISubscriber<IBaseUnitEntity>
{
	public ActionList Actions;

	public ActionList OnlyPhenomenaActions;

	public ActionList OnlyPerilsActions;

	public void OnEventAboutToTrigger(RulePerformPsychicPhenomena evt)
	{
	}

	public void OnEventDidTrigger(RulePerformPsychicPhenomena evt)
	{
		RunActions(evt.ResultIsPerils);
	}

	private void RunActions(bool isPerilsOfTheWarp)
	{
		base.Fact.RunActionInContext(Actions, base.Owner);
		base.Fact.RunActionInContext(isPerilsOfTheWarp ? OnlyPerilsActions : OnlyPhenomenaActions, base.Owner);
	}

	public void HandleForcedPsychicPhenomena(bool isPerilsOfTheWarp)
	{
		RunActions(isPerilsOfTheWarp);
	}
}
