using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.WeaponStyles;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationFall", menuName = "Animation Manager/Actions/Unit Prone")]
public class UnitAnimationActionProne : UnitAnimationAction
{
	private class ActionData
	{
		public bool FallingFinished;

		public bool HasLyingClip;

		public float FallingTime;

		public bool StandUp;

		public bool FastForwarding;
	}

	private const float MinLayingTime = 0.5f;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	private BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings.Get();

	[CanBeNull]
	private WeaponStyleProneData NonCombatSettings => WeaponStyleSettings[WeaponAnimationStyle.NonCombat]?.Prone;

	public override UnitAnimationType Type => UnitAnimationType.Prone;

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
			m_ClipWrappersHashSet.AddRange(WeaponStyleSettings.EnumerateProneClips());
		}
		return m_ClipWrappersHashSet;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		WeaponAnimationStyle weaponStyle = handle.WeaponStyle;
		AnimationClipWrapper lyingClip = GetLyingClip(weaponStyle);
		ActionData actionData2 = (ActionData)(handle.ActionData = new ActionData());
		handle.HasCrossfadePriority = true;
		handle.AnimationLayer = AnimationLayerType.Prone;
		AnimationClipWrapper fallingClip = GetFallingClip(weaponStyle);
		AnimationClipWrapper animationClipWrapper = (handle.DeathFromProne ? lyingClip : fallingClip);
		animationClipWrapper = (animationClipWrapper ? animationClipWrapper : fallingClip);
		if (animationClipWrapper != null && (bool)animationClipWrapper.AnimationClip)
		{
			handle.StartClip(animationClipWrapper, lyingClip ? ClipDurationType.Oneshot : ClipDurationType.Endless);
		}
		else
		{
			PFLog.Animations.Error(this, "No clip for prone");
		}
		if ((bool)lyingClip)
		{
			handle.ActiveAnimation.ChangeTransitionTime(TransitionIn);
		}
		actionData2.HasLyingClip = lyingClip;
		actionData2.FallingTime = (animationClipWrapper ? animationClipWrapper.Length : 0f);
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		AbstractUnitEntity data = handle.Unit.Data;
		if (data.LifeState.IsFinallyDead && handle.DeathFromProne && !actionData.FastForwarding && !data.GetOptional<UnitPartCompanion>())
		{
			actionData.FallingFinished = true;
			handle.Unit.Animator.enabled = false;
			handle.Manager.SuspendEvents();
			handle.Release();
			handle.Manager.Disabled = true;
		}
		else if (!actionData.StandUp)
		{
			if (actionData.HasLyingClip && !actionData.FallingFinished)
			{
				actionData.FallingFinished = true;
				AnimationClipWrapper lyingClip = GetLyingClip(handle.WeaponStyle);
				handle.StartClip(lyingClip, ClipDurationType.Endless);
				handle.ActiveAnimation.TransitionIn = TransitionIn;
			}
			else
			{
				handle.Release();
			}
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		base.OnUpdate(handle, deltaTime);
		AbstractUnitEntity data = handle.Unit.Data;
		if (data == null || (data.LifeState.IsFinallyDead && handle.DeathFromProne && !data.GetOptional<UnitPartCompanion>()))
		{
			ActionData actionData = (ActionData)handle.ActionData;
			if (IsActuallyProne(handle) && handle.GetTime() >= actionData.FallingTime + 0.5f)
			{
				handle.Unit.Animator.enabled = false;
				handle.FinishInternal();
				handle.Manager.StopEvents();
				handle.Release();
				handle.Manager.Disabled = true;
			}
		}
		if (handle.ActiveAnimation == null)
		{
			handle.Release();
		}
	}

	public void SwitchToExit(UnitAnimationActionHandle handle)
	{
		if (handle.ActionData == null)
		{
			handle.Release();
			return;
		}
		ActionData actionData = handle.ActionData as ActionData;
		if (!actionData.StandUp)
		{
			actionData.StandUp = true;
			AnimationClipWrapper standUpClip = GetStandUpClip(handle.WeaponStyle, handle.Unit.Data.IsInCombat);
			if ((bool)standUpClip)
			{
				handle.StartClip(standUpClip, ClipDurationType.Oneshot);
				handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
				OnUpdate(handle, 0f);
			}
			else
			{
				handle.Release();
			}
		}
	}

	public void FastForward(UnitAnimationActionHandle handle)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (handle.ActiveAnimation == null)
		{
			actionData.FallingFinished = true;
			handle.Unit.RigidbodyController?.CancelRagdoll();
			AnimationClipWrapper lyingClip = GetLyingClip(handle.WeaponStyle);
			if ((bool)lyingClip)
			{
				handle.StartClip(lyingClip, ClipDurationType.Endless);
				return;
			}
			AnimationClipWrapper fallingClip = GetFallingClip(handle.WeaponStyle);
			handle.StartClip(fallingClip, ClipDurationType.Endless);
		}
		actionData.FastForwarding = true;
		if (actionData.HasLyingClip)
		{
			if (!actionData.FallingFinished)
			{
				handle.ActiveAnimation.TransitionOut = 0f;
				handle.ActiveAnimation.StartTransitionOut();
				handle.ActiveAnimation.StopEvents();
				handle.ActiveAnimation.TransitionIn = 0f;
			}
		}
		else
		{
			handle.ActiveAnimation.SetTime(10f);
			handle.ActiveAnimation.SetWeight(1f);
			handle.UpdateInternal(0.1f);
		}
		actionData.FastForwarding = false;
		actionData.FallingFinished = true;
	}

	public bool IsActuallyProne(UnitAnimationActionHandle handle)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (actionData.FallingFinished || handle.IsReleased)
		{
			return true;
		}
		if (!actionData.HasLyingClip && handle.GetTime() >= actionData.FallingTime)
		{
			return true;
		}
		return false;
	}

	private AnimationClipWrapper GetFallingClip(WeaponAnimationStyle weaponStyle)
	{
		WeaponStyleProneData obj = WeaponStyleSettings[weaponStyle]?.Prone;
		object obj2 = ((obj != null) ? obj.ProneIn.Or(null) : null);
		if (obj2 == null)
		{
			WeaponStyleProneData nonCombatSettings = NonCombatSettings;
			if (nonCombatSettings == null)
			{
				return null;
			}
			obj2 = nonCombatSettings.ProneIn;
		}
		return (AnimationClipWrapper)obj2;
	}

	private AnimationClipWrapper GetStandUpClip(WeaponAnimationStyle weaponStyle, bool isInCombat)
	{
		WeaponStyleProneData weaponStyleProneData = WeaponStyleSettings[weaponStyle]?.Prone;
		object obj;
		if (!isInCombat)
		{
			obj = ((weaponStyleProneData != null) ? weaponStyleProneData.ProneOutNonCombat.Or(null) : null);
			if (obj == null)
			{
				return NonCombatSettings?.ProneOutNonCombat;
			}
		}
		else
		{
			obj = ((weaponStyleProneData != null) ? weaponStyleProneData.ProneOutCombat.Or(null) : null);
			if (obj == null)
			{
				WeaponStyleProneData nonCombatSettings = NonCombatSettings;
				if (nonCombatSettings == null)
				{
					return null;
				}
				obj = nonCombatSettings.ProneOutCombat;
			}
		}
		return (AnimationClipWrapper)obj;
	}

	private AnimationClipWrapper GetLyingClip(WeaponAnimationStyle weaponStyle)
	{
		WeaponStyleProneData obj = WeaponStyleSettings[weaponStyle]?.Prone;
		object obj2 = ((obj != null) ? obj.Prone.Or(null) : null);
		if (obj2 == null)
		{
			WeaponStyleProneData nonCombatSettings = NonCombatSettings;
			if (nonCombatSettings == null)
			{
				return null;
			}
			obj2 = nonCombatSettings.Prone;
		}
		return (AnimationClipWrapper)obj2;
	}
}
