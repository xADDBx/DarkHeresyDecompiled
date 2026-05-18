using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationActionTurnAround : UnitAnimationAction
{
	private class ActionData
	{
		public float AngularSpeed;

		public float TargetOrientation;

		public AnimationClipWrapper Clip;

		public int RemainedPlaybackTimes;

		public float SpeedScale;

		public float OnePlaybackDuration;

		public float StartAnimationTimestamp;

		public void Reset()
		{
			Clip = null;
			RemainedPlaybackTimes = 0;
		}
	}

	private enum PlaybackType
	{
		Stretch,
		Loop
	}

	[Serializable]
	private class TurnAroundSettings
	{
		public float MaxDeltaAngle;

		public AnimationClipWrapper TurnLeft;

		public AnimationClipWrapper TurnRight;

		public PlaybackType PlaybackType;
	}

	[Serializable]
	private class SettingsListWrapper
	{
		public List<TurnAroundSettings> Settings = new List<TurnAroundSettings>();
	}

	[SerializeField]
	private List<TurnAroundSettings> m_Settings = new List<TurnAroundSettings>();

	[SerializeField]
	private bool m_UseCustomSettingsInCombat;

	[SerializeField]
	[ShowIf("m_UseCustomSettingsInCombat")]
	private SettingsListWrapper m_SettingsForCombat = new SettingsListWrapper();

	public override bool DontReleaseOnInterrupt => true;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			foreach (TurnAroundSettings setting in m_Settings)
			{
				if (setting.TurnLeft != null)
				{
					yield return setting.TurnLeft;
				}
				if (setting.TurnRight != null)
				{
					yield return setting.TurnRight;
				}
			}
			if (!m_UseCustomSettingsInCombat)
			{
				yield break;
			}
			foreach (TurnAroundSettings setting in m_SettingsForCombat.Settings)
			{
				if (setting.TurnLeft != null)
				{
					yield return setting.TurnLeft;
				}
				if (setting.TurnRight != null)
				{
					yield return setting.TurnRight;
				}
			}
		}
	}

	public override UnitAnimationType Type => UnitAnimationType.TurnAround;

	private TurnAroundSettings GetSettings(float deltaAngle, bool isInCombat)
	{
		if (!isInCombat || !m_UseCustomSettingsInCombat)
		{
			return GetSettings(m_Settings, deltaAngle);
		}
		return GetSettings(m_SettingsForCombat.Settings, deltaAngle);
	}

	private TurnAroundSettings GetSettings(List<TurnAroundSettings> settings, float deltaAngle)
	{
		return settings.OrderBy((TurnAroundSettings s) => s.MaxDeltaAngle).FirstOrDefault((TurnAroundSettings setting) => deltaAngle <= setting.MaxDeltaAngle);
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.AnimationLayer = UnitAnimationLayerType.TurnAround;
		handle.ActionData = new ActionData();
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		AbstractUnitEntity abstractUnitEntity = handle.Unit.Or(null)?.Data;
		if (abstractUnitEntity != null && !Mathf.Approximately(abstractUnitEntity.Orientation, abstractUnitEntity.DesiredOrientation))
		{
			ActionData actionData = (ActionData)handle.ActionData;
			TryUpdateActionData(actionData, abstractUnitEntity, handle.GetTime());
			if (actionData.RemainedPlaybackTimes > 0 && handle.GetTime() >= actionData.StartAnimationTimestamp)
			{
				handle.StartClip(actionData.Clip, ClipDurationType.Oneshot);
				handle.SpeedScale = actionData.SpeedScale;
				actionData.RemainedPlaybackTimes--;
				actionData.StartAnimationTimestamp = handle.GetTime() + actionData.OnePlaybackDuration;
			}
		}
	}

	private void TryUpdateActionData(ActionData data, AbstractUnitEntity unit, float time)
	{
		if (!Mathf.Approximately(data.TargetOrientation, unit.DesiredOrientation))
		{
			float deltaAngle = Mathf.DeltaAngle(unit.Orientation, unit.DesiredOrientation);
			data.AngularSpeed = unit.MaybeMovementAgent?.CurrentAngularSpeed ?? (unit.IsInCombat ? 360f : 180f);
			UpdateActionData(data, unit.IsInCombat, unit.DesiredOrientation, deltaAngle, time);
		}
	}

	private void UpdateActionData([NotNull] ActionData data, bool isInCombat, float targetOrientation, float deltaAngle, float startTime)
	{
		data.Reset();
		data.TargetOrientation = targetOrientation;
		float num = Mathf.Abs(deltaAngle);
		TurnAroundSettings settings = GetSettings(num, isInCombat);
		if (settings == null)
		{
			PFLog.Animations.Error(this, $"Settings are not found for angle of {num} degrees");
			return;
		}
		AnimationClipWrapper animationClipWrapper = ((deltaAngle > 0f) ? settings.TurnRight : settings.TurnLeft);
		if (animationClipWrapper == null)
		{
			PFLog.Animations.Error(this, string.Format("No clip for turning {0} by {1} degrees", (deltaAngle > 0f) ? "right" : "left", num));
		}
		else
		{
			if (num < 1f)
			{
				return;
			}
			data.Clip = animationClipWrapper;
			float num2 = num / data.AngularSpeed;
			float length = data.Clip.Length;
			if (settings.PlaybackType == PlaybackType.Loop)
			{
				float num3 = num2 / length;
				float num4 = Mathf.Ceil(num3);
				if (num3 < 1f || (num4 > num3 && (double)(num4 - num3) < 0.5))
				{
					num3 += 1f;
				}
				data.RemainedPlaybackTimes = Mathf.FloorToInt(num3);
			}
			else
			{
				data.RemainedPlaybackTimes = 1;
			}
			data.OnePlaybackDuration = num2 / (float)data.RemainedPlaybackTimes;
			data.SpeedScale = length / data.OnePlaybackDuration;
			data.StartAnimationTimestamp = startTime;
		}
	}
}
