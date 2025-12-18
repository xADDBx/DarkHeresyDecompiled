using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ComponentName("CriticalEffects/CriticalEffectsTriggerGlobal")]
[TypeId("28b9812c07884e14a94f7d74b6f3a10a")]
public sealed class CriticalEffectsTriggerGlobal : CriticalEffectsTrigger, IGlobalRulebookHandler<RulePerformCriticalEffects>, IRulebookHandler<RulePerformCriticalEffects>, ISubscriber, IGlobalRulebookSubscriber
{
	void IRulebookHandler<RulePerformCriticalEffects>.OnEventAboutToTrigger(RulePerformCriticalEffects evt)
	{
	}

	void IRulebookHandler<RulePerformCriticalEffects>.OnEventDidTrigger(RulePerformCriticalEffects evt)
	{
		TryApply(evt);
	}
}
