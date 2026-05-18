using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("4a65f37aabb64aaa8d459f405fb5e369")]
public class CheckAbilityWeaponTwoHandedGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		return EvalContext.Current.AbilityWeapon?.Blueprint.IsTwoHanded ?? false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon Family";
	}
}
