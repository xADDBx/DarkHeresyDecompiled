using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Obsolete]
[AllowMultipleComponents]
[TypeId("4e6fe9e1395573f458bafaa76364f65d")]
public class AddInitiatorSkillRollTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, ISubscriber, IInitiatorRulebookSubscriber
{
	public bool OnlySuccess;

	public StatType Skill;

	public ActionList Action;

	void IRulebookHandler<RulePerformSkillCheck>.OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
	}

	void IRulebookHandler<RulePerformSkillCheck>.OnEventDidTrigger(RulePerformSkillCheck evt)
	{
		if (CheckConditions(evt) && base.Fact.MaybeContext != null)
		{
			base.Fact.RunActionInContext(Action, base.OwnerTargetWrapper);
		}
	}

	private bool CheckConditions(RulePerformSkillCheck evt)
	{
		if (OnlySuccess && !evt.ResultIsSuccess)
		{
			return false;
		}
		if (Skill != 0 && Skill != evt.StatType)
		{
			return false;
		}
		return true;
	}
}
