using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("3584620744644ad6932525e79c8ca8e6")]
public class CheckAbilityWeaponIsSourceOfFactGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalFact
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is Fact From Weapon";
	}

	protected override bool GetBaseValue()
	{
		MechanicEntityFact fact = EvalContext.Current.Fact;
		ItemEntityWeapon abilityWeapon = EvalContext.Current.AbilityWeapon;
		if (fact != null && abilityWeapon != null)
		{
			return fact.IsFrom(abilityWeapon);
		}
		return false;
	}
}
