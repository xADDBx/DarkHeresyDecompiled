using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.WeaponStats;

[Serializable]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("9f491e61b52742b2b04cc7b0fb35f9c0")]
public class OverpenetrationModifierInitiator : OverpenetrationModifier, IInitiatorRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, IInitiatorRulebookSubscriber
{
	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon rule)
	{
		Apply(rule);
	}

	public void OnEventDidTrigger(RuleCalculateStatsWeapon evt)
	{
	}
}
