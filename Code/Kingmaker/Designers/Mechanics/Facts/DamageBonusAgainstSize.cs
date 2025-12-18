using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("bc27cedfd3b9067429f761205435e48a")]
[ComponentName("Damage/DamageBonusAgainstSize")]
public class DamageBonusAgainstSize : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber
{
	public ContextValue Value;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool UseForDifferentSizes;

	[HideIf("UseForDifferentSizes")]
	public Size EnemySize;

	[ShowIf("UseForDifferentSizes")]
	public List<Size> ValidEnemySizes = new List<Size>();

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (Restrictions.IsPassed(base.Context, null, null, evt))
		{
			ItemEntityWeapon itemEntityWeapon = evt.Ability?.Weapon;
			if ((!SpecificRangeType || (itemEntityWeapon != null && WeaponRangeType.IsSuitableWeapon(itemEntityWeapon))) && evt.MaybeTarget is UnitEntity unitEntity && (UseForDifferentSizes ? ValidEnemySizes.Contains(unitEntity.Size) : (EnemySize == unitEntity.Size)))
			{
				evt.Modifiers.Add(ModifierType.ValAdd, Value.Calculate(base.Context), base.Fact);
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}
}
