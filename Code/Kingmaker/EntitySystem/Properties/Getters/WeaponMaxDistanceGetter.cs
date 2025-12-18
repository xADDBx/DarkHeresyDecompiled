using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("709bbf9dd09c0fc44a3f8586c5728ba6")]
public class WeaponMaxDistanceGetter : IntPropertyGetter
{
	[SerializeField]
	private bool SecondWeapon;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!SecondWeapon)
		{
			return "First Weapon of " + FormulaTargetScope.Current + " Max Distance";
		}
		return "Second Weapon of " + FormulaTargetScope.Current + " Max Distance";
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return 0;
		}
		ItemEntityWeapon maybeWeapon = baseUnitEntity.Body.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon maybeWeapon2 = baseUnitEntity.Body.SecondaryHand.MaybeWeapon;
		if (!SecondWeapon && maybeWeapon == null)
		{
			return 0;
		}
		if (SecondWeapon && maybeWeapon2 == null)
		{
			return 0;
		}
		if (!SecondWeapon)
		{
			return maybeWeapon.Blueprint.WarhammerMaxDistance;
		}
		return maybeWeapon2.Blueprint.WarhammerMaxDistance;
	}
}
