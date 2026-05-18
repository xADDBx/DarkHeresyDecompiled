using System;
using Kingmaker.Framework.ContextContract;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Attack;

[Serializable]
[TypeId("4ecc6476a5be460799650fdaac16a608")]
[ReadsContext(new ContextField[]
{
	ContextField.Caster,
	ContextField.Owner
})]
public sealed class AttackTriggerInitiator : AttackTrigger, IInitiatorRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, IInitiatorRulebookSubscriber
{
	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
		TryTrigger(evt, before: true);
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		TryTrigger(evt, before: false);
	}
}
