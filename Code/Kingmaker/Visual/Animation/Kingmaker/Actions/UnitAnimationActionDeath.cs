using System.Collections.Generic;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationDeath", menuName = "Animation Manager/Actions/Unit Death")]
public class UnitAnimationActionDeath : UnitAnimationAction
{
	private class ActionData
	{
		public bool FallingFinished;

		public AnimationClipWrapper LyingAnimation;

		public bool HasLyingClip => LyingAnimation != null;
	}

	[SerializeField]
	public AnimationClipWrapper DeathAnimation;

	[SerializeField]
	public AnimationClipWrapper LyingAnimation;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			if (DeathAnimation != null)
			{
				yield return DeathAnimation;
			}
			if (LyingAnimation != null)
			{
				yield return LyingAnimation;
			}
		}
	}

	public override bool DontReleaseOnInterrupt => true;

	public override UnitAnimationType Type => UnitAnimationType.Death;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.AnimationLayer = UnitAnimationLayerType.Prone;
		ActionData actionData2 = (ActionData)(handle.ActionData = new ActionData());
		handle.HasCrossfadePriority = true;
		AnimationClipWrapper actionAnimation = GetActionAnimation();
		AbstractUnitEntity data = handle.Unit.Data;
		if (actionAnimation == null)
		{
			PFLog.Animations.Error(this, "No clip for death");
			handle.Release();
			DismembermentHandler.UseWithoutAnimationDeath(data);
		}
		else
		{
			handle.StartClip(actionAnimation, ClipDurationType.Oneshot);
			actionData2.LyingAnimation = GetLyingAnimation();
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (actionData.HasLyingClip && !actionData.FallingFinished)
		{
			actionData.FallingFinished = true;
			handle.StartClip(actionData.LyingAnimation, ClipDurationType.Endless);
			handle.ActiveAnimation.TransitionIn = 0.2f;
		}
		else
		{
			handle.Manager.HandleDeathWithoutAnimation();
		}
	}

	public void FastForward(UnitAnimationActionHandle handle)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (actionData == null)
		{
			PFLog.Animations.Error(this, "No ActionData, FastForward failed");
			return;
		}
		if (handle.ActiveAnimation == null)
		{
			actionData.FallingFinished = true;
			AnimationClipWrapper lyingAnimation = GetLyingAnimation();
			if ((bool)lyingAnimation)
			{
				handle.StartClip(lyingAnimation, ClipDurationType.Endless);
				return;
			}
			handle.StartClip(GetActionAnimation(), ClipDurationType.Endless);
		}
		if (actionData.HasLyingClip)
		{
			if (!actionData.FallingFinished)
			{
				handle.ActiveAnimation.TransitionOut = 0f;
				handle.ActiveAnimation.StartTransitionOut();
				handle.ActiveAnimation.TransitionIn = 0f;
			}
		}
		else
		{
			handle.ActiveAnimation.SetTime(10f);
			handle.UpdateInternal(10f);
		}
		actionData.FallingFinished = true;
	}

	private AnimationClipWrapper GetActionAnimation()
	{
		return DeathAnimation;
	}

	private AnimationClipWrapper GetLyingAnimation()
	{
		return LyingAnimation;
	}
}
