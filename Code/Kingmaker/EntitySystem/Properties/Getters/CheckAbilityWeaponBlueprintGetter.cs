using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("83a8a95dacc24c6f8bb7fdccf7ec43ea")]
public class CheckAbilityWeaponBlueprintGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[SerializeField]
	[InfoBox("Checks whether the ability weapon OR its prototypes match the specified blueprint")]
	private BlueprintItemWeaponReference m_Weapon;

	private BlueprintItemWeapon Weapon => m_Weapon?.Get();

	protected override bool GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		for (BlueprintItemWeapon blueprintItemWeapon = abilityWeapon?.Blueprint?.PrototypeLink as BlueprintItemWeapon; blueprintItemWeapon != null; blueprintItemWeapon = (BlueprintItemWeapon)blueprintItemWeapon.PrototypeLink)
		{
			if (blueprintItemWeapon == Weapon)
			{
				return true;
			}
		}
		return abilityWeapon?.Blueprint == Weapon;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability weapon OR prototype BP is " + (Weapon?.name ?? "null");
	}
}
