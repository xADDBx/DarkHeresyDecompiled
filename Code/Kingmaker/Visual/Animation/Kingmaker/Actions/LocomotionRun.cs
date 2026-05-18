using Code.Visual.Animation;
using Kingmaker.Visual.Animation.WeaponStyles;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class LocomotionRun : LocomotionState
{
	private bool m_IsInCombat;

	private WeaponAnimationStyle m_WeaponStyle;

	private WalkSpeedType m_WalkSpeedType;

	private Vector2 m_CurrentMoveDirection;

	private Vector2 m_DirectionSmoothDampVelocity = Vector2.zero;

	public LocomotionRun(UnitAnimationActionLocomotion animationAction, UnitAnimationActionHandle handle)
		: base(animationAction, handle)
	{
	}

	public override void OnEnterState()
	{
		base.m_RuntimeData.Speed = m_Handle.Unit.MovementAgent.Speed;
		m_CurrentMoveDirection = m_Handle.Unit.MovementAgent.FaceDirection.normalized;
		m_DirectionSmoothDampVelocity = Vector2.zero;
		UpdateLocomotionData();
		m_Handle.Manager.UpdateLocomotionParameters(m_Handle.Unit.MovementAgent.FaceDirection, m_CurrentMoveDirection, base.m_RuntimeData.Speed, base.m_MaxSpeed);
		m_Handle.Manager.PlayLocomotionMixer();
	}

	public override void OnExitState()
	{
		m_CurrentMoveDirection = m_Handle.Unit.MovementAgent.FaceDirection.normalized;
		m_Handle.SpeedScale = 1f;
	}

	public override LocomotionStateType Tick(float deltaTime)
	{
		if (m_Handle.Manager.IsClimbing || m_Handle.Manager.IsLeaping)
		{
			return LocomotionStateType.Traverse;
		}
		if (m_Handle.Unit != null && !m_Handle.Unit.AgentASP.IsReallyMoving)
		{
			return LocomotionStateType.Out;
		}
		if (m_Handle.Unit != null && m_Handle.Unit.AgentASP.IsInNodeLinkQueue && !m_Handle.Unit.MovementAgent.IsDirectionalMovementActive)
		{
			return LocomotionStateType.Traverse;
		}
		bool flag = m_WalkSpeedType != m_Handle.Manager.WalkSpeedType;
		UnitAnimationManager manager = m_Handle.Manager;
		AnimationBase animationBase = manager.CurrentEquipHandle?.ActiveAnimation;
		if (animationBase == null || animationBase.GetWeight() > 0.99f)
		{
			flag = flag || m_IsInCombat != manager.IsInCombat || (manager.IsInCombat && m_WeaponStyle != m_Handle.WeaponStyle);
		}
		if (flag && m_Handle.Manager.Animator.enabled)
		{
			m_Handle.ActiveAnimation?.StartTransitionOut();
			UpdateLocomotionData();
		}
		base.m_RuntimeData.Speed = m_Handle.Unit.MovementAgent.Speed;
		Vector2 normalized = m_Handle.Unit.MovementAgent.MoveDirection.normalized;
		m_CurrentMoveDirection = InterpolateMoveDirection(m_CurrentMoveDirection, normalized, ref m_DirectionSmoothDampVelocity, deltaTime);
		m_Handle.Manager.UpdateLocomotionParameters(m_Handle.Unit.MovementAgent.FaceDirection, m_CurrentMoveDirection, base.m_RuntimeData.Speed, base.m_MaxSpeed);
		m_Handle.Manager.PlayLocomotionMixer();
		return LocomotionStateType.Run;
	}

	private void UpdateLocomotionData()
	{
		m_WalkSpeedType = m_Handle.Manager.WalkSpeedType;
		m_IsInCombat = m_Handle.Manager.IsInCombat;
		m_WeaponStyle = m_Handle.WeaponStyle;
		UnitAnimationActionLocomotion.WalkingTypeData walkingTypeData = GetWalkingTypeData(base.m_ActualWeaponStyle);
		m_Handle.Manager.Speed = walkingTypeData.Parameters.Speed;
		LocomotionMixerAnimations locomotionMixerAnimations = m_AnimationAction.GetLocomotionMixerAnimations(m_WeaponStyle);
		m_Handle.Manager.UpdateLocomotionMixerAnimations(locomotionMixerAnimations);
	}

	private UnitAnimationActionLocomotion.WalkingTypeData GetWalkingTypeData(WeaponAnimationStyle style)
	{
		WeaponStyleLocomotionData locomotionData = m_AnimationAction.GetLocomotionData(style);
		return m_AnimationAction.GetWalkingTypeData(locomotionData, m_WalkSpeedType);
	}

	private static Vector2 InterpolateMoveDirection(Vector2 currentDirection, Vector2 targetDirection, ref Vector2 smoothDampVelocity, float deltaTime)
	{
		Vector2 vector = Vector2.SmoothDamp(currentDirection, targetDirection, ref smoothDampVelocity, 0.1f, 1000f, deltaTime);
		if ((vector - targetDirection).sqrMagnitude < 0.0001f)
		{
			vector = targetDirection;
		}
		return vector;
	}
}
