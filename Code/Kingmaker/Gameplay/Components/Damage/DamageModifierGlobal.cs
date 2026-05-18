using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Damage;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components.Damage;

[Serializable]
[AllowMultipleComponents]
[ComponentName("Damage/DamageModifierGlobal")]
[TypeId("9645f12ca0104274b735bbae1a05a688")]
public sealed class DamageModifierGlobal : DamageModifier, IGlobalRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IGlobalRulebookSubscriber, IStatModifier
{
	protected override StatModifierScope Scope => StatModifierScope.Global;

	void IRulebookHandler<RuleCalculateDamage>.OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		TryApply(evt);
	}

	void IRulebookHandler<RuleCalculateDamage>.OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}
}
