using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Target Restriction/AbilityTargetIsReachable")]
[TypeId("bc23117a326f4ecebd9ac8ac952d6eca")]
public class AbilityTargetIsReachable : BlueprintComponent, IAbilityTargetRestriction
{
	public bool CheckNodeOnUnitOccupation;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (target.NearestNode.Area == casterPosition.GetNearestNodeXZ()?.Area)
		{
			if (CheckNodeOnUnitOccupation)
			{
				return !Game.Instance.EntityPools.AllBaseAwakeUnits.Any((BaseUnitEntity checkedUnit) => !checkedUnit.LifeState.IsDead && checkedUnit.GetOccupiedNodes().Any((GridNodeBase occupiedNode) => occupiedNode == target.NearestNode));
			}
			return true;
		}
		return false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return ConfigRoot.Instance.LocalizedTexts.Reasons.CanNotReachTarget;
	}
}
