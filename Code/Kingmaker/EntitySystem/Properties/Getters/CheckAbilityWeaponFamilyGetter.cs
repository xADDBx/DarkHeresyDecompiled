using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("ba648cd4033740aa987af4b641d583de")]
public class CheckAbilityWeaponFamilyGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public WeaponFamily[] Families;

	protected override bool GetBaseValue()
	{
		if (EvalContext.Current.AbilityWeapon != null)
		{
			return Families.HasItem(EvalContext.Current.AbilityWeapon.Blueprint.Family);
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon Family";
	}
}
