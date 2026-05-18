using System;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Weapons;

[Serializable]
public class WeaponVisualParameters
{
	[SerializeField]
	private WeaponType m_WeaponType;

	[SerializeField]
	[ValidateIsPrefab]
	[ValidateHasComponent(typeof(EquipmentOffsets))]
	private GameObject m_WeaponModel;

	[SerializeField]
	[ValidateIsPrefab]
	[ValidateHasComponent(typeof(EquipmentOffsets))]
	private GameObject m_WeaponBeltModelOverride;

	[SerializeField]
	[ValidateIsPrefab]
	private GameObject m_WeaponSheathModelOverride;

	private WeaponEquipLinks m_CachedEquipLinks;

	private bool m_CachedEquipLinksUpToDate;

	[SerializeField]
	[AkEventReference]
	private string m_EquipSound;

	[SerializeField]
	[AkEventReference]
	private string m_UnequipSound;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryEquipSound;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryPutSound;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryTakeSound;

	private static readonly UnitEquipmentVisualSlotType[] s_BackSlots = new UnitEquipmentVisualSlotType[2]
	{
		UnitEquipmentVisualSlotType.LeftBack01,
		UnitEquipmentVisualSlotType.RightBack01
	};

	private static readonly UnitEquipmentVisualSlotType[] s_AllSlots = new UnitEquipmentVisualSlotType[4]
	{
		UnitEquipmentVisualSlotType.LeftFront01,
		UnitEquipmentVisualSlotType.RightFront01,
		UnitEquipmentVisualSlotType.LeftBack01,
		UnitEquipmentVisualSlotType.RightBack01
	};

	private static readonly UnitEquipmentVisualSlotType[] s_ShieldSlots = new UnitEquipmentVisualSlotType[1] { UnitEquipmentVisualSlotType.Shield };

	public WeaponType WeaponType => m_WeaponType;

	public GameObject Model => m_WeaponModel;

	public GameObject BeltModel
	{
		get
		{
			if (!m_CachedEquipLinksUpToDate)
			{
				m_CachedEquipLinks = ObjectExtensions.Or(ObjectExtensions.Or(Model, null)?.GetComponent<WeaponEquipLinks>(), null);
				m_CachedEquipLinksUpToDate = true;
			}
			if (!(m_CachedEquipLinks != null))
			{
				return m_WeaponBeltModelOverride;
			}
			return ObjectExtensions.Or(m_CachedEquipLinks.BeltModel, m_WeaponBeltModelOverride);
		}
	}

	public GameObject SheathModel
	{
		get
		{
			if (!m_CachedEquipLinksUpToDate)
			{
				m_CachedEquipLinks = ObjectExtensions.Or(ObjectExtensions.Or(Model, null)?.GetComponent<WeaponEquipLinks>(), null);
				m_CachedEquipLinksUpToDate = true;
			}
			if (!(m_CachedEquipLinks != null))
			{
				return m_WeaponSheathModelOverride;
			}
			return ObjectExtensions.Or(m_CachedEquipLinks.SheathModel, m_WeaponSheathModelOverride);
		}
	}

	public string EquipSound
	{
		get
		{
			return m_EquipSound;
		}
		set
		{
			m_EquipSound = value;
		}
	}

	public string UnequipSound
	{
		get
		{
			return m_UnequipSound;
		}
		set
		{
			m_UnequipSound = value;
		}
	}

	public string InventoryEquipSound
	{
		get
		{
			return m_InventoryEquipSound;
		}
		set
		{
			m_InventoryEquipSound = value;
		}
	}

	public string InventoryPutSound
	{
		get
		{
			return m_InventoryPutSound;
		}
		set
		{
			m_InventoryPutSound = value;
		}
	}

	public string InventoryTakeSound
	{
		get
		{
			return m_InventoryTakeSound;
		}
		set
		{
			m_InventoryTakeSound = value;
		}
	}

	public bool IsTwoHanded => WeaponType.IsTwoHanded();

	public UnitEquipmentVisualSlotType[] AttachSlots
	{
		get
		{
			switch (WeaponType)
			{
			case WeaponType.Shield:
				return s_ShieldSlots;
			case WeaponType.TwoHandedAxe:
			case WeaponType.TwoHandedHammer:
			case WeaponType.TwoHandedSword:
			case WeaponType.Staff:
			case WeaponType.RifleHipBase:
			case WeaponType.RifleHipFlamer:
			case WeaponType.RifleHipPlasma:
			case WeaponType.RifleShoulderBase:
			case WeaponType.RifleShoulderLaser:
			case WeaponType.HeavyHipBase:
			case WeaponType.HeavyHipFlamer:
			case WeaponType.HeavyHipLaser:
			case WeaponType.HeavyHipPlasma:
			case WeaponType.HeavyShoulder:
			case WeaponType.EldarRifleHipBase:
			case WeaponType.EldarRifleHipLaser:
			case WeaponType.EldarRifleShoulderBase:
			case WeaponType.EldarRifleShoulderLaser:
			case WeaponType.EldarHeavyHipBase:
			case WeaponType.EldarHeavyHipLaser:
			case WeaponType.EldarHeavyShoulder:
				return s_BackSlots;
			default:
				return s_AllSlots;
			}
		}
	}
}
