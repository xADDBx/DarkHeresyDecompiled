using Code.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.WeaponStyles;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class LocomotionOut : LocomotionState
{
	private WalkSpeedType m_WalkSpeedType;

	private float m_StartTime;

	private float m_EndTime;

	private float m_ReduceSpeedDuration;

	private AnimationClipWrapper m_Clip;

	private float m_StartSpeed;

	private Vector3 m_FinalDestination;

	private AnimationActionHandle m_LocomotionOutHandle;

	public LocomotionOut(UnitAnimationActionLocomotion animationAction, UnitAnimationActionHandle handle)
		: base(animationAction, handle)
	{
	}

	public override void OnEnterState()
	{
		UpdateLocomotionData();
		UnitAnimationActionLocomotion.WalkingTypeData walkingTypeData = GetWalkingTypeData();
		m_Clip = (IsUsingCustomStoppingAnimation(walkingTypeData) ? walkingTypeData.Out : null);
		m_ReduceSpeedDuration = walkingTypeData.Parameters.OutTransition;
		m_StartTime = m_Handle.GetTime();
		m_EndTime = ((m_Clip != null) ? (m_StartTime + m_Clip.Length) : (m_StartTime + m_ReduceSpeedDuration));
		m_StartSpeed = base.m_RuntimeData.Speed;
		if (m_Clip != null)
		{
			UnitAnimationActionClip unitAnimationActionClip = UnitAnimationActionClip.Create(m_Clip, "OnEnterState");
			unitAnimationActionClip.TransitionIn = 0.1f;
			unitAnimationActionClip.TransitionOut = 0.1f;
			m_Handle.Manager.TryExecute(unitAnimationActionClip, delegate(AnimationActionHandle h)
			{
				h.AnimationLayer = UnitAnimationLayerType.LocomotionOut;
			}, out m_LocomotionOutHandle);
		}
	}

	private bool IsUsingCustomStoppingAnimation(UnitAnimationActionLocomotion.WalkingTypeData walkingTypeData)
	{
		if (!m_Handle.Manager.View.AgentASP.DecelerateBeforeStop && walkingTypeData.Out != null)
		{
			return walkingTypeData.Out.AnimationClip != null;
		}
		return false;
	}

	public override void OnExitState()
	{
		m_LocomotionOutHandle?.Release();
		m_LocomotionOutHandle = null;
	}

	public override LocomotionStateType Tick(float deltaTime)
	{
		float time = m_Handle.GetTime();
		UpdateMovementSpeed(time);
		if (m_Handle.Manager.IsClimbing || m_Handle.Manager.IsLeaping)
		{
			return LocomotionStateType.Traverse;
		}
		if (m_Handle.Unit != null && m_Handle.Unit.AgentASP.IsInNodeLinkQueue && !m_Handle.Unit.MovementAgent.IsDirectionalMovementActive)
		{
			return LocomotionStateType.Traverse;
		}
		if (time > m_EndTime)
		{
			if (m_Handle.Unit != null && m_Handle.Unit.AgentASP.IsReallyMoving)
			{
				return LocomotionStateType.Run;
			}
			return LocomotionStateType.Idle;
		}
		if (m_Handle.Unit != null && !m_Handle.Unit.MovementAgent.IsDirectionalMovementActive && m_Handle.Unit.AgentASP.IsReallyMoving && (m_Handle.Unit.AgentASP.PathCursor.LastWaypoint3D - m_FinalDestination).sqrMagnitude > 0.1f)
		{
			return LocomotionStateType.Run;
		}
		if (m_WalkSpeedType != m_Handle.Manager.WalkSpeedType && m_Handle.Manager.Animator.enabled)
		{
			m_Handle.ActiveAnimation?.StartTransitionOut();
			UpdateLocomotionData();
			TryStartClip();
		}
		return LocomotionStateType.Out;
	}

	private void UpdateLocomotionData()
	{
		m_WalkSpeedType = m_Handle.Manager.WalkSpeedType;
		m_FinalDestination = ((m_Handle.Unit.AgentASP.PathCursor.Count > 0) ? m_Handle.Unit.AgentASP.PathCursor.LastWaypoint3D : m_Handle.Unit.Data.Position);
	}

	private bool TryStartClip()
	{
		if (GetWalkingTypeData().Out == null)
		{
			PFLog.Animations.Error(m_Clip, "Locomotion animation clip is not set, object {0}", m_Clip.name);
			return false;
		}
		m_Handle.StartClip(m_Clip, ClipDurationType.Oneshot);
		return true;
	}

	private void UpdateMovementSpeed(float time)
	{
		float t = ((time < m_EndTime) ? Mathf.Clamp(1f - Mathf.Sqrt((m_EndTime - time) / m_ReduceSpeedDuration), 0f, 1f) : 1f);
		base.m_RuntimeData.Speed = Mathf.Lerp(m_StartSpeed, 0f, t);
		m_Handle.Manager.UpdateLocomotionParameters(m_Handle.Unit.MovementAgent.FaceDirection, m_Handle.Unit.MovementAgent.MoveDirection, base.m_RuntimeData.Speed, base.m_MaxSpeed);
	}

	private UnitAnimationActionLocomotion.WalkingTypeData GetWalkingTypeData()
	{
		WeaponStyleLocomotionData locomotionData = m_AnimationAction.WeaponStyleSettings[base.m_ActualWeaponStyle]?.Locomotion ?? m_AnimationAction.NonCombatLocomotionData;
		return m_AnimationAction.GetWalkingTypeData(locomotionData, m_WalkSpeedType);
	}
}
