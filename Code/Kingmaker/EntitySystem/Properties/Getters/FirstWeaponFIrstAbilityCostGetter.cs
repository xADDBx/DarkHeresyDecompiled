using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("e2008e6eb60aff048af1e773926fe5b7")]
public class FirstWeaponFIrstAbilityCostGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "First ability of " + FormulaTargetScope.Current + " AP cost";
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return 0;
		}
		ItemEntityWeapon itemEntityWeapon = baseUnitEntity.Body.CurrentHandsEquipmentSet.PrimaryHand.MaybeWeapon;
		if (baseUnitEntity.Commands.Current is UnitUseAbility unitUseAbility)
		{
			itemEntityWeapon = unitUseAbility.Ability.Weapon;
		}
		return itemEntityWeapon?.Blueprint.WeaponAbilities.Ability1.AP ?? 0;
	}
}
