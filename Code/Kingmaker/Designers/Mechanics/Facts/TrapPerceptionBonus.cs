using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[ComponentName("Buff on spawned unit")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("783eb40c7bc69c047b363f92908f7632")]
public class TrapPerceptionBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateSkillCheck>, IRulebookHandler<RuleCalculateSkillCheck>, ISubscriber, IInitiatorRulebookSubscriber
{
	public ModifierDescriptor Descriptor;

	public ContextValue Value;

	void IRulebookHandler<RuleCalculateSkillCheck>.OnEventAboutToTrigger(RuleCalculateSkillCheck evt)
	{
	}

	void IRulebookHandler<RuleCalculateSkillCheck>.OnEventDidTrigger(RuleCalculateSkillCheck evt)
	{
	}
}
