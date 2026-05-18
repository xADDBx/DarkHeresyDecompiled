using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Damage;

[Serializable]
[ComponentName("Immunity/CritImmunityOnDamageTarget")]
[TypeId("10f9e2786ee044c293df079f00dd4614")]
public sealed class CritImmunityOnDamageTarget : CritImmunityOnDamage, ITargetRulebookHandler<RulePerformCriticalEffects>, IRulebookHandler<RulePerformCriticalEffects>, ISubscriber, ITargetRulebookSubscriber
{
	void IRulebookHandler<RulePerformCriticalEffects>.OnEventAboutToTrigger(RulePerformCriticalEffects evt)
	{
		TryApplyImmunity(evt);
	}

	void IRulebookHandler<RulePerformCriticalEffects>.OnEventDidTrigger(RulePerformCriticalEffects evt)
	{
	}
}
