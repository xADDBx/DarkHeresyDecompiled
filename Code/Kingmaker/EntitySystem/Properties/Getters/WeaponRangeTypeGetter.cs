using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("145fae42674897643a03d1d165941210")]
public class WeaponRangeTypeGetter : BoolPropertyGetter
{
	private enum WeaponRangeType
	{
		Melee,
		Ranged
	}

	[SerializeField]
	private bool SecondWeapon;

	[SerializeField]
	private WeaponRangeType m_RangeType;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = ((m_RangeType == WeaponRangeType.Melee) ? "Melee" : "Ranged");
		if (!SecondWeapon)
		{
			return "First Weapon of " + FormulaTargetScope.Current + " Range is " + text;
		}
		return "Second Weapon of " + FormulaTargetScope.Current + " Range is " + text;
	}

	protected override bool GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		ItemEntityWeapon itemEntityWeapon = (SecondWeapon ? baseUnitEntity.Body.SecondaryHand.MaybeWeapon : baseUnitEntity.Body.PrimaryHand.MaybeWeapon);
		if (itemEntityWeapon == null)
		{
			return false;
		}
		bool isMelee = itemEntityWeapon.Blueprint.IsMelee;
		if (!isMelee || m_RangeType != 0)
		{
			if (!isMelee)
			{
				return m_RangeType == WeaponRangeType.Ranged;
			}
			return false;
		}
		return true;
	}
}
