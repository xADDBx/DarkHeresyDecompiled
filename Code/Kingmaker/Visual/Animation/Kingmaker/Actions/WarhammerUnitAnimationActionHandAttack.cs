using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		private class AimingData
		{
			public Vector3 Target;

			public float StartAimTime;

			public float Duration;

			public AttackingLimb AttackingLimb;

			public bool IsAlreadyAiming;

			public bool IsStopAimAfterShot;

			public float RecoilProcessingTime;

			public AimingData(Vector3 target, float startAimTime, float duration, AttackingLimb attackingLimb)
			{
				Target = target;
				StartAimTime = startAimTime;
				Duration = duration;
				AttackingLimb = attackingLimb;
			}
		}

		private AimIKController m_AimIKController;

		private readonly List<AimingData> m_AimingDataList = new List<AimingData>();

		private int m_AimTargetIndex;

		public readonly List<ClipData> Sequence = new List<ClipData>();

		public int Index { get; set; } = -1;


		public int ExpectedActEventsCount { get; set; }

		public float CurrentAnimationSpeedScale { get; set; } = 1f;


		public bool Invalid { get; set; }

		public bool IsIKAimingEnabled { get; set; }

		public void SetupIKAimingData(UnitAnimationManager manager, AttackingLimb attackingLimb)
		{
			AimIKController component = manager.gameObject.GetComponent<AimIKController>();
			if (component == null)
			{
				return;
			}
			m_AimIKController = component;
			m_AimingDataList.Clear();
			m_AimTargetIndex = 0;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			int num5 = 0;
			int count = manager.AimIKTargets.Count;
			if (count == 0)
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < Sequence.Count; i++)
			{
				ClipData clipData = Sequence[i];
				if (num5 >= count)
				{
					num += clipData.Clip.Length;
					num2 += clipData.Clip.Length / clipData.SpeedScale;
					continue;
				}
				float num6 = clipData.Clip.Length;
				if (clipData.Clip.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventAct, out var result))
				{
					num6 = result.Time;
				}
				float num7 = num + num6;
				float num8 = num2 + num6 / clipData.SpeedScale;
				float num9 = num8 - num4;
				if (flag && num9 > 0.29f)
				{
					float num10 = num9 * 0.66f;
					num4 += num10;
					m_AimingDataList[num5 - 1].IsStopAimAfterShot = true;
					m_AimingDataList[num5 - 1].RecoilProcessingTime = num10;
					flag = false;
				}
				float num11 = ((!flag) ? CalcScaledStartAimTime(clipData, num2, num8, num4) : num4);
				flag = true;
				float num12 = num8 - num11;
				float startAimTime = CalcUnscaledStartAimTime(i, num7, num6, num12);
				m_AimingDataList.Add(new AimingData(manager.AimIKTargets[num5], startAimTime, num12, attackingLimb));
				num5++;
				num3 = num7;
				num4 = num8;
				num += clipData.Clip.Length;
				num2 += clipData.Clip.Length / clipData.SpeedScale;
			}
			m_AimingDataList[num5 - 1].RecoilProcessingTime = Mathf.Max(num - num3, 1f);
		}

		private float CalcUnscaledStartAimTime(int clipIndex, float actTime, float actEventTime, float scaledDuration)
		{
			float speedScale = Sequence[clipIndex].SpeedScale;
			if (scaledDuration < actEventTime / speedScale)
			{
				return actTime - scaledDuration * speedScale;
			}
			float speedScale2 = Sequence[clipIndex - 1].SpeedScale;
			return actTime - actEventTime - (scaledDuration - actEventTime / speedScale) * speedScale2;
		}

		private float CalcScaledStartAimTime(ClipData clipData, float scaledTime, float scaledActTime, float lastScaledActTime)
		{
			if (!TryGetStartAimEvent(clipData.Clip, out var startAimTime))
			{
				if (!(scaledActTime - lastScaledActTime > 0.5f))
				{
					return lastScaledActTime;
				}
				return scaledActTime - 0.5f;
			}
			return scaledTime + startAimTime / clipData.SpeedScale;
		}

		private bool TryGetStartAimEvent(AnimationClipWrapper clipWrapper, out float startAimTime)
		{
			startAimTime = (clipWrapper.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventAiming animationClipEventAiming && animationClipEventAiming.Type == AimingAnimationEventType.Start, out var result) ? result.Time : (-1f));
			return startAimTime >= 0f;
		}

		public void UpdateIKAiming(float time, int actedCount)
		{
			if (!IsIKAimingEnabled || m_AimIKController == null || m_AimTargetIndex >= m_AimingDataList.Count)
			{
				return;
			}
			AimingData aimingData = m_AimingDataList[m_AimTargetIndex];
			if (time >= aimingData.StartAimTime && !aimingData.IsAlreadyAiming)
			{
				StartAim(aimingData.Target, aimingData.AttackingLimb, aimingData.Duration);
				aimingData.IsAlreadyAiming = true;
			}
			if (actedCount > m_AimTargetIndex)
			{
				m_AimTargetIndex++;
				if (m_AimTargetIndex >= m_AimingDataList.Count || aimingData.IsStopAimAfterShot)
				{
					StopAim(aimingData.RecoilProcessingTime);
				}
			}
		}

		public void DisableIKAiming()
		{
			if (IsIKAimingEnabled && !(m_AimIKController == null))
			{
				m_AimIKController.DisableIKAiming();
				m_AimIKController = null;
			}
		}

		private void StartAim(Vector3 target, AttackingLimb attackingLimb, float duration)
		{
			m_AimIKController.StartAim(target, attackingLimb, duration);
		}

		private void StopAim(float stopAimingDuration)
		{
			m_AimIKController.StopAim(stopAimingDuration);
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
			m_ClipWrappersHashSet.AddRange(from c in WeaponStyleSettings.EnumerateAttackClips()
				where c != null
				select c);
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
		return handle.AttackingLimb switch
		{
			AttackingLimb.MainHand => weaponStyleAttackData.GetMainHandAttackData(handle.AttackWeaponType, handle.AttackType), 
			AttackingLimb.OffHand => weaponStyleAttackData.GetOffHandAttackData(handle.AttackWeaponType, handle.AttackType), 
			AttackingLimb.Mechadendrite => weaponStyleAttackData.GetMechadendriteAttackData(handle.AttackWeaponType, handle.AttackType), 
			_ => throw new Exception($"Unknown AttackingLimb type: {handle.AttackingLimb}"), 
		};
	}

	private bool CheckCanAimWithIK(UnitAnimationActionHandle handle)
	{
		if (handle.AttackWeaponType.IsRanged())
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
			StringBuilder stringBuilder = new StringBuilder($"No attack animation for {handle.WeaponStyle}/{handle.AttackingLimb}/");
			stringBuilder.Append((handle.AttackingLimb == AttackingLimb.Mechadendrite) ? ((object)handle.AttackWeaponType.GetMechadendriteAttackAnimationType()) : ((object)handle.AttackWeaponType));
			stringBuilder.Append($"/{handle.AttackType}");
			PFLog.Animations.Error(handle.Manager.View, stringBuilder.ToString());
			data.Invalid = true;
			return;
		}
		if (handle.IsBurst)
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
		if (handle.IsBurst)
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
		data.SetupIKAimingData(handle.Manager, handle.AttackingLimb);
		if (!Next(handle))
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
			flag = unitAnimationManager != null && unitAnimationManager.BlockAttackAnimation;
		}
		if (handle.Manager.BlockAttackAnimation || flag)
		{
			handle.SpeedScale = 0f;
			return;
		}
		Data data = GetData(handle);
		handle.SpeedScale = data.CurrentAnimationSpeedScale * Game.CombatAnimSpeedUp;
		float time = handle.GetTime();
		data.UpdateIKAiming(time, handle.ActEventsCounter);
		if (!(time < GetNextClipStartTime(handle)) && !Next(handle))
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
		handle.Action.TransitionIn = 0f;
		handle.StartClip(clipData.Clip, ClipDurationType.Oneshot);
		if (data.Index != data.Sequence.Count - 1)
		{
			handle.ActiveAnimation.ChangeTransitionTime(0f);
		}
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
			num += data.Sequence[i].Clip.Length;
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
		float num = 0.3f;
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
