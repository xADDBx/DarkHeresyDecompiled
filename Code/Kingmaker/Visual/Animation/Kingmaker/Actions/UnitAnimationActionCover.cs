using System;
using System.Collections.Generic;
using System.Linq;
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
		Exit
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
		m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
		if (WeaponStyleSettings != null)
		{
			m_ClipWrappersHashSet.AddRange(from c in WeaponStyleSettings.EnumerateCoverClips()
				where c != null
				select c);
		}
		return m_ClipWrappersHashSet;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.HasCrossfadePriority = true;
		handle.AnimationLayer = UnitAnimationLayerType.Cover;
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
		case CoverAnimationState.Idle:
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				handle.Manager.AbilityIsSpell = false;
				ChangeStateAndStartAction(handle, CoverAnimationState.Idle);
			}
			break;
		case CoverAnimationState.Exit:
			data.WaitForceExit = true;
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				data.ActionStarted = false;
				data.WaitForceExit = false;
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
			bool flag = data.CurrentAnimationState == CoverAnimationState.Idle;
			handle.StartClip(animationClip, (!flag) ? ClipDurationType.Oneshot : ClipDurationType.Endless);
			if (flag && handle.ActiveAnimation != null)
			{
				handle.ActiveAnimation.TransitionIn = 0f;
				handle.ActiveAnimation.TransitionOut = 0f;
			}
			handle.SpeedScale = (flag ? 1f : Game.CombatAnimSpeedUp);
			data.ActionStarted = true;
			data.Time = handle.GetTime() + animationClip.Length;
		}
	}

	private AnimationClipWrapper GetAnimationClip(UnitAnimationActionHandle handle, CoverAnimationState animState)
	{
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
			_ => null, 
		};
	}
}
