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
[Obsolete]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("4c1eb3ebee3415948864e625dedd7a90")]
public class WeaponStatsModifierTarget : WeaponStatsModifier, ITargetRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, ITargetRulebookSubscriber
{
	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon rule)
	{
	}

	public void OnEventDidTrigger(RuleCalculateStatsWeapon evt)
	{
	}
}
