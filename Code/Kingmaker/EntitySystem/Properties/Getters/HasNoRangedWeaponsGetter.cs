using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("250de3495a5143389abd428fcfd0325d")]
public class HasNoRangedWeaponsGetter : BoolPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Both weapons of " + FormulaTargetScope.Current + " are not ranged";
	}

	protected override bool GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		foreach (HandsEquipmentSet handsEquipmentSet in baseUnitEntity.Body.HandsEquipmentSets)
		{
			ItemEntityWeapon maybeWeapon = handsEquipmentSet.PrimaryHand.MaybeWeapon;
			if (maybeWeapon != null && maybeWeapon.Blueprint.IsRanged)
			{
				return false;
			}
			ItemEntityWeapon maybeWeapon2 = handsEquipmentSet.SecondaryHand.MaybeWeapon;
			if (maybeWeapon2 != null && maybeWeapon2.Blueprint.IsRanged)
			{
				return false;
			}
		}
		return true;
	}
}
