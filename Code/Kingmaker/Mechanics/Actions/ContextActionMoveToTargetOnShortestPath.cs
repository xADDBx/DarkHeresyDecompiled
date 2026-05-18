using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("66f6fdc539a947a0a28892b83dd5729f")]
public class ContextActionMoveToTargetOnShortestPath : ContextAction
{
	public enum TransitionType
	{
		Movement,
		Jump
	}

	private class CellsCollector : Linecast.ICanTransitionBetweenCells
	{
		public readonly List<GridNodeBase> CollectedCells = new List<GridNodeBase>();

		public bool CanTransitionBetweenCells(GridNodeBase nodeFrom, GridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			CollectedCells.Add(nodeTo);
			return true;
		}
	}

	[SerializeField]
	[SerializeReference]
	private PositionEvaluator m_TargetPoint;

	[SerializeField]
	private bool m_ConsideringLenght;

	[SerializeField]
	[ShowIf("m_ConsideringLenght")]
	private ContextValue m_Cells;

	[SerializeField]
	private bool m_ProvokeAttackOfOpportunity;

	[SerializeField]
	private TransitionType m_TransitionType;

	[SerializeField]
	[ShowIf("IsJumpTransition")]
	private AnimationClipWrapperLink m_JumpAnimationLink;

	private bool IsJumpTransition => m_TransitionType == TransitionType.Jump;

	public override string GetCaption()
	{
		return $"Move unit to the {m_TargetPoint} on the shortest path";
	}

	protected override void RunAction()
	{
		UnitEntity unitEntity = base.Target?.Entity as UnitEntity;
		AbilityExecutionContext abilityContext = base.AbilityContext;
		if (unitEntity?.View != null && m_TargetPoint != null && unitEntity.CanMove && (abilityContext != null || IsJumpTransition))
		{
			if (m_TransitionType == TransitionType.Jump)
			{
				MakeUnitJump(unitEntity, m_TargetPoint.GetValue(), m_JumpAnimationLink);
			}
			else
			{
				MakeUnitMove(unitEntity, m_TargetPoint.GetValue());
			}
		}
	}

	private void MakeUnitMove(UnitEntity unit, Vector3 targetPoint)
	{
		Vector3 startingPosition = unit.Position;
		int num = m_Cells.Calculate(base.Context);
		if (!GridAreaHelper.TryGetStandableNode(unit, targetPoint.GetNearestNodeXZ(), num, out var targetNode))
		{
			return;
		}
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(unit.View.MovementAgent, targetNode.Vector3Position(), limitRangeByActionPoints: false);
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, this))
		{
			if (m_ConsideringLenght && warhammerPathPlayer.vectorPath.Count > num + 1)
			{
				warhammerPathPlayer.vectorPath.RemoveRange(num + 1, warhammerPathPlayer.vectorPath.Count - num - 1);
			}
			base.AbilityContext?.TemporarilyBlockLastPathNode(warhammerPathPlayer, unit);
			UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer), 0f);
			if (m_ProvokeAttackOfOpportunity)
			{
				unitMoveToProperParams.DisableAttackOfOpportunity.Release();
			}
			unit?.Commands.Run(unitMoveToProperParams);
			EventBus.RaiseEvent(delegate(IUnitAbilityNonPushForceMoveHandler h)
			{
				h.HandleUnitNonPushForceMove(startingPosition.GetNearestNodeXZ().CellDistanceTo(targetPoint.GetNearestNodeXZ()), base.Context, unit);
			});
		}
	}

	private void MakeUnitJump(UnitEntity unit, Vector3 targetPoint, AnimationClipWrapperLink jumpAnimation)
	{
		Vector3 startingPosition = unit.Position;
		GridNodeBase nearestNodeXZ = startingPosition.GetNearestNodeXZ();
		CellsCollector condition = new CellsCollector();
		Linecast.LinecastGrid(nearestNodeXZ.Graph, startingPosition, targetPoint, nearestNodeXZ, out var _, null, ref condition, 0.0001f);
		GridNodeBase gridNodeBase = nearestNodeXZ;
		int num = m_Cells.Calculate(base.Context);
		for (int num2 = condition.CollectedCells.Count - 1; num2 > 0; num2--)
		{
			GridNodeBase gridNodeBase2 = condition.CollectedCells[num2];
			if (gridNodeBase2.Walkable && WarhammerBlockManager.Instance.CanUnitStandOnNode(unit, gridNodeBase2))
			{
				gridNodeBase = gridNodeBase2;
				break;
			}
		}
		if (gridNodeBase != nearestNodeXZ && gridNodeBase.CellDistanceTo(targetPoint.GetNearestNodeXZ()) <= num)
		{
			UnitJumpToCellParams cmdParams = new UnitJumpToCellParams(nearestNodeXZ.Vector3Position(), gridNodeBase.Vector3Position(), jumpAnimation)
			{
				ProvokeAttackOfOpportunity = m_ProvokeAttackOfOpportunity
			};
			unit.Commands.Run(cmdParams);
			EventBus.RaiseEvent(delegate(IUnitAbilityNonPushForceMoveHandler h)
			{
				h.HandleUnitNonPushForceMove(startingPosition.GetNearestNodeXZ().CellDistanceTo(targetPoint.GetNearestNodeXZ()), base.Context, unit);
			});
		}
	}
}
