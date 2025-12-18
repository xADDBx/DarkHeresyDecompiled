using System;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

namespace Kingmaker.View.Equipment;

public class HandEquipmentHelper : CustomYieldInstruction, IDisposable
{
	private readonly UnitAnimationActionHandle m_MainHandHandle;

	private readonly UnitAnimationActionHandle m_OffHandHandle;

	private Action m_Callback;

	private bool m_IsDisposed;

	public float Duration => (((IUnitAnimationActionHandEquip)m_MainHandHandle.Action).GetDuration(m_MainHandHandle) + ((IUnitAnimationActionHandEquip)(m_OffHandHandle?.Action))?.GetDuration(m_OffHandHandle)).GetValueOrDefault();

	public override bool keepWaiting
	{
		get
		{
			try
			{
				return Next();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
				return false;
			}
		}
	}

	public void Scale(float scale)
	{
		m_MainHandHandle.SpeedScale = scale;
		if (m_OffHandHandle != null)
		{
			m_OffHandHandle.SpeedScale = scale;
		}
	}

	public static HandEquipmentHelper StartEquipTwoHanded(UnitAnimationManager manager, UnitEquipmentAnimationSlotType slot, WeaponAnimationStyle style, Action callback)
	{
		return new HandEquipmentHelper(manager, equip: true, style, slot, UnitEquipmentAnimationSlotType.None, callback);
	}

	public static HandEquipmentHelper StartEquipDualWielding(UnitAnimationManager manager, UnitEquipmentAnimationSlotType mainSlot, UnitEquipmentAnimationSlotType offSlot, WeaponAnimationStyle style, Action callback)
	{
		return new HandEquipmentHelper(manager, equip: true, style, mainSlot, offSlot, callback);
	}

	public static HandEquipmentHelper StartUnequipTwoHanded(UnitAnimationManager manager, UnitEquipmentAnimationSlotType mainSlot, WeaponAnimationStyle style, Action callback)
	{
		return new HandEquipmentHelper(manager, equip: false, style, mainSlot, UnitEquipmentAnimationSlotType.None, callback);
	}

	public static HandEquipmentHelper StartUnequipDualWielding(UnitAnimationManager manager, UnitEquipmentAnimationSlotType mainSlot, UnitEquipmentAnimationSlotType offSlot, WeaponAnimationStyle style, Action callback)
	{
		return new HandEquipmentHelper(manager, equip: false, style, mainSlot, offSlot, callback);
	}

	private HandEquipmentHelper(UnitAnimationManager manager, bool equip, WeaponAnimationStyle style, UnitEquipmentAnimationSlotType mainSlot, UnitEquipmentAnimationSlotType offSlot, Action callback)
	{
		m_MainHandHandle = manager.CreateHandle(equip ? UnitAnimationType.Equip : UnitAnimationType.Unequip);
		m_MainHandHandle.EquipmentSlot = mainSlot;
		m_MainHandHandle.EquipActionWeaponStyle = style;
		manager.Execute(m_MainHandHandle);
		m_Callback = callback;
	}

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_MainHandHandle.Release();
			m_OffHandHandle?.Release();
			m_IsDisposed = true;
		}
	}

	public bool Next()
	{
		if (m_IsDisposed)
		{
			PFLog.Default.Error("Do not use disposed HandEquipmentHelper");
			return false;
		}
		if (m_Callback != null && (m_MainHandHandle.IsActed || m_MainHandHandle.IsReleased) && (m_OffHandHandle == null || m_OffHandHandle.IsActed || m_OffHandHandle.IsReleased))
		{
			m_Callback();
			m_Callback = null;
		}
		if (m_MainHandHandle.IsReleased)
		{
			UnitAnimationActionHandle offHandHandle = m_OffHandHandle;
			if (offHandHandle != null)
			{
				return !offHandHandle.IsReleased;
			}
			return false;
		}
		return true;
	}
}
