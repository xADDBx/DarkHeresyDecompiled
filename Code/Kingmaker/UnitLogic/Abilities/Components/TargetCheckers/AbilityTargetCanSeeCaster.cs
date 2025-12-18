using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[Obsolete]
[ComponentName("Predicates/Target can see caster")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("8507226f5bdd6a24bae48dcbcbfe0958")]
public class AbilityTargetCanSeeCaster : BlueprintComponent, IAbilityTargetRestriction
{
	public bool Not;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		PartCombatGroup partCombatGroup = target.Entity?.GetCombatGroupOptional();
		if (partCombatGroup != null)
		{
			bool flag = partCombatGroup.Memory.ContainsVisible(ability.Caster);
			if (!Not)
			{
				return flag;
			}
			return !flag;
		}
		return false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return Not ? ConfigRoot.Instance.LocalizedTexts.Reasons.TargetCanNotSeeCaster : ConfigRoot.Instance.LocalizedTexts.Reasons.TargetCanSeeCaster;
	}
}
