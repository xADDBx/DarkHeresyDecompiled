using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Target Restriction/WarhammerAbilityTargetIsReachableByCaster")]
[TypeId("64b9b460b89091f43b1a76354b5ae77f")]
public class WarhammerAbilityTargetIsReachableByCaster : BlueprintComponent, IAbilityTargetRestriction
{
	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (ability.Caster.MaybeMovementAgent == null)
		{
			return false;
		}
		ForcedPath forcedPath = PathfindingService.Instance.FindPathRT_Blocking(ability.Caster.MaybeMovementAgent, target.Point, 10000f);
		bool result = !forcedPath.error;
		PathPool.Pool(forcedPath);
		return result;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return "<target is not reachable>";
	}
}
