using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("3670afde0e8b4c418aba24b22927d2d8")]
public class CheckAbilityWeaponIsOneHanded : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		return this.GetAbilityWeapon()?.HoldInTwoHands ?? false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon is One-Handed";
	}
}
