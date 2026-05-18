using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Localization;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[ComponentName("Ability/Movement/AbilityCustomDirectMovement")]
[TypeId("8cbc9755b89b4a81bf497fb24c1144c0")]
public class AbilityCustomDirectMovement : AbilityCustomLogic, IAbilityAoEPatternProvider, IAbilityTargetRestriction
{
	public bool StepThroughTarget;

	public bool MustStandInTarget;

	[HideIf("StepThroughTarget")]
	public bool StopOnFirstEncounter;

	[ShowIf("ShowIgnoreFields")]
	public bool IgnoreEnemies;

	[ShowIf("ShowIgnoreFields")]
	public bool IgnoreAllies;

	[HideIf("StopOnFirstEncounter")]
	public bool IgnoreAllTargetsExceptLast;

	public bool DamageAllUnitsInLine;

	public bool DisableAttacksOfOpportunity;

	public bool IsCharge;

	[ShowIf("IsCharge")]
	[SerializeField]
	private bool m_OnlyValidIfHitTheTarget;

	public ActionList ActionsOnEncounteredTarget;

	public ActionList ActionsOnCaster;

	[SerializeField]
	private BlueprintBuffReference m_BuffOnMovement;

	public BlueprintBuff BuffOnMovement => m_BuffOnMovement.Get();

	public bool IsIgnoreLos => false;

	public bool UseMeleeLos => false;

	public bool IsIgnoreLevelDifference => false;

	public int PatternAngle => 0;

	public bool CalculateAttackFromPatternCentre => false;

	TargetType IAbilityAoEPatternProvider.Targets => TargetType.Any;

	public AoEPattern Pattern => null;

	public int? HaloSize => null;

	public int MinRangeCells => ((BlueprintAbility)base.OwnerBlueprint).MinRange;

	public override bool IsMoveUnit => true;

	public override bool IsEngageUnit => true;

	private bool ShowIgnoreFields
	{
		get
		{
			if (!StepThroughTarget)
			{
				return StopOnFirstEncounter;
			}
			return false;
		}
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper clickedTarget)
	{
		MechanicEntity caster = context.Caster;
		if (caster.MaybeMovementAgent == null)
		{
			PFLog.Default.Error("Movement agent is missing");
			yield break;
		}
		GridNodeBase nearestNode = clickedTarget.NearestNode;
		if (!CalculatePathAndTargets(context.Ability, nearestNode, caster.CurrentUnwalkableNode, out var pathNodes, out var targets))
		{
			PFLog.Ability.ErrorWithReport($"{context.Ability}: can't find path for custom movement");
			yield break;
		}
		Buff buff = caster.Buffs.Add(BuffOnMovement, context, null);
		try
		{
			int pathCellsCount = pathNodes.Count;
			foreach (AbilityDeliveryTarget item in MoveAlongPath(context, ForcedPath.Construct(pathNodes), targets))
			{
				yield return item;
			}
			if (caster is BaseUnitEntity baseUnitEntity)
			{
				baseUnitEntity.CombatState.RegisterMoveCells(pathCellsCount - 1);
			}
			using (EvalContext.PushContext(context, caster))
			{
				ActionsOnCaster.Run();
			}
		}
		finally
		{
			buff?.Remove();
		}
	}

	private bool CalculatePathAndTargets(AbilityData ability, GridNodeBase targetNode, GridNodeBase casterNode, out List<GridNodeBase> pathNodes, out MechanicEntity[] targets)
	{
		MechanicEntity caster = ability.Caster;
		(OrientedPatternData Pattern, List<GridNodeBase> Path) orientedPatternAndPath = GetOrientedPatternAndPath(ability, casterNode, targetNode);
		OrientedPatternData item = orientedPatternAndPath.Pattern;
		List<GridNodeBase> item2 = orientedPatternAndPath.Path;
		pathNodes = item2;
		int limit = (DamageAllUnitsInLine ? int.MaxValue : caster.Size.GetLesserSide());
		targets = GetAllTargetUnits(item, caster, limit);
		return pathNodes.HasItem((GridNodeBase i) => i != casterNode);
	}

	private IEnumerable<AbilityDeliveryTarget> MoveAlongPath(AbilityExecutionContext context, ForcedPath path, MechanicEntity[] targets)
	{
		MechanicEntity caster = context.Caster;
		BaseUnitEntity casterUnit = caster as BaseUnitEntity;
		UnitMovementAgent movementAgent = caster.MaybeMovementAgent;
		GraphNode lastNode = path.path.Last();
		if (path.vectorPath.Count == 0)
		{
			yield break;
		}
		float distanceToHandle = Mathf.Sqrt(2f) * 1.Cells().Meters * 1.1f;
		HashSet<MechanicEntity> handledTargets = new HashSet<MechanicEntity>(targets.Length * 2);
		movementAgent.MaxSpeedOverride = 10f;
		casterUnit?.Features.IsCharging.Retain(context.Ability.Fact);
		bool failedToStartPath = false;
		try
		{
			movementAgent.ForcePath(path);
		}
		catch (Exception exception)
		{
			failedToStartPath = true;
			PFLog.Ability.ExceptionWithReport(exception, null);
		}
		EventBus.RaiseEvent((IMechanicEntity)caster, (Action<IDirectMovementHandler>)delegate(IDirectMovementHandler h)
		{
			h.HandleDirectMovementStarted(path, DisableAttacksOfOpportunity);
		}, isCheckRuntime: true);
		movementAgent.Blocker.Unblock();
		movementAgent.Blocker.BlockAt(lastNode.Vector3Position());
		TimeSpan startTime = Game.Instance.Controllers.TimeController.GameTime;
		while (!failedToStartPath && movementAgent.IsReallyMoving)
		{
			yield return null;
			IEnumerable<MechanicEntity> enumerable = HandleNecessaryTargets(context, targets, handledTargets, distanceToHandle);
			foreach (MechanicEntity item in enumerable)
			{
				yield return new AbilityDeliveryTarget(item);
			}
			if (Game.Instance.Controllers.TimeController.GameTime - startTime > 5f.Seconds())
			{
				PFLog.Default.ErrorWithReport("Direct movement takes too long time, force finished");
				break;
			}
		}
		context.Caster.Position = lastNode.Vector3Position();
		movementAgent.MaxSpeedOverride = null;
		casterUnit?.Features.IsCharging.Release(context.Ability.Fact);
		EventBus.RaiseEvent((IMechanicEntity)caster, (Action<IDirectMovementHandler>)delegate(IDirectMovementHandler h)
		{
			h.HandleDirectMovementEnded();
		}, isCheckRuntime: true);
		movementAgent.Blocker.Unblock();
		movementAgent.Blocker.BlockAtCurrentPosition();
		if (StepThroughTarget && casterUnit != null)
		{
			MechanicEntity mechanicEntity = targets.LastItem();
			if (mechanicEntity != null)
			{
				casterUnit.TurnTo(SizePathfindingHelper.FromMechanicsToViewPosition(mechanicEntity, mechanicEntity.Position));
			}
		}
		IEnumerable<MechanicEntity> enumerable2 = HandleNecessaryTargets(context, targets, handledTargets, distanceToHandle);
		foreach (MechanicEntity item2 in enumerable2)
		{
			yield return new AbilityDeliveryTarget(item2);
		}
		if (targets.Empty())
		{
			yield return new AbilityDeliveryTarget(context.Caster.Position);
		}
	}

	private IEnumerable<MechanicEntity> HandleNecessaryTargets(AbilityExecutionContext context, MechanicEntity[] targets, HashSet<MechanicEntity> handledTargets, float distanceToHandle)
	{
		if (targets.Empty())
		{
			return Enumerable.Empty<MechanicEntity>();
		}
		List<MechanicEntity> list = TempList.Get<MechanicEntity>();
		MechanicEntity caster = context.Caster;
		foreach (MechanicEntity mechanicEntity in targets)
		{
			if (handledTargets.Contains(mechanicEntity))
			{
				continue;
			}
			GridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(mechanicEntity.Position);
			GridNodeBase innerNodeNearestToTarget2 = mechanicEntity.GetInnerNodeNearestToTarget(innerNodeNearestToTarget.Vector3Position());
			if (GeometryUtils.MechanicsDistance(innerNodeNearestToTarget.Vector3Position(), innerNodeNearestToTarget2.Vector3Position()) <= distanceToHandle)
			{
				try
				{
					HandleTarget(context, mechanicEntity);
				}
				catch (Exception exception)
				{
					PFLog.Default.ExceptionWithReport(exception, null);
				}
				list.Add(mechanicEntity);
				handledTargets.Add(mechanicEntity);
			}
		}
		return list;
	}

	private void HandleTarget(AbilityExecutionContext context, [NotNull] MechanicEntity target)
	{
		using (EvalContext.PushContext(context, target))
		{
			ActionsOnEncounteredTarget.Run();
		}
	}

	[NotNull]
	[ItemNotNull]
	private MechanicEntity[] GetAllTargetUnits(OrientedPatternData pattern, MechanicEntity caster, int limit)
	{
		List<GridNodeBase> list = pattern.Nodes.ToTempList();
		List<MechanicEntity> list2 = TempList.Get<MechanicEntity>();
		foreach (GridNodeBase item in list)
		{
			BaseUnitEntity firstUnit = item.GetFirstUnit();
			if (firstUnit != null && firstUnit != caster && !firstUnit.IsDeadOrUnconscious && !list2.Contains(firstUnit) && (!IgnoreAllies || !caster.IsAlly(firstUnit)) && (!IgnoreEnemies || !caster.IsEnemy(firstUnit)))
			{
				if (IgnoreAllTargetsExceptLast)
				{
					list2.Clear();
				}
				else
				{
					limit--;
				}
				list2.Add(firstUnit);
				if (limit < 1)
				{
					break;
				}
			}
		}
		return list2.ToArray();
	}

	public IComparer<GridNodeBase> DistanceComparer(MechanicEntity caster)
	{
		return Comparer<GridNodeBase>.Create((GridNodeBase a, GridNodeBase b) => Comparer<float>.Default.Compare(caster.DistanceToInCells(a.Vector3Position()), caster.DistanceToInCells(b.Vector3Position())));
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public void OverrideHaloSize(int? haloSize)
	{
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		return GetOrientedPatternAndPath(ability, casterNode, targetNode, coveredTargetsOnly).Pattern;
	}

	public OrientedPatternData GetOrientedHaloPattern(IAbilityDataProviderForPattern ability, int haloSize, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		return default(OrientedPatternData);
	}

	public (OrientedPatternData Pattern, List<GridNodeBase> Path) GetOrientedPatternAndPath(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		return AbilityCustomMovementHelper.GetOrientedPatternAndPath(ability, casterNode, targetNode, MinRangeCells, coveredTargetsOnly, StepThroughTarget, StopOnFirstEncounter, IgnoreEnemies, IgnoreAllies, IsCharge, stopBeforeTargetNode: false);
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper targetWrapper, Vector3 casterPosition)
	{
		LocalizedString failReason;
		return CheckTargetRestriction(ability, targetWrapper, casterPosition, out failReason);
	}

	[CanBeNull]
	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper targetWrapper, Vector3 casterPosition)
	{
		CheckTargetRestriction(ability, targetWrapper, casterPosition, out var failReason);
		return failReason?.Text;
	}

	private bool CheckTargetRestriction(AbilityData ability, TargetWrapper target, Vector3 casterPosition, [CanBeNull] out LocalizedString failReason)
	{
		GridNodeBase nearestNode = target.NearestNode;
		GridNodeBase nearestNodeXZUnwalkable = casterPosition.GetNearestNodeXZUnwalkable();
		if (target.Entity != null && target.Entity.DistanceToInCells(casterPosition) < ability.MinRangeCells)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (!CalculatePathAndTargets(ability, target.NearestNode, nearestNodeXZUnwalkable, out var pathNodes, out var targets))
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.CanNotReachTarget;
			return false;
		}
		if (StopOnFirstEncounter && IsCharge && !targets.Empty() && !targets[^1].GetOccupiedNodes().Contains(nearestNode))
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.CanNotReachTarget;
			return false;
		}
		if (MustStandInTarget)
		{
			if (!pathNodes.Empty())
			{
				List<GridNodeBase> list = pathNodes;
				if (list[list.Count - 1] == nearestNode)
				{
					MechanicEntity caster = ability.Caster;
					List<GridNodeBase> list2 = pathNodes;
					if (caster.CanStandHere(list2[list2.Count - 1]))
					{
						goto IL_010e;
					}
				}
			}
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
			return false;
		}
		goto IL_010e;
		IL_010e:
		if (!StepThroughTarget && !AbilityCustomMovementHelper.IsPathToTargetReachDestination(ability.Caster, nearestNode, pathNodes))
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
			return false;
		}
		GridNodeBase gridNodeBase = pathNodes.LastItem();
		if (gridNodeBase == null || nearestNodeXZUnwalkable.CellDistanceTo(gridNodeBase) < MinRangeCells)
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (gridNodeBase != null && !ability.Caster.CanStandHere(gridNodeBase))
		{
			failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.CanNotReachTarget;
			return false;
		}
		if (IsCharge)
		{
			Vector3 to = ((ability.TargetAnchor == AbilityTargetAnchor.Point || target.Entity == null) ? target.NearestNode.Vector3Position() : target.Entity.GetInnerNodeNearestToTarget(target.Point).Vector3Position());
			if (WarhammerGeometryUtils.DistanceToInCells(nearestNodeXZUnwalkable.Vector3Position(), ability.Caster.SizeRect, to, default(IntRect)) > ability.RangeCells)
			{
				failReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
				return false;
			}
		}
		failReason = null;
		return true;
	}
}
