using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("53ddc60b996d4bd2aa81e66b49d5f97d")]
public class CheckAbilityWeaponRangeTypeGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	private enum WeaponRangeType
	{
		Melee,
		Ranged
	}

	[SerializeField]
	private WeaponRangeType m_RangeType;

	protected override bool GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		if (abilityWeapon == null)
		{
			return false;
		}
		bool isMelee = abilityWeapon.Blueprint.IsMelee;
		if ((isMelee && m_RangeType == WeaponRangeType.Melee) || (!isMelee && m_RangeType == WeaponRangeType.Ranged))
		{
			return true;
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = ((m_RangeType == WeaponRangeType.Melee) ? "Melee" : "Ranged");
		return "Ability Weapon Range is " + text;
	}
}
