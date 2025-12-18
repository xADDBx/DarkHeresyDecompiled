using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.WeaponStyles;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "WarhammerUnitAnimationActionHandEquipHuman", menuName = "Animation Manager/Actions/Warhammer Unit Hand Equip|Unequip Human")]
public class WarhammerUnitAnimationActionHandEquipHuman : UnitAnimationAction, IUnitAnimationActionHandEquip
{
	private enum ActionType
	{
		Equip,
		Unequip
	}

	private class ActionData
	{
		public AnimationClipWrapper clip;

		public bool wasMoving;
	}

	[SerializeField]
	private ActionType m_Type;

	[SerializeField]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public bool IsEquip => m_Type == ActionType.Equip;

	[CanBeNull]
	private BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings?.Get();

	public override UnitAnimationType Type
	{
		get
		{
			if (!IsEquip)
			{
				return UnitAnimationType.Unequip;
			}
			return UnitAnimationType.Equip;
		}
	}

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
			m_ClipWrappersHashSet.AddRange(WeaponStyleSettings.EnumerateEquipClips());
		}
		foreach (WeaponStyleEquipData item in WeaponStyleSettings.WeaponStyles.Select((BlueprintWeaponStyleList.WeaponStyleEntry x) => x.AnimationSet.Equip))
		{
			foreach (WeaponStyleEquipData.EquipmentSlotSettings slotsSetting in item.SlotsSettings)
			{
				m_ClipWrappersHashSet.Add(slotsSetting.EquipIn);
				m_ClipWrappersHashSet.Add(slotsSetting.EquipInCombat);
				m_ClipWrappersHashSet.Add(slotsSetting.EquipOut);
			}
		}
		return m_ClipWrappersHashSet;
	}

	public void SetType(bool isEquip)
	{
		m_Type = ((!isEquip) ? ActionType.Unequip : ActionType.Equip);
	}

	public float GetDuration(UnitAnimationActionHandle handle)
	{
		return GetClip(handle)?.AnimationClip.length ?? 0.01f;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper clip = GetClip(handle);
		if ((bool)clip)
		{
			ActionData actionData = new ActionData
			{
				clip = clip,
				wasMoving = (handle.Manager.NewSpeed > 0f)
			};
			AvatarMask avatarMask = ((actionData.wasMoving && AvatarMasks.Count > 0) ? AvatarMasks[0] : null);
			handle.StartClip(clip, avatarMask, ClipDurationType.Oneshot);
			handle.ActiveAnimation.DoNotZeroOtherAnimations = false;
			handle.ActionData = actionData;
		}
		else
		{
			handle.Release();
		}
		handle.Manager.CurrentEquipHandle = handle;
		handle.Manager.PreviousInCombat = handle.Manager.IsInCombat;
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		base.OnUpdate(handle, deltaTime);
		ActionData actionData = handle.ActionData as ActionData;
		bool flag = handle.Manager.NewSpeed > 0f;
		if (actionData.wasMoving != flag && AvatarMasks.Count > 0)
		{
			actionData.wasMoving = flag;
			float time = handle.GetTime();
			float speedScale = handle.SpeedScale;
			AvatarMask avatarMask = (flag ? AvatarMasks[0] : null);
			handle.StartClip(actionData.clip, avatarMask, ClipDurationType.Oneshot);
			handle.ActiveAnimation.SetTime(time);
			handle.SpeedScale = speedScale;
		}
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		base.OnFinish(handle);
		handle.Manager.CurrentEquipHandle = null;
	}

	[CanBeNull]
	private AnimationClipWrapper GetClip(UnitAnimationActionHandle handle)
	{
		if (!IsEquip)
		{
			return GetUnequipClip(handle);
		}
		return GetEquipClip(handle);
	}

	[CanBeNull]
	private AnimationClipWrapper GetEquipClip(UnitAnimationActionHandle handle)
	{
		WeaponStyleEquipData weaponStyleEquipData = WeaponStyleSettings?[handle.EquipActionWeaponStyle]?.Equip;
		if (weaponStyleEquipData == null)
		{
			PFLog.Animations.Error($"{base.name} can't find equip settings for weapon style: {handle.EquipActionWeaponStyle}");
			return null;
		}
		AnimationClipWrapper obj = ((!handle.Manager.PreviousInCombat) ? weaponStyleEquipData[handle.EquipmentSlot]?.EquipIn : weaponStyleEquipData[handle.EquipmentSlot]?.EquipInCombat);
		if (!obj)
		{
			PFLog.Animations.Error($"{base.name} can't find equip clip for weapon style: {handle.EquipActionWeaponStyle} slot: {handle.EquipmentSlot}");
		}
		return obj;
	}

	[CanBeNull]
	private AnimationClipWrapper GetUnequipClip(UnitAnimationActionHandle handle)
	{
		WeaponStyleEquipData weaponStyleEquipData = WeaponStyleSettings?[handle.EquipActionWeaponStyle]?.Equip;
		if (weaponStyleEquipData == null)
		{
			PFLog.Animations.Error($"{base.name} can't find unequip settings for weapon style: {handle.EquipActionWeaponStyle}");
			return null;
		}
		AnimationClipWrapper obj = weaponStyleEquipData[handle.EquipmentSlot]?.EquipOut;
		if (!obj)
		{
			PFLog.Animations.Error($"{base.name} can't find unequip clip for weapon style: {handle.EquipActionWeaponStyle} slot: {handle.EquipmentSlot}");
		}
		return obj;
	}
}
