using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintToggleAbility))]
[AllowMultipleComponents]
[ComponentName("Caster Restriction/AbilityCasterMainWeaponIsMelee")]
[TypeId("d1981651ef7aa384ca2fb43930b81032")]
public class AbilityCasterMainWeaponIsMelee : BlueprintComponent, IAbilityCasterRestriction
{
	public bool CanBeSecondaryWeapon;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		PartUnitBody bodyOptional = caster.GetBodyOptional();
		bool flag = bodyOptional != null && bodyOptional.PrimaryHand.HasWeapon && bodyOptional.PrimaryHand.Weapon.Blueprint.IsMelee;
		if (CanBeSecondaryWeapon)
		{
			flag |= bodyOptional != null && bodyOptional.SecondaryHand.HasWeapon && bodyOptional.SecondaryHand.Weapon.Blueprint.IsMelee;
		}
		return flag;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.MeleeWeaponRequired;
	}

	public IEnumerable<string> GetAbilityCasterRestrictionShortUITexts(MechanicEntity caster)
	{
		bool flag = IsCasterRestrictionPassed(caster);
		CasterRestrictionsStrings casterRestrictionsStrings = ConfigRoot.Instance.LocalizedTexts.CasterRestrictionsStrings;
		yield return flag ? casterRestrictionsStrings.MeleeWeaponEquiped : casterRestrictionsStrings.MeleeWeaponNotEquiped;
	}
}
