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
[TypeId("7609dc343119477fbeb5e7db815d7c9b")]
public class OverpenetrationModifierTarget : OverpenetrationModifier, ITargetRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, ITargetRulebookSubscriber
{
	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon rule)
	{
		Apply(rule);
	}

	public void OnEventDidTrigger(RuleCalculateStatsWeapon evt)
	{
	}
}
