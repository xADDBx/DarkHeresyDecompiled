using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleEquipData : IWeaponStyleAnimationClipsProvider
{
	public enum EquipAnimationType
	{
		Back,
		Waist
	}

	[Serializable]
	public class EquipmentSlotSettings
	{
		public EquipAnimationType EquipAnimation;

		[Header("Equip")]
		[AssetPicker("")]
		[ValidateNotNull]
		[ValidateHasActEvent]
		[DrawEventWarning]
		[Tooltip("In combat full clip")]
		public AnimationClipWrapper EquipIn;

		[Tooltip("Idle to combat")]
		public AnimationClipWrapper EquipInCombat;

		[Header("Unequip")]
		[Tooltip("Combat to idle")]
		public AnimationClipWrapper EquipOut;
	}

	[ProvideNameWithProperty("EquipAnimation")]
	public List<EquipmentSlotSettings> SlotsSettings;

	public EquipmentSlotSettings this[UnitEquipmentAnimationSlotType slot] => SlotsSettings.SingleOrDefault((EquipmentSlotSettings x) => x.EquipAnimation == SlotToEquipAnimationType(slot));

	private static EquipAnimationType SlotToEquipAnimationType(UnitEquipmentAnimationSlotType slot)
	{
		if (slot != UnitEquipmentAnimationSlotType.FrontRight && slot != UnitEquipmentAnimationSlotType.FrontLeft)
		{
			return EquipAnimationType.Back;
		}
		return EquipAnimationType.Waist;
	}

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		foreach (EquipmentSlotSettings slotSettings in SlotsSettings)
		{
			yield return slotSettings.EquipIn;
			yield return slotSettings.EquipInCombat;
			yield return slotSettings.EquipOut;
		}
	}
}
