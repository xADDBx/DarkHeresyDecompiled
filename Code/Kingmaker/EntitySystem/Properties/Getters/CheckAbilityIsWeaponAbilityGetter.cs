using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("3d3ea03bc11a4a68ba661ee1b53388a8")]
public class CheckAbilityIsWeaponAbilityGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public bool OnlyHasAttackDelivery;

	protected override bool GetBaseValue()
	{
		if (OnlyHasAttackDelivery && base.PropertyContext.Ability != null)
		{
			return base.PropertyContext.Ability.Blueprint.ComponentsArray.Any((BlueprintComponent p) => p is AbilityAttackDelivery);
		}
		return this.GetAbilityWeapon() != null;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability comes from a weapon or uses a weapon";
	}
}
