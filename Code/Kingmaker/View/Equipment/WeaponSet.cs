using System.Linq;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;

namespace Kingmaker.View.Equipment;

public class WeaponSet
{
	public readonly UnitViewHandSlotData MainHand;

	public readonly UnitViewHandSlotData OffHand;

	private WeaponAnimationStyle? m_WeaponStyleCached;

	private WeaponType? m_MainHandWeaponCached;

	private WeaponType? m_OffHandWeaponCached;

	public WeaponAnimationStyle WeaponStyle
	{
		get
		{
			bool num = MainHand.Owner.HasMechadendriteOfType(MechadendritesType.Ballistic);
			ItemEntityWeapon maybeWeapon = MainHand.Slot.MaybeWeapon;
			ItemEntityWeapon itemEntityWeapon = (num ? null : OffHand.Slot.MaybeWeapon);
			if (m_WeaponStyleCached.HasValue && m_MainHandWeaponCached == maybeWeapon?.WeaponType && m_OffHandWeaponCached == itemEntityWeapon?.WeaponType)
			{
				return m_WeaponStyleCached.Value;
			}
			m_MainHandWeaponCached = maybeWeapon?.WeaponType;
			m_OffHandWeaponCached = itemEntityWeapon?.WeaponType;
			if (maybeWeapon == null)
			{
				m_WeaponStyleCached = ((itemEntityWeapon == null) ? WeaponAnimationStyle.MeleeMelee : WeaponAnimationStyleHelper.DetectDualWieldingStyle(WeaponType.Fist, itemEntityWeapon.WeaponType));
				return m_WeaponStyleCached.Value;
			}
			m_WeaponStyleCached = (maybeWeapon.WeaponType.IsTwoHanded() ? WeaponAnimationStyleHelper.DetectTwoHandedWeaponStyle(m_MainHandWeaponCached.Value) : WeaponAnimationStyleHelper.DetectDualWieldingStyle(m_MainHandWeaponCached.Value, m_OffHandWeaponCached.GetValueOrDefault()));
			return m_WeaponStyleCached.Value;
		}
	}

	public WeaponSet(UnitViewHandSlotData mainHand, UnitViewHandSlotData offHand)
	{
		MainHand = mainHand;
		OffHand = offHand;
	}

	public HandEquipmentHelper Equip(UnitAnimationManager animationManager, UnitEquipmentVisualSlotType mainSlot = UnitEquipmentVisualSlotType.None, UnitEquipmentVisualSlotType offSlot = UnitEquipmentVisualSlotType.None)
	{
		BlueprintItemEquipmentHand weaponBlueprint = MainHand.Slot.MaybeItem?.Blueprint as BlueprintItemEquipmentHand;
		BlueprintItemEquipmentHand blueprintItemEquipmentHand = OffHand.Slot.MaybeItem?.Blueprint as BlueprintItemEquipmentHand;
		UnitEquipmentAnimationSlotType animationSlot = GetAnimationSlot(mainSlot, MainHand.EquipmentSlotType, weaponBlueprint, isOff: false);
		UnitEquipmentAnimationSlotType unitEquipmentAnimationSlotType = ((blueprintItemEquipmentHand != null) ? GetAnimationSlot(offSlot, OffHand.EquipmentSlotType, blueprintItemEquipmentHand, isOff: true) : UnitEquipmentAnimationSlotType.None);
		if (animationSlot == UnitEquipmentAnimationSlotType.None && unitEquipmentAnimationSlotType == UnitEquipmentAnimationSlotType.None)
		{
			OnEquipComplete();
			return null;
		}
		if (!WeaponStyle.IsTwoHanded())
		{
			return HandEquipmentHelper.StartEquipDualWielding(animationManager, animationSlot, WeaponStyle, OnEquipComplete);
		}
		return HandEquipmentHelper.StartEquipTwoHanded(animationManager, animationSlot, WeaponStyle, OnEquipComplete);
	}

	public HandEquipmentHelper Unequip(UnitAnimationManager animationManager)
	{
		if (MainHand.VisibleItem == null && OffHand.VisibleItem == null)
		{
			return null;
		}
		if (!MainHand.IsInHand && !OffHand.IsInHand)
		{
			return null;
		}
		if (MainHand.EquipmentSlotType == UnitEquipmentAnimationSlotType.None && OffHand.EquipmentSlotType == UnitEquipmentAnimationSlotType.None)
		{
			HideWeaponModels();
			return null;
		}
		if (!WeaponStyle.IsTwoHanded())
		{
			return HandEquipmentHelper.StartUnequipDualWielding(animationManager, MainHand.EquipmentSlotType, WeaponStyle, HideWeaponModels);
		}
		return HandEquipmentHelper.StartUnequipTwoHanded(animationManager, MainHand.EquipmentSlotType, WeaponStyle, HideWeaponModels);
	}

	private static UnitEquipmentAnimationSlotType GetAnimationSlot(UnitEquipmentVisualSlotType visualSlot, UnitEquipmentAnimationSlotType defaultSlot, BlueprintItemEquipmentHand weaponBlueprint, bool isOff)
	{
		UnitEquipmentAnimationSlotType unitEquipmentAnimationSlotType = ((visualSlot == UnitEquipmentVisualSlotType.None && defaultSlot != 0) ? defaultSlot : visualSlot.GetAnimSlot());
		if (unitEquipmentAnimationSlotType == UnitEquipmentAnimationSlotType.None && weaponBlueprint != null)
		{
			UnitEquipmentVisualSlotType slot = weaponBlueprint.VisualParameters.AttachSlots.FirstOrDefault();
			unitEquipmentAnimationSlotType = slot.GetAnimSlot();
			if (isOff && slot.IsLeft())
			{
				unitEquipmentAnimationSlotType = weaponBlueprint.VisualParameters.AttachSlots.FirstOrDefault((UnitEquipmentVisualSlotType s) => !s.IsLeft()).GetAnimSlot();
			}
		}
		return unitEquipmentAnimationSlotType;
	}

	private void OnEquipComplete()
	{
		MainHand.AttachModel(toHand: true);
		if (OffHand.Slot.MaybeItem?.Blueprint is BlueprintItemEquipmentHand)
		{
			OffHand.AttachModel(toHand: true);
		}
		PFLog.Animations.Log(MainHand.Owner.View.AsUnitEntityView(), $"{MainHand.Owner.View.gameObject.name} current weapon style is {WeaponStyle}");
	}

	public void HideWeaponModels()
	{
		MainHand.AttachModel(toHand: false);
		OffHand.AttachModel(toHand: false);
	}
}
