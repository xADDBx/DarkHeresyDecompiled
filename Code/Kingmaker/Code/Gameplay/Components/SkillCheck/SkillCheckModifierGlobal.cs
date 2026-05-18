using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.SkillCheck;

[Serializable]
[ComponentName("SkillCheck/SkillCheckModifierGlobal")]
[TypeId("1ab4cec70e1f47cbaa48e9efd817ddec")]
public sealed class SkillCheckModifierGlobal : SkillCheckModifier, IGlobalRulebookHandler<RuleCalculateSkillCheck>, IRulebookHandler<RuleCalculateSkillCheck>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RuleCalculateSkillCheck>.OnEventAboutToTrigger(RuleCalculateSkillCheck evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateSkillCheck>.OnEventDidTrigger(RuleCalculateSkillCheck evt)
	{
	}
}
