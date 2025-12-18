using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5f8f336ef74785c44862a41246f50a9b")]
public class FirstWeaponRateOfFireGetter : IntPropertyGetter
{
	public bool ChosenWeapon;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "First weapon of " + FormulaTargetScope.Current + " Rate of Fire";
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return 0;
		}
		ItemEntityWeapon itemEntityWeapon = base.CurrentEntity.GetOptional<WarhammerUnitPartChooseWeapon>()?.ChosenWeapon;
		ItemEntityWeapon maybeWeapon = baseUnitEntity.Body.PrimaryHand.MaybeWeapon;
		if (maybeWeapon == null)
		{
			return 0;
		}
		if (ChosenWeapon && itemEntityWeapon == null)
		{
			return 0;
		}
		if (!ChosenWeapon)
		{
			return maybeWeapon.GetWeaponStats().ResultRateOfFire;
		}
		return itemEntityWeapon.GetWeaponStats().ResultRateOfFire;
	}
}
