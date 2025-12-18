using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class LocomotionIdle : LocomotionState
{
	private bool m_IsInCombat;

	private WeaponAnimationStyle m_WeaponStyle;

	private readonly StatefulRandom m_StatefulRandom;

	private TimedProbabilityCurve.Tracker m_Tracker;

	private float m_VariantIdleAnimationEndTime;

	private bool m_IsPlayingIdleAnimation;

	public LocomotionIdle(UnitAnimationActionLocomotion animationAction, UnitAnimationActionHandle handle)
		: base(animationAction, handle)
	{
		m_StatefulRandom = m_Handle.Manager.StatefulRandom;
	}

	public override void OnEnterState()
	{
		UnitMovementAgentBase unitMovementAgentBase = m_Handle.Unit.Or(null)?.MovementAgent;
		if ((object)unitMovementAgentBase != null)
		{
			m_Handle.Manager.UpdateLocomotionParameters(unitMovementAgentBase.FaceDirection, unitMovementAgentBase.MoveDirection, 0f, base.m_MaxSpeed);
		}
		UpdateLocomotionData();
		StartIdleAnimation(shouldRandomizeStart: true);
	}

	public override void OnExitState()
	{
		m_Handle.Manager.IsMoodMaskCanBeApplied = false;
	}

	public override LocomotionStateType Tick(float deltaTime)
	{
		if (m_Handle.Unit != null && !m_Handle.Unit.AgentASP.IsInNodeLinkQueue && m_Handle.Unit.AgentASP.IsReallyMoving && !m_Handle.Manager.IsClimbing && !m_Handle.Manager.IsLeaping)
		{
			return LocomotionStateType.Run;
		}
		bool flag = false;
		UnitAnimationManager manager = m_Handle.Manager;
		AnimationBase animationBase = manager.CurrentEquipHandle?.ActiveAnimation;
		if (animationBase == null || animationBase.GetWeight() > 0.99f)
		{
			flag = flag || m_IsInCombat != manager.IsInCombat || (manager.IsInCombat && m_WeaponStyle != m_Handle.WeaponStyle);
		}
		if (flag && manager.Animator.enabled)
		{
			m_Handle.ActiveAnimation?.StartTransitionOut();
			m_Handle.ActiveAnimation?.StopEvents();
			UpdateLocomotionData();
			StartIdleAnimation(shouldRandomizeStart: true);
		}
		else if (ShouldStartVariantIdleAnimation(m_Handle.GetTime(), deltaTime))
		{
			StartVariantIdleAnimation();
		}
		else if (ShouldStartIdleAnimation(m_Handle.GetTime()))
		{
			StartIdleAnimation(shouldRandomizeStart: false);
		}
		return LocomotionStateType.Idle;
	}

	private void UpdateLocomotionData()
	{
		m_IsInCombat = m_Handle.Manager.IsInCombat;
		m_WeaponStyle = m_Handle.WeaponStyle;
	}

	private bool ShouldStartVariantIdleAnimation(float currentTime, float deltaTime)
	{
		if (currentTime < m_VariantIdleAnimationEndTime)
		{
			return false;
		}
		if (CutsceneLock.Active)
		{
			return false;
		}
		if (m_Tracker == null)
		{
			m_Tracker = m_AnimationAction.TriggerProbability.Track(m_StatefulRandom);
		}
		return m_Tracker.Tick(deltaTime);
	}

	private void StartVariantIdleAnimation()
	{
		AnimationClipWrapper variantIdleClip = m_AnimationAction.GetVariantIdleClip(m_Handle);
		if (!(variantIdleClip == null))
		{
			m_Tracker = null;
			m_IsPlayingIdleAnimation = false;
			m_VariantIdleAnimationEndTime = m_Handle.GetTime() + variantIdleClip.Length;
			m_Handle.Manager.IsMoodMaskCanBeApplied = false;
			m_Handle.StartClip(variantIdleClip, ClipDurationType.Oneshot);
		}
	}

	private bool ShouldStartIdleAnimation(float currentTime)
	{
		if (currentTime >= m_VariantIdleAnimationEndTime)
		{
			return !m_IsPlayingIdleAnimation;
		}
		return false;
	}

	private void StartIdleAnimation(bool shouldRandomizeStart)
	{
		m_IsPlayingIdleAnimation = true;
		m_Handle.Manager.NewSpeed = 0f;
		m_Handle.Manager.IsMoodMaskCanBeApplied = true;
		AnimationClipWrapper idleClip = m_AnimationAction.GetIdleClip(base.m_ActualWeaponStyle, m_AnimationAction.IsForDollRoom);
		if (idleClip == null)
		{
			PFLog.Animations.Error($"No idle animation for {base.m_ActualWeaponStyle}");
		}
		m_Handle.StartClip(idleClip, ClipDurationType.Endless);
		if (shouldRandomizeStart)
		{
			m_Handle.ActiveAnimation?.SetTime(PFStatefulRandom.Visuals.AnimationIdle.value * idleClip.Length);
		}
		m_Handle.ActiveAnimation?.SetSpeed(1f);
	}
}
