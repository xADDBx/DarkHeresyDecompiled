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
[ComponentName("Damage/DamageModifierTarget")]
[TypeId("aac928a1f314a4144923e7e3850a24ba")]
public sealed class DamageModifierTarget : DamageModifier, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, ITargetRulebookSubscriber
{
	protected override StatModifierScope Scope => StatModifierScope.Owner;

	void IRulebookHandler<RuleCalculateDamage>.OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateDamage>.OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}
}
