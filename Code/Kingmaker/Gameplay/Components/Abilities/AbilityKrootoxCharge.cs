using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Gameplay.Components.Abilities;

[Serializable]
[ComponentName("Custom/Krootox/AbilityKrootoxCharge")]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("7042d1eb5570490a87e0510c70243a86")]
public class AbilityKrootoxCharge : AbilityCustomLogic, IAbilityAoEPatternProvider
{
	public bool DisableAttacksOfOpportunity;

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

	public override bool IsMoveUnit => true;

	public override bool IsEngageUnit => true;

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
			foreach (AbilityDeliveryTarget item in MoveAlongPath(context, ForcedPath.Construct(pathNodes), targets, nearestNode))
			{
				yield return item;
			}
			if (caster is BaseUnitEntity baseUnitEntity)
			{
				baseUnitEntity.CombatState.RegisterMoveCells(pathCellsCount - 1);
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
		targets = GetAllTargets(item, caster, targetNode, 1);
		return pathNodes.HasItem((GridNodeBase i) => i != casterNode);
	}

	private IEnumerable<AbilityDeliveryTarget> MoveAlongPath(AbilityExecutionContext context, ForcedPath path, MechanicEntity[] targets, GridNodeBase targetNode)
	{
		MechanicEntity caster = context.Caster;
		BaseUnitEntity casterUnit = caster as BaseUnitEntity;
		UnitMovementAgentBase movementAgent = caster.MaybeMovementAgent;
		GraphNode lastNode = path.path.Last();
		if (path.vectorPath.Count == 0)
		{
			yield break;
		}
		float distanceToHandle = Mathf.Sqrt(2f) * 1.Cells().Meters * 1.1f;
		HashSet<AbilityDeliveryTarget> handledTargets = new HashSet<AbilityDeliveryTarget>(targets.Length * 2);
		movementAgent.MaxSpeedOverride = 10f;
		movementAgent.IsCharging = true;
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
			IEnumerable<AbilityDeliveryTarget> enumerable = HandleNecessaryTargets(context, targets, handledTargets, distanceToHandle, targetNode);
			foreach (AbilityDeliveryTarget item in enumerable)
			{
				yield return item;
			}
			if (Game.Instance.Controllers.TimeController.GameTime - startTime > 5f.Seconds())
			{
				PFLog.Default.ErrorWithReport("Direct movement takes too long time, force finished");
				break;
			}
		}
		context.Caster.Position = lastNode.Vector3Position();
		movementAgent.IsCharging = false;
		movementAgent.MaxSpeedOverride = null;
		casterUnit?.Features.IsCharging.Release(context.Ability.Fact);
		EventBus.RaiseEvent((IMechanicEntity)caster, (Action<IDirectMovementHandler>)delegate(IDirectMovementHandler h)
		{
			h.HandleDirectMovementEnded();
		}, isCheckRuntime: true);
		movementAgent.Blocker.Unblock();
		movementAgent.Blocker.BlockAtCurrentPosition();
		if (targets.Empty())
		{
			yield return new AbilityDeliveryTarget(context.Caster.Position);
		}
	}

	private IEnumerable<AbilityDeliveryTarget> HandleNecessaryTargets(AbilityExecutionContext context, MechanicEntity[] targets, HashSet<AbilityDeliveryTarget> handledTargets, float distanceToHandle, GraphNode lastPathNode)
	{
		List<AbilityDeliveryTarget> list = TempList.Get<AbilityDeliveryTarget>();
		if (targets.Empty())
		{
			if (handledTargets.Any((AbilityDeliveryTarget t) => t.Target.NearestNode == lastPathNode))
			{
				return Enumerable.Empty<AbilityDeliveryTarget>();
			}
			if (GeometryUtils.MechanicsDistance(context.Caster.GetInnerNodeNearestToTarget(lastPathNode.Vector3Position()).Vector3Position(), lastPathNode.Vector3Position()) <= distanceToHandle)
			{
				AbilityDeliveryTarget item = new AbilityDeliveryTarget(new TargetWrapper(lastPathNode.Vector3Position()));
				list.Add(item);
				handledTargets.Add(item);
			}
		}
		else
		{
			MechanicEntity caster = context.Caster;
			foreach (MechanicEntity target in targets)
			{
				if (!handledTargets.Any((AbilityDeliveryTarget t) => t.Target.Entity == target))
				{
					GridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(target.Position);
					GridNodeBase innerNodeNearestToTarget2 = target.GetInnerNodeNearestToTarget(innerNodeNearestToTarget.Vector3Position());
					if (GeometryUtils.MechanicsDistance(innerNodeNearestToTarget.Vector3Position(), innerNodeNearestToTarget2.Vector3Position()) <= distanceToHandle)
					{
						AbilityDeliveryTarget item2 = new AbilityDeliveryTarget(target);
						list.Add(item2);
						handledTargets.Add(item2);
					}
				}
			}
		}
		return list;
	}

	[NotNull]
	[ItemNotNull]
	private MechanicEntity[] GetAllTargets(OrientedPatternData pattern, MechanicEntity caster, GridNodeBase targetNode, int limit)
	{
		List<GridNodeBase> list = pattern.Nodes.ToTempList();
		List<MechanicEntity> list2 = TempList.Get<MechanicEntity>();
		foreach (GridNodeBase item3 in list)
		{
			BaseUnitEntity firstUnit = item3.GetFirstUnit();
			if (firstUnit != null && firstUnit != caster && !firstUnit.IsDeadOrUnconscious && !list2.Contains(firstUnit))
			{
				list2.Add(firstUnit);
				if (limit-- < 1 || limit < 1)
				{
					break;
				}
			}
		}
		if (!list.Contains(targetNode) && list.Count > 1)
		{
			GridNodeBase gridNodeBase = list[0];
			if (gridNodeBase != null)
			{
				GridNodeBase gridNodeBase2 = list[list.Count - 1];
				if (gridNodeBase2 != null)
				{
					GridNodeBase neighbourAlongDirection = gridNodeBase2.GetNeighbourAlongDirection(gridNodeBase2.Vector3Position() - gridNodeBase.Vector3Position());
					NodeList occupiedNodes = caster.GetOccupiedNodes(neighbourAlongDirection);
					foreach (var item4 in caster.GetOccupiedNodes(gridNodeBase2).Zip(occupiedNodes, (GridNodeBase n1, GridNodeBase n2) => (n1: n1, n2: n2)))
					{
						GridNodeBase item = item4.n1;
						GridNodeBase item2 = item4.n2;
						DestructibleEntity destructibleEntity = ThinCoverEntity.FindThinCover(item, item2);
						if (destructibleEntity != null && !list2.Contains(destructibleEntity))
						{
							list2.Add(destructibleEntity);
							if (limit-- < 1)
							{
								break;
							}
						}
					}
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
		return AbilityCustomMovementHelper.GetOrientedPatternAndPath(ability, casterNode, targetNode, ((BlueprintAbility)base.OwnerBlueprint).MinRange, coveredTargetsOnly, stepThroughTarget: false, stopOnFirstEncounter: true, ignoreEnemies: false, ignoreAllies: false, isCharge: true, stopBeforeTargetNode: true);
	}
}
