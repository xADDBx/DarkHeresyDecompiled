using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Obsolete]
[TypeId("2b7e896fea233934d9e41502f583205c")]
public class AddInitiatorSavingThrowTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber
{
	public bool OnlyPass;

	public bool OnlyFail;

	public ActionList Action;

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
	{
		if (CheckConditions(evt) && base.Fact.MaybeContext != null)
		{
			base.Fact.RunActionInContext(Action, base.OwnerTargetWrapper);
		}
	}

	private bool CheckConditions(RulePerformSavingThrow evt)
	{
		return false;
	}
}
