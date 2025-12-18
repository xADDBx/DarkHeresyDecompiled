using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("50690cf0ebf34e52840be06f95eb8337")]
[ComponentName("Roll/ChanceRollTriggerInitiator")]
public sealed class ChanceRollTriggerInitiator : ChanceRollTrigger, IInitiatorRulebookHandler<RuleRollChance>, IRulebookHandler<RuleRollChance>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RuleRollChance>.OnEventAboutToTrigger(RuleRollChance evt)
	{
	}

	void IRulebookHandler<RuleRollChance>.OnEventDidTrigger(RuleRollChance evt)
	{
		TryTrigger(evt);
	}
}
