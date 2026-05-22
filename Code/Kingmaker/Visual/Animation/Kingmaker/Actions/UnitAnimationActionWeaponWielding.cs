using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.WeaponStyles;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationActionWeaponWielding : UnitAnimationAction
{
	private enum MovementState
	{
		Idle,
		Walk,
		Run,
		Sprint,
		Traverse
	}

	private class ActionData
	{
		public MovementState LastMovementState;

		public bool WasInCombat;
	}

	[SerializeField]
	private AvatarMask m_WeaponWieldingMask;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings?.Get();

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => CollectClipWrappers();

	public override UnitAnimationType Type => UnitAnimationType.WeaponWielding;

	public override bool DontReleaseOnInterrupt => true;

	private IEnumerable<AnimationClipWrapper> CollectClipWrappers()
	{
		if (m_ClipWrappersHashSet != null)
		{
			return m_ClipWrappersHashSet;
		}
		m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
		if (WeaponStyleSettings != null)
		{
			m_ClipWrappersHashSet.AddRange(from c in WeaponStyleSettings.EnumerateLocomotionClips()
				where c != null
				select c);
		}
		return m_ClipWrappersHashSet;
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.AnimationLayer = UnitAnimationLayerType.WeaponWielding;
		handle.ActionData = new ActionData
		{
			LastMovementState = MovementState.Idle
		};
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (handle.Unit == null || handle.Unit.Data == null)
		{
			return;
		}
		ActionData actionData = (ActionData)handle.ActionData;
		bool isInCombat = handle.Unit.Data.IsInCombat;
		if (!actionData.WasInCombat && !isInCombat)
		{
			return;
		}
		if (actionData.WasInCombat && !isInCombat)
		{
			actionData.WasInCombat = false;
			AnimationClipWrapper animationClip = GetAnimationClip(handle, MovementState.Idle);
			if (animationClip != null)
			{
				handle.StartClip(animationClip, m_WeaponWieldingMask, ClipDurationType.Oneshot);
				return;
			}
		}
		actionData.WasInCombat = isInCombat;
		MovementState movementState = GetMovementState(handle);
		if (actionData.LastMovementState == movementState)
		{
			return;
		}
		actionData.LastMovementState = movementState;
		AnimationClipWrapper animationClip2 = GetAnimationClip(handle, movementState);
		if (!(animationClip2 == null))
		{
			switch (movementState)
			{
			case MovementState.Idle:
			{
				float time = Mathf.Max(animationClip2.Length - TransitionIn - TransitionOut, 0f);
				handle.StartClip(animationClip2, m_WeaponWieldingMask, ClipDurationType.Oneshot);
				handle.ActiveAnimation.SetTime(time);
				break;
			}
			case MovementState.Traverse:
				handle.ActiveAnimation.StartTransitionOut();
				break;
			default:
				handle.StartClip(animationClip2, m_WeaponWieldingMask, ClipDurationType.Endless);
				break;
			}
		}
	}

	private AnimationClipWrapper GetAnimationClip(UnitAnimationActionHandle handle, MovementState currentMovementState)
	{
		WeaponStyleLocomotionData weaponStyleLocomotionData = WeaponStyleSettings[handle.WeaponStyle]?.Locomotion;
		if (weaponStyleLocomotionData == null)
		{
			return null;
		}
		return currentMovementState switch
		{
			MovementState.Idle => weaponStyleLocomotionData.Idle, 
			MovementState.Walk => weaponStyleLocomotionData.Walk, 
			MovementState.Run => weaponStyleLocomotionData.Run, 
			MovementState.Sprint => weaponStyleLocomotionData.Sprint, 
			MovementState.Traverse => weaponStyleLocomotionData.Idle, 
			_ => null, 
		};
	}

	private static MovementState GetMovementState(UnitAnimationActionHandle handle)
	{
		if (handle.Unit == null || handle.Unit.MovementAgent == null)
		{
			return MovementState.Idle;
		}
		if (!handle.Unit.MovementAgent.IsReallyMoving || handle.Manager == null)
		{
			return MovementState.Idle;
		}
		if (handle.Manager.IsLeaping || handle.Manager.IsClimbing)
		{
			return MovementState.Traverse;
		}
		return handle.Manager.WalkSpeedType switch
		{
			WalkSpeedType.Walk => MovementState.Walk, 
			WalkSpeedType.Run => MovementState.Run, 
			WalkSpeedType.Sprint => MovementState.Sprint, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
