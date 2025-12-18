using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.Runtime.Core.Utility;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("9a266991f02ed7b4485aa76eb5a9a09f")]
public class WarhammerAbilityIsUsingMelee : BlueprintComponent, IAbilityRestriction
{
	public string GetAbilityRestrictionUIText()
	{
		return "Must use melee weapon";
	}

	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		return ability.Weapon?.Blueprint.IsMelee ?? false;
	}
}
