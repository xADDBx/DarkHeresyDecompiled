using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.WeaponStyles;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationActionDisabled : UnitAnimationAction
{
	private enum State
	{
		EnterDisabled,
		Disabled,
		ExitDisabled
	}

	private class Data
	{
		public State State;

		public float StartLoopTime;
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	private BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings.Get();

	[CanBeNull]
	private WeaponStyleDisabledData NonCombatSettings => WeaponStyleSettings[WeaponAnimationStyle.NonCombat]?.Disabled;

	public override bool DontReleaseOnInterrupt => true;

	public override UnitAnimationType Type => UnitAnimationType.Disabled;

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
			m_ClipWrappersHashSet.AddRange(WeaponStyleSettings.EnumerateDisabledClips());
		}
		return m_ClipWrappersHashSet;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		Data data = new Data
		{
			State = State.EnterDisabled
		};
		WeaponAnimationStyle weaponStyle = handle.WeaponStyle;
		AnimationClipWrapper animation = GetAnimation(weaponStyle, State.EnterDisabled);
		if (animation == null)
		{
			PFLog.Animations.Error($"Disabled In-animation not found for {weaponStyle}");
		}
		else
		{
			handle.StartClip(animation, ClipDurationType.Oneshot);
			handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
			data.StartLoopTime = animation.Length;
		}
		handle.ActionData = data;
		handle.Manager.CurrentEquipHandle = handle;
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (!(handle.ActionData is Data data))
		{
			handle.Release();
		}
		else if (data.State == State.EnterDisabled && handle.GetTime() > data.StartLoopTime)
		{
			data.State = State.Disabled;
			WeaponAnimationStyle weaponStyle = handle.WeaponStyle;
			AnimationClipWrapper animation = GetAnimation(weaponStyle, State.Disabled);
			if (animation == null)
			{
				PFLog.Animations.Error($"Disabled Loop-animation not found for {weaponStyle}");
				return;
			}
			handle.StartClip(animation, ClipDurationType.Endless);
			handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if (handle.ActionData is Data { State: State.ExitDisabled })
		{
			handle.Release();
		}
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		base.OnFinish(handle);
		handle.Manager.CurrentEquipHandle = null;
	}

	public void SwitchToExit(UnitAnimationActionHandle handle)
	{
		if (!(handle.ActionData is Data data))
		{
			handle.Release();
		}
		else if (data.State != State.ExitDisabled)
		{
			data.State = State.ExitDisabled;
			WeaponAnimationStyle weaponStyle = handle.WeaponStyle;
			AnimationClipWrapper animation = GetAnimation(weaponStyle, State.ExitDisabled);
			if (animation == null)
			{
				PFLog.Animations.Error($"Disabled Out-animation not found for {weaponStyle}");
				handle.Release();
			}
			else
			{
				handle.StartClip(animation, ClipDurationType.Oneshot);
				handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
				OnUpdate(handle, 0f);
			}
		}
	}

	[CanBeNull]
	private AnimationClipWrapper GetAnimation(WeaponAnimationStyle weaponStyle, State state)
	{
		WeaponStyleDisabledData weaponStyleDisabledData = WeaponStyleSettings[weaponStyle]?.Disabled;
		return state switch
		{
			State.EnterDisabled => ((weaponStyleDisabledData != null) ? weaponStyleDisabledData.DisabledIn.Or(null) : null) ?? NonCombatSettings?.DisabledIn, 
			State.Disabled => ((weaponStyleDisabledData != null) ? weaponStyleDisabledData.Disabled.Or(null) : null) ?? NonCombatSettings?.Disabled, 
			State.ExitDisabled => ((weaponStyleDisabledData != null) ? weaponStyleDisabledData.DisabledOut.Or(null) : null) ?? NonCombatSettings?.DisabledOut, 
			_ => null, 
		};
	}
}
