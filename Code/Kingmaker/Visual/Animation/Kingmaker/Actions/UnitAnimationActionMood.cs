using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.DialogSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationActionMood : UnitAnimationAction
{
	private class ActionData
	{
		public Mood CurrentMood;

		public bool IsMoodMaskApplied;

		public AnimationClipWrapper MoodPose;
	}

	[Serializable]
	public class MoodSettings
	{
		public Mood Mood;

		public AnimationClipWrapper Pose;
	}

	[SerializeField]
	private MoodSettings[] m_MoodSettings;

	public override bool DontReleaseOnInterrupt => true;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => from s in m_MoodSettings
		where s.Pose != null
		select s.Pose;

	public override UnitAnimationType Type => UnitAnimationType.Mood;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.AnimationLayer = UnitAnimationLayerType.Mood;
		ActionData actionData = new ActionData();
		if (handle.Unit.Or(null)?.Data is UnitEntity entity)
		{
			actionData.CurrentMood = entity.GetMood();
		}
		if (actionData.CurrentMood != 0)
		{
			AnimationClipWrapper moodPoseClip = GetMoodPoseClip(actionData.CurrentMood);
			if (moodPoseClip != null)
			{
				handle.StartClip(moodPoseClip, ClipDurationType.Endless);
			}
		}
		handle.ActionData = actionData;
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (!(handle.Unit.Or(null)?.Data is UnitEntity entity))
		{
			return;
		}
		ActionData actionData = (ActionData)handle.ActionData;
		bool flag = IsMoodMaskCanBeApplied(handle);
		if (actionData.IsMoodMaskApplied && !flag)
		{
			if (handle.ActiveAnimation != null)
			{
				handle.ActiveAnimation.StartTransitionOut();
			}
			actionData.IsMoodMaskApplied = false;
			return;
		}
		if (!actionData.IsMoodMaskApplied && flag)
		{
			if (actionData.MoodPose != null)
			{
				handle.StartClip(actionData.MoodPose, ClipDurationType.Endless);
			}
			actionData.IsMoodMaskApplied = true;
		}
		Mood mood = entity.GetMood();
		if (mood == actionData.CurrentMood)
		{
			return;
		}
		actionData.CurrentMood = mood;
		if (mood == Mood.Neutral)
		{
			actionData.MoodPose = null;
			handle.ActiveAnimation.StartTransitionOut();
			return;
		}
		AnimationClipWrapper moodPoseClip = GetMoodPoseClip(mood);
		if (moodPoseClip != null)
		{
			actionData.MoodPose = moodPoseClip;
			handle.StartClip(moodPoseClip, ClipDurationType.Endless);
		}
	}

	private bool IsMoodMaskCanBeApplied(UnitAnimationActionHandle handle)
	{
		AbstractUnitEntity abstractUnitEntity = handle.Unit.Or(null)?.Data;
		if (abstractUnitEntity == null)
		{
			return false;
		}
		if (abstractUnitEntity.IsInCombat)
		{
			return false;
		}
		return abstractUnitEntity.AnimationManager.IsMoodMaskCanBeApplied;
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	private AnimationClipWrapper GetMoodPoseClip(Mood mood)
	{
		return m_MoodSettings?.FirstOrDefault((MoodSettings s) => s.Mood == mood)?.Pose;
	}
}
