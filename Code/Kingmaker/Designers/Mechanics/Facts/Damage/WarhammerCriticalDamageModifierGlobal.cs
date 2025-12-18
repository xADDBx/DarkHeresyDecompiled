using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Obsolete]
[TypeId("d57dfe1aff99dcd43b58359dc1ef8618")]
public class WarhammerCriticalDamageModifierGlobal : WarhammerCriticalDamageModifier, IGlobalRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IGlobalRulebookSubscriber
{
	public bool OnlyAgainstAllies;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		MechanicEntity initiator = evt.Initiator;
		MechanicEntity maybeTarget = evt.MaybeTarget;
		if (!OnlyAgainstAllies || (maybeTarget != null && initiator != null && initiator.IsAlly(maybeTarget) && maybeTarget.IsAlly(base.Owner)))
		{
			TryApply(evt);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}
}
