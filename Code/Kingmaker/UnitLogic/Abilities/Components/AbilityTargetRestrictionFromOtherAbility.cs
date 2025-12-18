using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[ComponentName("Target Restriction/AbilityTargetRestrictionFromOtherAbility")]
[TypeId("ef3d18bc1e0e4ce5a5d5debc78423d7b")]
public class AbilityTargetRestrictionFromOtherAbility : BlueprintComponent, IAbilityTargetRestriction
{
	public BlueprintAbilityReference Ability;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return Ability.Get().GetComponents<IAbilityTargetRestriction>().All((IAbilityTargetRestriction x) => x.IsTargetRestrictionPassed(ability, target, casterPosition));
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return Ability.Get().GetComponents<IAbilityTargetRestriction>().First()
			.GetAbilityTargetRestrictionUIText(ability, target, casterPosition);
	}
}
