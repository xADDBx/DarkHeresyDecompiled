using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("323b0da7a07e4dc7b3e5311d3c609ff6")]
public class CheckAbilityWeaponCategoryGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public WeaponCategory[] Categories;

	protected override bool GetBaseValue()
	{
		if (this.GetAbilityWeapon() != null)
		{
			return Categories.HasItem(this.GetAbilityWeapon().Blueprint.Category);
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon Category";
	}
}
