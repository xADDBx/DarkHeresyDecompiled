using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("9dd5c8ea0cade7141b71b41fb84fa1e2")]
public class WarhammerHitChancePenalty : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, IInitiatorRulebookSubscriber
{
	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public ContextValue Value;

	public ContextValue Multiplier;

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		ItemEntityWeapon weapon = evt.Ability.Weapon;
		if (!SpecificRangeType || (weapon != null && WeaponRangeType.IsSuitableWeapon(weapon)))
		{
			evt.Modifiers.Add(ModifierType.ValAdd, -Value.Calculate(base.Context) * Multiplier.Calculate(base.Context), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}
}
