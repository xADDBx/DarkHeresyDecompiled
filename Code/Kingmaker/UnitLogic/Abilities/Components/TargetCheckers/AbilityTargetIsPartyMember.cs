using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[Obsolete]
[ComponentName("Predicates/Target has fact")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("c593a8b96d9421043b4033b92ea3117c")]
public class AbilityTargetIsPartyMember : BlueprintComponent, IAbilityTargetRestriction
{
	public bool Not;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (!Not)
		{
			if (target.Entity != null)
			{
				return target.Entity.IsPlayerFaction;
			}
			return false;
		}
		if (target.Entity != null)
		{
			return !target.Entity.IsPlayerFaction;
		}
		return true;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return Not ? ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsNotPartyMember : ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsPartyMember;
	}
}
