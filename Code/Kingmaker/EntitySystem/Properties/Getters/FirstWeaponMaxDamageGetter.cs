using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("4c8cb68a4355e444e87f858307f151c0")]
public class FirstWeaponMaxDamageGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "First weapon of " + FormulaTargetScope.Current + " Max Damage";
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return 0;
		}
		ItemEntityWeapon itemEntityWeapon = baseUnitEntity.Body.PrimaryHand.MaybeWeapon;
		if (baseUnitEntity.Commands.Current is UnitUseAbility unitUseAbility)
		{
			itemEntityWeapon = unitUseAbility.Ability.Weapon;
		}
		return itemEntityWeapon?.DamageMax ?? 0;
	}
}
