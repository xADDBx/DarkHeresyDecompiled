using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("d5a557904cf7461f90f423f098160e66")]
public class CheckAbilityWeaponClassificationGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public WeaponClassification[] Classifications;

	protected override bool GetBaseValue()
	{
		if (EvalContext.Current.AbilityWeapon != null)
		{
			return Classifications.HasItem(EvalContext.Current.AbilityWeapon.Blueprint.Classification);
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon Family";
	}
}
