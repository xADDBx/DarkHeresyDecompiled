using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Gameplay.Components.Abilities;

[Serializable]
[ComponentName("Custom/Krootox/AbilityKrootoxChargeRestriction")]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("9eaa9f1802e84faaa2f084945ed1d4c6")]
public class AbilityKrootoxChargeRestriction : BlueprintComponent, IAbilityTargetRestriction
{
	private int MinRangeCells => ((BlueprintAbility)base.OwnerBlueprint).MinRange;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper targetWrapper, Vector3 casterPosition)
	{
		LocalizedString failReason;
		return CheckTargetRestriction(ability, targetWrapper, casterPosition, out failReason);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper targetWrapper, Vector3 casterPosition)
	{
		CheckTargetRestriction(ability, targetWrapper, casterPosition, out var failReason);
		return failReason?.Text;
	}

	private bool CheckTargetRestriction(AbilityData ability, TargetWrapper target, Vector3 casterPosition, [CanBeNull] out LocalizedString failReason)
	{
		GridNodeBase nearestNode = target.NearestNode;
		GridNodeBase nearestNodeXZUnwalkable = casterPosition.GetNearestNodeXZUnwalkable();
		if (!CalculatePath(ability, nearestNode, nearestNodeXZUnwalkable, ((BlueprintAbility)base.OwnerBlueprint).MinRange, out var pathNodes))
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.CanNotReachTarget;
			return false;
		}
		GridNodeBase gridNodeBase = pathNodes.LastItem();
		if (gridNodeBase == null || nearestNodeXZUnwalkable.CellDistanceTo(gridNodeBase) < MinRangeCells)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (!pathNodes.Empty())
		{
			MechanicEntity caster = ability.Caster;
			List<GridNodeBase> list = pathNodes;
			if (!caster.CanStandHere(list[list.Count - 1]))
			{
				failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.CanNotReachTarget;
				return false;
			}
		}
		Vector3 to = ((ability.TargetAnchor == AbilityTargetAnchor.Point || target.Entity == null) ? nearestNode.Vector3Position() : target.Entity.GetInnerNodeNearestToTarget(target.Point).Vector3Position());
		if (WarhammerGeometryUtils.DistanceToInCells(nearestNodeXZUnwalkable.Vector3Position(), ability.Caster.SizeRect, to, default(IntRect)) > ability.RangeCells)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
			return false;
		}
		failReason = null;
		return true;
	}

	private static bool CalculatePath(AbilityData ability, GridNodeBase targetNode, GridNodeBase casterNode, int minRangeCells, out List<GridNodeBase> pathNodes)
	{
		List<GridNodeBase> item = AbilityCustomMovementHelper.GetOrientedPatternAndPath(ability, casterNode, targetNode, minRangeCells, coveredTargetsOnly: false, stepThroughTarget: false, stopOnFirstEncounter: true, ignoreEnemies: false, ignoreAllies: false, isCharge: true, stopBeforeTargetNode: true).Path;
		pathNodes = item;
		return pathNodes.HasItem((GridNodeBase i) => i != casterNode);
	}
}
