using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("4d6a30178423cf44aa80c44e2f8573e8")]
public class CheckFirstWeaponAbilityGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbilityWeapon
{
	protected override int GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		BlueprintAbilityWrapper blueprintAbilityWrapper = this.GetAbility()?.Blueprint;
		if (abilityWeapon == null || blueprintAbilityWrapper == null)
		{
			return 0;
		}
		if (!blueprintAbilityWrapper.SameAbility(abilityWeapon.Blueprint.WeaponAbilities.Ability1.Ability))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability is first weapon ability";
	}
}
