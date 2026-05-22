using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.WeaponStyles;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "WarhammerBuffLoopAction", menuName = "Animation Manager/Actions/Buff Loop Animation")]
public class WarhammerBuffLoopAction : UnitAnimationAction
{
	private enum State
	{
		Start,
		Loop,
		End
	}

	private class ActionData
	{
		[NotNull]
		public WeaponStyleCustomLoopActionData.Entry Data;

		public State State;

		public WeaponAnimationStyle WeaponStyle;
	}

	[SerializeField]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	[CanBeNull]
	private BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings?.Get();

	public override UnitAnimationType Type => UnitAnimationType.BuffLoopAction;

	public override bool DontReleaseOnInterrupt => true;

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
			m_ClipWrappersHashSet.AddRange(from c in WeaponStyleSettings.EnumerateCustomLoopActionClips()
				where c != null
				select c);
		}
		return m_ClipWrappersHashSet;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		StartAnimation(handle);
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		ActionData actionData = handle.ActionData as ActionData;
		if (actionData == null || actionData.WeaponStyle != handle.WeaponStyle)
		{
			StartAnimation(handle, actionData);
		}
	}

	private void StartAnimation(UnitAnimationActionHandle handle, ActionData actionData = null)
	{
		WeaponStyleCustomLoopActionData.Entry animationData = GetAnimationData(handle);
		if (animationData == null)
		{
			PFLog.Animations.Warning($"BuffLoopAction: no animation found for {handle.WeaponStyle}");
			return;
		}
		if (actionData == null)
		{
			actionData = new ActionData();
		}
		actionData.Data = animationData;
		actionData.WeaponStyle = handle.WeaponStyle;
		handle.ActionData = actionData;
		handle.AnimationLayer = UnitAnimationLayerType.BuffLoopAction;
		if ((bool)animationData.In)
		{
			actionData.State = State.Start;
			handle.StartClip(animationData.In, ClipDurationType.Oneshot);
			handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
		}
		else if ((bool)animationData.Loop)
		{
			actionData.State = State.Loop;
			handle.StartClip(animationData.Loop, ClipDurationType.Endless);
			handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if (!(handle.ActionData is ActionData actionData))
		{
			handle.Release();
			return;
		}
		if (actionData.State == State.Start)
		{
			WeaponStyleCustomLoopActionData.Entry data = actionData.Data;
			if ((bool)data.Loop)
			{
				actionData.State = State.Loop;
				handle.StartClip(data.Loop, ClipDurationType.Endless);
			}
			else
			{
				handle.Release();
			}
		}
		if (actionData.State == State.End)
		{
			handle.Release();
		}
	}

	public void SwitchToExit(UnitAnimationActionHandle handle)
	{
		if (!(handle.ActionData is ActionData actionData))
		{
			handle.Release();
		}
		else if (actionData.State != State.End)
		{
			actionData.State = State.End;
			WeaponStyleCustomLoopActionData.Entry animationData = GetAnimationData(handle);
			if ((bool)animationData?.Out)
			{
				handle.StartClip(animationData.Out, ClipDurationType.Oneshot);
			}
			else
			{
				handle.Release();
			}
		}
	}

	public void InterruptAnimation(UnitAnimationActionHandle handle)
	{
		if (!(handle.ActionData is ActionData actionData))
		{
			handle.Release();
		}
		else
		{
			actionData.State = State.End;
		}
	}

	[CanBeNull]
	private WeaponStyleCustomLoopActionData.Entry GetAnimationData(UnitAnimationActionHandle handle)
	{
		if (WeaponStyleSettings == null)
		{
			return null;
		}
		BlueprintCustomLoopActionType customLoopActionType = handle.CustomLoopActionType;
		WeaponStyleCustomLoopActionData.Entry entry = WeaponStyleSettings[handle.WeaponStyle]?.CustomLoopAction?[customLoopActionType];
		if (entry != null)
		{
			return entry;
		}
		if (handle.Unit.Data.IsInCombat && handle.WeaponStyle != WeaponAnimationStyle.MeleeMelee)
		{
			entry = WeaponStyleSettings[WeaponAnimationStyle.MeleeMelee]?.CustomLoopAction?[customLoopActionType];
		}
		WeaponStyleCustomLoopActionData.Entry entry2 = entry;
		if (entry2 == null)
		{
			BlueprintWeaponStyleAnimationSet blueprintWeaponStyleAnimationSet = WeaponStyleSettings[WeaponAnimationStyle.NonCombat];
			if (blueprintWeaponStyleAnimationSet == null)
			{
				return null;
			}
			WeaponStyleCustomLoopActionData customLoopAction = blueprintWeaponStyleAnimationSet.CustomLoopAction;
			if (customLoopAction == null)
			{
				return null;
			}
			entry2 = customLoopAction[customLoopActionType];
		}
		return entry2;
	}
}
