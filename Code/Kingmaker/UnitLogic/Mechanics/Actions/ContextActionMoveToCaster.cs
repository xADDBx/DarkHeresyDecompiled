using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("80bcc9b54f1a4ae2849b8f449d86e0ea")]
public class ContextActionMoveToCaster : ContextAction
{
	public enum TransitionType
	{
		Movement,
		Jump
	}

	[Tooltip("The target will stop at this distance in cells from the caster. If this value equal to or less than 1, the target will attempt to stop next to the caster.")]
	[SerializeField]
	private int m_StopAtDistanceInCells = 1;

	[SerializeField]
	private TransitionType m_TransitionType;

	[SerializeField]
	[ShowIf("IsJumpTransition")]
	private AnimationClipWrapper m_JumpAnimation;

	private bool IsJumpTransition => m_TransitionType == TransitionType.Jump;

	public override string GetCaption()
	{
		if (m_StopAtDistanceInCells <= 1)
		{
			return "Move to caster";
		}
		return $"Move to caster and stop at range of {m_StopAtDistanceInCells} cells";
	}

	protected override void RunAction()
	{
		if (base.Caster is BaseUnitEntity baseUnitEntity && base.Target?.Entity is BaseUnitEntity baseUnitEntity2 && !(baseUnitEntity2.View == null) && baseUnitEntity2.CanMove && base.AbilityContext != null && baseUnitEntity2.DistanceToInCells(baseUnitEntity) > m_StopAtDistanceInCells)
		{
			if (m_TransitionType == TransitionType.Jump)
			{
				MakeUnitJump(baseUnitEntity2, baseUnitEntity, m_StopAtDistanceInCells, base.AbilityContext, m_JumpAnimation);
			}
			else
			{
				MakeUnitMove(baseUnitEntity2, baseUnitEntity, m_StopAtDistanceInCells, base.AbilityContext);
			}
		}
	}

	private static void MakeUnitMove(BaseUnitEntity unit, BaseUnitEntity target, int stopAtDistanceInCells, AbilityExecutionContext abilityContext)
	{
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(unit.MovementAgent, target, limitRangeByActionPoints: false);
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, target))
		{
			while (warhammerPathPlayer.vectorPath.Count > 0)
			{
				GraphNode graphNode = warhammerPathPlayer.path[warhammerPathPlayer.vectorPath.Count - 1];
				Vector3 point = graphNode.Vector3Position();
				bool num = target.DistanceToInCells(point, default(IntRect)) < stopAtDistanceInCells;
				bool flag = WarhammerBlockManager.Instance.CanUnitStandOnNode(unit.SizeRect, graphNode, unit.MaybeMovementAgent.Or(null).Blocker);
				if (!num && flag)
				{
					break;
				}
				warhammerPathPlayer.vectorPath.RemoveAt(warhammerPathPlayer.vectorPath.Count - 1);
			}
			abilityContext.TemporarilyBlockLastPathNode(warhammerPathPlayer, unit);
			UnitMoveToProperParams cmdParams = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer), 0f);
			unit.Commands.Run(cmdParams);
		}
	}

	private static void MakeUnitJump(BaseUnitEntity unit, BaseUnitEntity target, int stopAtDistanceInCells, AbilityExecutionContext abilityContext, AnimationClipWrapper jumpAnimation)
	{
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(unit.MovementAgent, target, limitRangeByActionPoints: false);
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, target))
		{
			while (warhammerPathPlayer.vectorPath.Count > 0)
			{
				GraphNode graphNode = warhammerPathPlayer.path[warhammerPathPlayer.vectorPath.Count - 1];
				Vector3 point = graphNode.Vector3Position();
				bool num = target.DistanceToInCells(point, default(IntRect)) < stopAtDistanceInCells;
				bool flag = WarhammerBlockManager.Instance.CanUnitStandOnNode(unit.SizeRect, graphNode, unit.MaybeMovementAgent.Or(null).Blocker);
				if (!num && flag)
				{
					break;
				}
				warhammerPathPlayer.vectorPath.RemoveAt(warhammerPathPlayer.vectorPath.Count - 1);
			}
			abilityContext.TemporarilyBlockLastPathNode(warhammerPathPlayer, unit);
			Vector3 from = warhammerPathPlayer.vectorPath[0];
			List<Vector3> vectorPath = warhammerPathPlayer.vectorPath;
			UnitJumpToCellParams cmdParams = new UnitJumpToCellParams(from, vectorPath[vectorPath.Count - 1], jumpAnimation);
			unit.Commands.Run(cmdParams);
		}
	}
}
