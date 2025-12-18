using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.WeaponStyles;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationCover", menuName = "Animation Manager/Actions/Unit Cover")]
public class UnitAnimationActionCover : UnitAnimationAction
{
	private enum CoverAnimationState
	{
		NotInCover,
		Enter,
		Idle,
		Exit,
		SideStepIn,
		SideStepOut
	}

	public enum StepOutDirectionAnimationType
	{
		None,
		Left,
		Right
	}

	private class Data
	{
		public CoverAnimationState CurrentAnimationState;

		public bool ActionStarted;

		public bool WaitForceExit;

		public float Time;
	}

	[Header("Default Cover Settings")]
	public AnimationClipWrapper FullCoverInside;

	public AnimationClipWrapper FullCoverOutside;

	public AnimationClipWrapper LeftStepFullCoverEnteringForCast;

	public AnimationClipWrapper LeftStepFullCoverExitingForCast;

	public AnimationClipWrapper RightStepFullCoverEnteringForCast;

	public AnimationClipWrapper RightStepFullCoverExitingForCast;

	[SerializeField]
	[ValidateNotNull]
	[Header("Weapon Style Specific Cover Settings")]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public override bool DontReleaseOnInterrupt => true;

	private BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings.Get();

	[NotNull]
	private WeaponStyleCoverData NonCombatSettings => WeaponStyleSettings[WeaponAnimationStyle.NonCombat]?.Cover ?? throw new NullReferenceException();

	public override UnitAnimationType Type => UnitAnimationType.Cover;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => CollectClipWrappers();

	private IEnumerable<AnimationClipWrapper> CollectClipWrappers()
	{
		if (m_ClipWrappersHashSet != null)
		{
			return m_ClipWrappersHashSet;
		}
		m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper> { LeftStepFullCoverEnteringForCast, LeftStepFullCoverExitingForCast, RightStepFullCoverEnteringForCast, RightStepFullCoverExitingForCast };
		if (WeaponStyleSettings != null)
		{
			m_ClipWrappersHashSet.AddRange(WeaponStyleSettings.EnumerateCoverClips());
		}
		return m_ClipWrappersHashSet;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.HasCrossfadePriority = true;
		handle.AnimationLayer = AnimationLayerType.Cover;
		handle.ActionData = new Data
		{
			CurrentAnimationState = CoverAnimationState.NotInCover
		};
		if (WeaponStyleSettings == null)
		{
			PFLog.Animations.Error(this, $"{this}: WeaponStyleSettings not set!");
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		Data data = (Data)handle.ActionData;
		if (data == null)
		{
			return;
		}
		switch (data.CurrentAnimationState)
		{
		case CoverAnimationState.Enter:
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				data.ActionStarted = false;
				ChangeStateAndStartAction(handle, CoverAnimationState.Idle);
			}
			break;
		case CoverAnimationState.SideStepOut:
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				data.ActionStarted = false;
				handle.Manager.StepOutDirectionAnimationType = StepOutDirectionAnimationType.None;
				handle.Manager.AbilityIsSpell = false;
				ChangeStateAndStartAction(handle, CoverAnimationState.Idle);
			}
			break;
		case CoverAnimationState.Idle:
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				handle.Manager.StepOutDirectionAnimationType = StepOutDirectionAnimationType.None;
				handle.Manager.AbilityIsSpell = false;
				ChangeStateAndStartAction(handle, CoverAnimationState.Idle);
			}
			break;
		case CoverAnimationState.SideStepIn:
			handle.Manager.BlockAttackAnimation = true;
			if (handle.Manager.SequencedActions.Count > 0)
			{
				data.WaitForceExit = true;
				data.ActionStarted = false;
				handle.Manager.BlockAttackAnimation = false;
				break;
			}
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				handle.Manager.BlockAttackAnimation = false;
				if (!handle.Manager.NeedStepOut)
				{
					ChangeStateAndStartAction(handle, CoverAnimationState.SideStepOut);
				}
			}
			break;
		case CoverAnimationState.Exit:
			handle.Manager.BlockAttackAnimation = true;
			data.WaitForceExit = true;
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				data.ActionStarted = false;
				data.WaitForceExit = false;
				handle.Manager.BlockAttackAnimation = false;
				ChangeStateAndStartAction(handle, CoverAnimationState.NotInCover);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case CoverAnimationState.NotInCover:
			break;
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public void EnterCover(UnitAnimationActionHandle handle)
	{
		if (((Data)handle.ActionData).CurrentAnimationState == CoverAnimationState.NotInCover)
		{
			SetActionData(handle, CoverAnimationState.Enter);
		}
	}

	public void ExitCover(UnitAnimationActionHandle handle)
	{
		if (((Data)handle.ActionData).CurrentAnimationState == CoverAnimationState.Idle)
		{
			SetActionData(handle, CoverAnimationState.Exit);
		}
	}

	public void DoSideStep(UnitAnimationActionHandle handle)
	{
		CoverAnimationState currentAnimationState = ((Data)handle.ActionData).CurrentAnimationState;
		if (currentAnimationState == CoverAnimationState.Idle || currentAnimationState == CoverAnimationState.NotInCover)
		{
			SetActionData(handle, CoverAnimationState.SideStepIn);
		}
	}

	private void SetActionData(UnitAnimationActionHandle handle, CoverAnimationState newState)
	{
		handle.ActionData = new Data
		{
			CurrentAnimationState = newState
		};
	}

	public bool IsCoverForceExitingFinished(UnitAnimationActionHandle handle)
	{
		return ((Data)handle.ActionData)?.WaitForceExit ?? false;
	}

	private void ChangeStateAndStartAction(UnitAnimationActionHandle handle, CoverAnimationState state)
	{
		Data data = (Data)handle.ActionData;
		if (data != null && data.CurrentAnimationState != state)
		{
			data.CurrentAnimationState = state;
			StartAction(handle);
		}
	}

	private void StartAction(UnitAnimationActionHandle handle)
	{
		Data data = (Data)handle.ActionData;
		if (data == null)
		{
			return;
		}
		AnimationClipWrapper animationClip = GetAnimationClip(handle, data.CurrentAnimationState);
		if (!(animationClip == null))
		{
			CoverAnimationState currentAnimationState = data.CurrentAnimationState;
			handle.StartClip(animationClip, (currentAnimationState != CoverAnimationState.Idle && currentAnimationState != CoverAnimationState.SideStepIn) ? ClipDurationType.Oneshot : ClipDurationType.Endless);
			if (data.CurrentAnimationState == CoverAnimationState.Idle && handle.ActiveAnimation != null)
			{
				handle.ActiveAnimation.TransitionIn = 0f;
				handle.ActiveAnimation.TransitionOut = 0f;
			}
			if (Game.CombatAnimSpeedUp > 1f && data.CurrentAnimationState != CoverAnimationState.Idle)
			{
				handle.SpeedScale = Game.CombatAnimSpeedUp;
			}
			data.ActionStarted = true;
			data.Time = handle.GetTime() + animationClip.Length;
		}
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		handle.Manager.BlockAttackAnimation = false;
		base.OnFinish(handle);
	}

	private AnimationClipWrapper GetAnimationClip(UnitAnimationActionHandle handle, CoverAnimationState animState)
	{
		if (handle.Manager.AbilityIsSpell && TryGetCastSpellAnimationClip(animState, handle.Manager.StepOutDirectionAnimationType, out var clip))
		{
			return clip;
		}
		if (WeaponStyleSettings == null)
		{
			return null;
		}
		WeaponStyleCoverData weaponStyleCoverData = WeaponStyleSettings[handle.WeaponStyle]?.Cover;
		return animState switch
		{
			CoverAnimationState.Enter => ((weaponStyleCoverData != null) ? weaponStyleCoverData.CoverIn.Or(null) : null) ?? NonCombatSettings.CoverIn, 
			CoverAnimationState.Idle => ((weaponStyleCoverData != null) ? weaponStyleCoverData.Cover.Or(null) : null) ?? NonCombatSettings.Cover, 
			CoverAnimationState.Exit => ((weaponStyleCoverData != null) ? weaponStyleCoverData.CoverOut.Or(null) : null) ?? NonCombatSettings.CoverOut, 
			CoverAnimationState.SideStepOut => (handle.Manager.StepOutDirectionAnimationType != 0) ? null : (((weaponStyleCoverData != null) ? weaponStyleCoverData.CoverIn.Or(null) : null) ?? NonCombatSettings.CoverIn), 
			CoverAnimationState.SideStepIn => (handle.Manager.StepOutDirectionAnimationType != 0) ? null : (((weaponStyleCoverData != null) ? weaponStyleCoverData.CoverOut.Or(null) : null) ?? NonCombatSettings.CoverOut), 
			_ => null, 
		};
	}

	private bool TryGetCastSpellAnimationClip(CoverAnimationState animState, StepOutDirectionAnimationType stepOutDirection, out AnimationClipWrapper clip)
	{
		clip = null;
		switch (animState)
		{
		case CoverAnimationState.SideStepOut:
			switch (stepOutDirection)
			{
			case StepOutDirectionAnimationType.Left:
				clip = LeftStepFullCoverEnteringForCast;
				break;
			case StepOutDirectionAnimationType.Right:
				clip = RightStepFullCoverEnteringForCast;
				break;
			}
			break;
		case CoverAnimationState.SideStepIn:
			switch (stepOutDirection)
			{
			case StepOutDirectionAnimationType.Left:
				clip = LeftStepFullCoverExitingForCast;
				break;
			case StepOutDirectionAnimationType.Right:
				clip = RightStepFullCoverExitingForCast;
				break;
			}
			break;
		}
		return clip != null;
	}
}
