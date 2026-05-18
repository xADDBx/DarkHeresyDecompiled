using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.Pathfinding;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[TypeId("644f628e3cd8eee419717e45d4f7a834")]
public class AbilityCustomRam : AbilityCustomLogic, IAbilityTargetRestriction, IAbilityCustomAnimation, IAbilityAoEPatternProvider
{
	[SerializeField]
	private int m_MinRange;

	[SerializeField]
	private int m_MaxRange;

	[SerializeField]
	private bool m_OverrideSpeed;

	[SerializeField]
	[ShowIf("m_OverrideSpeed")]
	private float m_MaxSpeedOverride = 6f;

	[SerializeField]
	private bool m_RamThrough;

	[SerializeField]
	private ActionList m_EndPointActions;

	[SerializeField]
	[ShowIf("m_RamThrough")]
	private ActionList m_RamThroughActions;

	public override bool IsEngageUnit => true;

	public override bool IsMoveUnit => true;

	public bool IsIgnoreLos => false;

	public bool UseMeleeLos => false;

	public bool IsIgnoreLevelDifference => false;

	public int PatternAngle => 0;

	bool IAbilityAoEPatternProvider.CalculateAttackFromPatternCentre => false;

	TargetType IAbilityAoEPatternProvider.Targets => TargetType.Any;

	public AoEPattern Pattern => null;

	public int? HaloSize => null;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		yield break;
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
		(context.Caster as BaseUnitEntity).MovementAgent.MaxSpeedOverride = null;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		CheckTargetRestriction(ability.Caster, target, casterPosition, out var failReason);
		return failReason;
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		LocalizedString failReason;
		return CheckTargetRestriction(ability.Caster, target, casterPosition, out failReason);
	}

	private bool CheckTargetRestriction(MechanicEntity caster, TargetWrapper target, Vector3 casterPosition, [CanBeNull] out LocalizedString failReason)
	{
		int num = caster.DistanceToInCells(target.Point);
		if (num < m_MinRange)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (num > m_MaxRange)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
			return false;
		}
		if (ObstacleAnalyzer.TraceAlongNavmesh(casterPosition, target.Point) != target.Point)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.ObstacleBetweenCasterAndTarget;
			return false;
		}
		GridNodeBase casterNode = (GridNodeBase)ObstacleAnalyzer.GetNearestNode(casterPosition).node;
		GridNodeBase targetNode = (GridNodeBase)ObstacleAnalyzer.GetNearestNode(target.Point).node;
		int num2 = CalcActualRayLength(caster, casterNode, targetNode);
		if (num2 < m_MinRange)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (num2 < num)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
			return false;
		}
		failReason = null;
		return true;
	}

	public UnitAnimationActionLink GetAbilityAction(BaseUnitEntity caster)
	{
		return null;
	}

	public void OverrideHaloSize(int? haloSize)
	{
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		int length = CalcActualRayLength(ability.Caster, casterNode, targetNode);
		return GetOrientedRayPattern(ability.Caster, casterNode, targetNode, length);
	}

	public OrientedPatternData GetOrientedHaloPattern(IAbilityDataProviderForPattern ability, int haloSize, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		return default(OrientedPatternData);
	}

	private OrientedPatternData GetOrientedRayPattern(MechanicEntity caster, GridNodeBase casterNode, GridNodeBase targetNode, int length)
	{
		GridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(targetNode.Vector3Position());
		return AoEPattern.Ray(length).GetOriented(casterNode, targetNode.Vector3Position() - innerNodeNearestToTarget.Vector3Position());
	}

	private int CalcActualRayLength(MechanicEntity caster, GridNodeBase casterNode, GridNodeBase targetNode)
	{
		OrientedPatternData orientedRay = GetOrientedRayPattern(caster, casterNode, targetNode, m_MaxRange);
		List<GridNodeBase> list = new List<GridNodeBase>();
		foreach (GridNodeBase item in orientedRay.Nodes.OrderBy((GridNodeBase x) => x.CellDistanceTo(orientedRay.ApplicationNode)))
		{
			if (list.Count == 0 || list.Last().ContainsConnection(item))
			{
				list.Add(item);
				continue;
			}
			break;
		}
		int num = casterNode.CellDistanceTo(list.Last());
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.EntityPools.AllBaseAwakeUnits)
		{
			if (allBaseAwakeUnit == caster || allBaseAwakeUnit.View == null || allBaseAwakeUnit.View.MovementAgent.AvoidanceDisabled)
			{
				continue;
			}
			GridNodeBase gridNodeBase = (GridNodeBase)(GraphNode)allBaseAwakeUnit.CurrentNode;
			if (list.Contains(gridNodeBase))
			{
				int warhammerCellDistance = GraphHelper.GetWarhammerCellDistance(casterNode, gridNodeBase);
				if (!m_RamThrough && warhammerCellDistance <= num)
				{
					num = warhammerCellDistance - 1;
				}
			}
		}
		if (m_RamThrough && list.Last().ContainsUnit())
		{
			num--;
		}
		return num;
	}
}
