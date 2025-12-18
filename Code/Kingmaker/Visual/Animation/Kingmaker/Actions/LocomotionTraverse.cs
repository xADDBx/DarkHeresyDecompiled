using Kingmaker.Pathfinding;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class LocomotionTraverse : LocomotionState
{
	private float m_CurrentSpeed;

	public LocomotionTraverse(UnitAnimationActionLocomotion animationAction, UnitAnimationActionHandle handle)
		: base(animationAction, handle)
	{
	}

	public override void OnEnterState()
	{
		m_CurrentSpeed = m_Handle.Unit.MovementAgent.NodeLinkTraverser.SpeedBeforeTraverse;
		if (m_Handle.Unit.MovementAgent.NodeLinkTraverser.HasPathAfterTraverse)
		{
			m_Handle.Unit.MovementAgent.NodeLinkTraverser.CurrentSpeed = GetTargetSpeedAfterTraverse();
		}
	}

	public override LocomotionStateType Tick(float deltaTime)
	{
		float targetSpeedAfterTraverse = GetTargetSpeedAfterTraverse();
		float f = deltaTime - m_CurrentSpeed;
		float num = deltaTime * m_Handle.Unit.MovementAgent.Acceleration;
		m_CurrentSpeed = ((Mathf.Abs(f) < num) ? targetSpeedAfterTraverse : (m_CurrentSpeed + Mathf.Sign(f) * num));
		m_Handle.Manager.UpdateLocomotionParameters(m_Handle.Unit.MovementAgent.FaceDirection, m_Handle.Unit.MovementAgent.MoveDirection, m_CurrentSpeed, base.m_MaxSpeed);
		if (m_Handle.Manager.IsClimbing || m_Handle.Manager.IsLeaping || m_Handle.Unit.AgentASP.IsInNodeLinkQueue)
		{
			return LocomotionStateType.Traverse;
		}
		if (!m_Handle.Unit.MovementAgent.NodeLinkTraverser.HasPathAfterTraverse)
		{
			return LocomotionStateType.Idle;
		}
		return LocomotionStateType.Run;
	}

	private float GetTargetSpeedAfterTraverse()
	{
		TraverseData traverseData = m_Handle.Unit.MovementAgent.NodeLinkTraverser.TraverseData;
		if (traverseData == null)
		{
			return 0f;
		}
		if (!traverseData.IsLeapTraverse)
		{
			if (!traverseData.IsUpTraverse)
			{
				return m_AnimationAction.SpeedAfterClimbedDown;
			}
			return m_AnimationAction.SpeedAfterClimbedUp;
		}
		return m_AnimationAction.SpeedAfterLeaped;
	}
}
