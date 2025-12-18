using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("ac51167116d84a73aab80c41972980c0")]
[ComponentName("Roll/ChanceRollTriggerGlobal")]
public sealed class ChanceRollTriggerGlobal : ChanceRollTrigger, IGlobalRulebookHandler<RuleRollChance>, IRulebookHandler<RuleRollChance>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RuleRollChance>.OnEventAboutToTrigger(RuleRollChance evt)
	{
	}

	void IRulebookHandler<RuleRollChance>.OnEventDidTrigger(RuleRollChance evt)
	{
		TryTrigger(evt);
	}
}
