using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("b686e6d4f01af2244bdef9f165f2c511")]
public class CheckAbilityWeaponChosenGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public bool SpecificWeapon;

	[SerializeField]
	[ShowIf("SpecificWeapon")]
	private BlueprintItemWeaponReference[] m_Weapons;

	public ReferenceArrayProxy<BlueprintItemWeapon> Weapons
	{
		get
		{
			BlueprintReference<BlueprintItemWeapon>[] weapons = m_Weapons;
			return weapons;
		}
	}

	protected override bool GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = EvalContext.Current.AbilityWeapon;
		ItemEntityWeapon itemEntityWeapon = base.CurrentEntity.GetOptional<WarhammerUnitPartChooseWeapon>()?.ChosenWeapon;
		if (abilityWeapon == null || itemEntityWeapon == null)
		{
			return false;
		}
		if (!SpecificWeapon)
		{
			return abilityWeapon == itemEntityWeapon;
		}
		return Weapons.Contains(itemEntityWeapon.Blueprint);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon Family";
	}
}
