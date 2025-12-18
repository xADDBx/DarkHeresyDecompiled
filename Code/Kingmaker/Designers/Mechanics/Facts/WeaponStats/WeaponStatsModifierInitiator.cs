using System;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.WeaponStats;

[Serializable]
[TypeId("28fa403fa7a94a2f9ea361583e66cdb4")]
public class WeaponStatsModifierInitiator : WeaponStatsModifier, IInitiatorRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, IInitiatorRulebookSubscriber
{
	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateStatsWeapon evt)
	{
	}
}
