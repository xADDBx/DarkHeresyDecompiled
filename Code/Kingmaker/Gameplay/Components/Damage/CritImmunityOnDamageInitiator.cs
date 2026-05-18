using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Damage;

[Serializable]
[ComponentName("Immunity/CritImmunityOnDamageInitiator")]
[TypeId("6adcf19a764c4c12b6f0e78020133354")]
public sealed class CritImmunityOnDamageInitiator : CritImmunityOnDamage, IInitiatorRulebookHandler<RulePerformCriticalEffects>, IRulebookHandler<RulePerformCriticalEffects>, ISubscriber, IInitiatorRulebookSubscriber
{
	void IRulebookHandler<RulePerformCriticalEffects>.OnEventAboutToTrigger(RulePerformCriticalEffects evt)
	{
		TryApplyImmunity(evt);
	}

	void IRulebookHandler<RulePerformCriticalEffects>.OnEventDidTrigger(RulePerformCriticalEffects evt)
	{
	}
}
