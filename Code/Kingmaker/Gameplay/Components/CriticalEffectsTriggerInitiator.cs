using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("CriticalEffects/CriticalEffectsTriggerInitiator")]
[TypeId("1ce818f959244e4b96064b658ed5fc03")]
public sealed class CriticalEffectsTriggerInitiator : CriticalEffectsTrigger, IInitiatorRulebookHandler<RulePerformCriticalEffects>, IRulebookHandler<RulePerformCriticalEffects>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RulePerformCriticalEffects>.OnEventAboutToTrigger(RulePerformCriticalEffects evt)
	{
	}

	void IRulebookHandler<RulePerformCriticalEffects>.OnEventDidTrigger(RulePerformCriticalEffects evt)
	{
		TryApply(evt);
	}
}
