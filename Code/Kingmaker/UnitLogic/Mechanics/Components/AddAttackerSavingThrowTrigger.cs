using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Obsolete]
[AllowMultipleComponents]
[TypeId("aa361ba866af4258ade424a4951bae3f")]
public class AddAttackerSavingThrowTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IGlobalRulebookSubscriber
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
		if ((evt.Reason.Ability?.Caster == base.Owner || evt.Reason.Caster == base.Owner) && CheckConditions(evt) && base.Fact.MaybeContext != null)
		{
			base.Fact.RunActionInContext(Action, evt.InitiatorUnit.ToITargetWrapper());
		}
	}

	private bool CheckConditions(RulePerformSavingThrow evt)
	{
		return false;
	}
}
