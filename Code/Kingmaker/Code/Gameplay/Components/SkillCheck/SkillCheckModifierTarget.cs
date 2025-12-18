using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.SkillCheck;

[Serializable]
[ComponentName("SkillCheck/SkillCheckModifierTarget")]
[TypeId("1bfc8c1bdbd64823b7dcdd4054b8c46f")]
public sealed class SkillCheckModifierTarget : SkillCheckModifier, ITargetRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, ISubscriber, ITargetRulebookSubscriber
{
	void IRulebookHandler<RulePerformSkillCheck>.OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RulePerformSkillCheck>.OnEventDidTrigger(RulePerformSkillCheck evt)
	{
	}
}
