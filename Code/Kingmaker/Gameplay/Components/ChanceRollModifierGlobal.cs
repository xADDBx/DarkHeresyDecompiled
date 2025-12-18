using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("64938cc990f7459e8ae8219238ed53e8")]
[ComponentName("Roll/ChanceRollModifierGlobal")]
public sealed class ChanceRollModifierGlobal : ChanceRollModifier, IGlobalRulebookHandler<RuleRollChance>, IRulebookHandler<RuleRollChance>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RuleRollChance>.OnEventAboutToTrigger(RuleRollChance evt)
	{
		TryApplyBefore(evt);
	}

	void IRulebookHandler<RuleRollChance>.OnEventDidTrigger(RuleRollChance evt)
	{
		TryApplyAfter(evt);
	}
}
