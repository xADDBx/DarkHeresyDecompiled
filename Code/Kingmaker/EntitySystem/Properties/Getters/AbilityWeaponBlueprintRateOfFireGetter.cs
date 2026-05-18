using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("0ab3ba348c2e46ff9a6623fce1e3ac84")]
public class AbilityWeaponBlueprintRateOfFireGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		return EvalContext.Current.AbilityWeapon?.RateOfFire ?? 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Unmodified Rate of Fire (entity, PL-aware)";
	}
}
