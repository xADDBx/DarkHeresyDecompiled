using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Serializable]
[AllowMultipleComponents]
[ComponentName("Damage/DamageModifierInitiator")]
[TypeId("cf4a4d014c2610548a3f9213c3de882e")]
public sealed class DamageModifierInitiator : DamageModifier, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber
{
	protected override StatModifierScope Scope => StatModifierScope.Against;

	void IRulebookHandler<RuleCalculateDamage>.OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateDamage>.OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}
}
