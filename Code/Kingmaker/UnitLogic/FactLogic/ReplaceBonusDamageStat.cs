using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[ComponentName("Add condition")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("960638d10abe4b71a23cad4873b9a5d5")]
public class ReplaceBonusDamageStat : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, IInitiatorRulebookSubscriber
{
	public StatType NewStat;

	public bool ReplaceStrengthForMeleeDamage;

	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon evt)
	{
		if (ReplaceStrengthForMeleeDamage)
		{
			evt.MeleeDamageStats.Add(NewStat);
		}
	}

	public void OnEventDidTrigger(RuleCalculateStatsWeapon evt)
	{
	}
}
