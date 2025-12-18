using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation.Events;
using Kingmaker.Visual.Animation.WeaponStyles;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "WarhammerUnitAnimationActionHandAttack", menuName = "Animation Manager/Actions/Unit Hand Attack (WH)")]
public class WarhammerUnitAnimationActionHandAttack : UnitAnimationAction
{
	private class Data
	{
		public class ClipData
		{
			public readonly AnimationClipWrapper Clip;

			public readonly float SpeedScale;

			public ClipData(AnimationClipWrapper clip, float speedScale)
			{
				Clip = clip;
				SpeedScale = speedScale;
			}
		}

		public readonly List<ClipData> Sequence = new List<ClipData>();

		private float StartAimingWithIKTimestamp;

		private float StopAimingWithIKTimestamp;

		private float StartAimingDuration;

		private float StopAimingDuration;

		private Transform Target;

		private bool IsMainHand;

		private AimIKController AimIKController;

		public int Index { get; set; } = -1;


		public int ExpectedActEventsCount { get; set; }

		public float CurrentAnimationSpeedScale { get; set; } = 1f;


		public bool Invalid { get; set; }

		public bool IsIKAimingEnabled { get; set; }

		public void SetupIKAimingData(AnimationClipWrapper clipWrapper, UnitAnimationManager manager, bool isMainHand)
		{
			if (clipWrapper.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventAct, out var result) && manager.AimIKTargetsQueue.Count != 0)
			{
				Target = manager.AimIKTargetsQueue.Dequeue();
				AimIKController component = manager.gameObject.GetComponent<AimIKController>();
				if (!(component == null))
				{
					AimIKController = component;
					IsMainHand = isMainHand;
					StartAimingWithIKTimestamp = GetStartAimingWithIKTimestamp(clipWrapper, result);
					StopAimingWithIKTimestamp = GetStopAimingWithIKTimestamp(clipWrapper, result);
					StartAimingDuration = result.Time - StartAimingWithIKTimestamp;
					StopAimingDuration = StopAimingWithIKTimestamp - result.Time;
				}
			}
		}

		private static float GetStartAimingWithIKTimestamp(AnimationClipWrapper clipWrapper, AnimationClipEvent actEvent)
		{
			if (clipWrapper.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventAiming animationClipEventAiming && animationClipEventAiming.Type == AimingAnimationEventType.Start, out var result))
			{
				return result.Time;
			}
			float num = ((actEvent.Time > 0.2f) ? (actEvent.Time - 0.15f) : actEvent.Time);
			return Mathf.Max(0f, actEvent.Time - num);
		}

		private static float GetStopAimingWithIKTimestamp(AnimationClipWrapper clipWrapper, AnimationClipEvent actEvent)
		{
			if (clipWrapper.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventAiming animationClipEventAiming && animationClipEventAiming.Type == AimingAnimationEventType.Finish, out var result))
			{
				return result.Time;
			}
			return actEvent.Time + 0.033f;
		}

		public void UpdateIKAiming(float time)
		{
			if (IsIKAimingEnabled && !(AimIKController == null))
			{
				if (time >= StartAimingWithIKTimestamp && Target != null)
				{
					StartAim();
					Target = null;
				}
				if (time >= StopAimingWithIKTimestamp)
				{
					StopAim();
					AimIKController = null;
				}
			}
		}

		public void DisableIKAiming()
		{
			if (IsIKAimingEnabled && !(AimIKController == null))
			{
				AimIKController.DisableIKAiming();
				AimIKController = null;
			}
		}

		private void StartAim()
		{
			AimIKController.StartAim(Target, IsMainHand, StartAimingDuration);
		}

		private void StopAim()
		{
			AimIKController.StopAim(StopAimingDuration);
		}
	}

	[SerializeField]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	[CanBeNull]
	private BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings?.Get();

	public override UnitAnimationType Type => UnitAnimationType.Attack;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => CollectClipWrappers();

	private static Data GetData(UnitAnimationActionHandle handle)
	{
		return (Data)handle.ActionData;
	}

	private IEnumerable<AnimationClipWrapper> CollectClipWrappers()
	{
		if (m_ClipWrappersHashSet != null)
		{
			return m_ClipWrappersHashSet;
		}
		m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
		if (WeaponStyleSettings != null)
		{
			m_ClipWrappersHashSet.AddRange(WeaponStyleSettings.EnumerateAttackClips());
		}
		return m_ClipWrappersHashSet;
	}

	[CanBeNull]
	private AttackAnimationData GetAttackData(UnitAnimationActionHandle handle)
	{
		WeaponStyleAttackData weaponStyleAttackData = WeaponStyleSettings?[handle.WeaponStyle]?.Attack;
		if (weaponStyleAttackData == null)
		{
			PFLog.Animations.Error(this, $"No animation for weapon style '{handle.WeaponStyle}' in action '{base.name}'");
			return null;
		}
		if (!handle.IsMainHandAttack)
		{
			return weaponStyleAttackData.GetOffHandAttackData(handle.AttackWeaponType, handle.AttackType);
		}
		return weaponStyleAttackData.GetMainHandAttackData(handle.AttackWeaponType, handle.AttackType);
	}

	private bool CheckCanAimWithIK(UnitAnimationActionHandle handle)
	{
		if (handle.AttackWeaponType.IsRanged() && !handle.IsBurst)
		{
			return !handle.Spell.IsAoE;
		}
		return false;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		Data data = (Data)(handle.ActionData = new Data
		{
			IsIKAimingEnabled = CheckCanAimWithIK(handle)
		});
		handle.SpeedScale = Game.CombatAnimSpeedUp;
		AttackAnimationData attackData = GetAttackData(handle);
		if (attackData == null)
		{
			PFLog.Animations.Error(handle.Manager.View, $"No attack animation for {handle.WeaponStyle}/{handle.AttackWeaponType}/{handle.AttackType}");
			data.Invalid = true;
			return;
		}
		if ((handle.IsBurst && (!handle.Manager.NeedStepOut || handle.Manager.StepOutDirectionAnimationType == UnitAnimationActionCover.StepOutDirectionAnimationType.None)) || (handle.NeedPreparingForShooting && handle.IsPreparingForShooting))
		{
			AnimationClipWrapper @in = attackData.In;
			if (@in != null)
			{
				if (@in.AnimationClip == null)
				{
					data.Invalid = true;
					return;
				}
				data.Sequence.Add(new Data.ClipData(@in, 1f));
			}
		}
		if (!handle.IsPreparingForShooting)
		{
			int num = ((!handle.IsBurst) ? 1 : handle.BurstCount);
			int count = attackData.Clips.Count;
			int num2 = PFStatefulRandom.Visuals.Animation1.Range(0, count);
			for (int i = 0; i < num; i++)
			{
				AnimationClipWrapper animationClipWrapper = attackData.Clips[(num2 + i) % count];
				if (animationClipWrapper == null || animationClipWrapper.AnimationClip == null)
				{
					data.Invalid = true;
					return;
				}
				float speedScaleByRpm = GetSpeedScaleByRpm(animationClipWrapper, handle);
				data.Sequence.Add(new Data.ClipData(animationClipWrapper, speedScaleByRpm));
			}
			if ((handle.IsBurst && (!handle.Manager.NeedStepOut || handle.Manager.StepOutDirectionAnimationType == UnitAnimationActionCover.StepOutDirectionAnimationType.None)) || (handle.NeedPreparingForShooting && handle.IsPreparingForShooting))
			{
				AnimationClipWrapper @out = attackData.Out;
				if (@out != null)
				{
					if (@out.AnimationClip == null)
					{
						data.Invalid = true;
						return;
					}
					data.Sequence.Add(new Data.ClipData(@out, 1f));
				}
			}
		}
		if (handle.Manager.NeedStepOut && handle.Manager.StepOutDirectionAnimationType != 0)
		{
			data.Invalid = true;
		}
		else if ((!handle.NeedPreparingForShooting || handle.IsPreparingForShooting) && !Next(handle))
		{
			handle.Release();
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (GetData(handle).Invalid)
		{
			UpdateInvalid(handle);
		}
		else
		{
			Update(handle);
		}
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		GetData(handle).DisableIKAiming();
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		Data data = GetData(handle);
		if (!data.Invalid && data.Index >= data.Sequence.Count)
		{
			base.OnTransitionOutStarted(handle);
		}
	}

	private static void Update(UnitAnimationActionHandle handle)
	{
		bool flag = false;
		if (handle.Manager.TryGetComponent<MechadendriteSettings>(out var component))
		{
			UnitEntityView componentInParent = component.GetComponentInParent<UnitEntityView>();
			UnitAnimationManager unitAnimationManager = ((componentInParent != null) ? componentInParent.AnimationManager : null);
			flag = unitAnimationManager != null && (unitAnimationManager.BlockAttackAnimation || !(unitAnimationManager.CurrentAction.Action is WarhammerUnitAnimationActionHandAttack));
		}
		if (handle.Manager.BlockAttackAnimation || flag)
		{
			handle.SpeedScale = 0f;
			return;
		}
		Data data = GetData(handle);
		handle.SpeedScale = data.CurrentAnimationSpeedScale * Game.CombatAnimSpeedUp;
		float time = handle.GetTime();
		data.UpdateIKAiming(time);
		if (!(time < GetNextClipStartTime(handle)) && (!handle.NeedPreparingForShooting || !handle.IsPreparingForShooting) && (!handle.NeedPreparingForShooting || !(Math.Abs(handle.Manager.Orientation - handle.Manager.UseAbilityDirection) > 10f)) && !Next(handle))
		{
			handle.Release();
		}
	}

	private static bool Next(UnitAnimationActionHandle handle)
	{
		Data data = GetData(handle);
		Data.ClipData clipData = data.Sequence.Get(data.Index + 1);
		if (clipData?.Clip == null)
		{
			return false;
		}
		if (data.ExpectedActEventsCount > handle.ActEventsCounter)
		{
			handle.ActEventsCounter = data.ExpectedActEventsCount;
		}
		data.Index++;
		data.ExpectedActEventsCount += clipData.Clip.Events.Count((AnimationClipEvent e) => e is AnimationClipEventAct);
		data.SetupIKAimingData(clipData.Clip, handle.Manager, handle.IsMainHandAttack);
		handle.Action.TransitionIn = 0f;
		handle.StartClip(clipData.Clip, ClipDurationType.Oneshot);
		handle.ActiveAnimation.ChangeTransitionTime(0f);
		handle.SpeedScale = clipData.SpeedScale;
		data.CurrentAnimationSpeedScale = clipData.SpeedScale;
		return true;
	}

	private static float GetNextClipStartTime(UnitAnimationActionHandle handle)
	{
		Data data = GetData(handle);
		float num = 0f;
		for (int i = 0; i <= data.Index; i++)
		{
			num = ((data.Sequence.Count <= 2 || i <= 0 || i >= data.Sequence.Count - 2) ? (num + data.Sequence[i].Clip.Length) : (num + (data.Sequence[i].Clip.Length + handle.BurstAnimationDelay)));
		}
		return num;
	}

	private float GetSpeedScaleByRpm(AnimationClipWrapper attackClip, UnitAnimationActionHandle handle)
	{
		if (!handle.IsBurst || handle.CustomRpm == 0)
		{
			return 1f;
		}
		float num = 60f / (float)handle.CustomRpm;
		return attackClip.Length / num;
	}

	private static void UpdateInvalid(UnitAnimationActionHandle handle)
	{
		float num = 0.3f + handle.BurstAnimationDelay;
		int num2 = ((!handle.IsBurst) ? 1 : handle.BurstCount);
		float time = handle.GetTime();
		int actEventsCounter = Math.Clamp(Mathf.FloorToInt((time - 0.45f) / num), 0, num2);
		handle.ActEventsCounter = actEventsCounter;
		if (time >= 0.79999995f + num * (float)num2)
		{
			handle.Release();
		}
	}
}
