using System;
using Kingmaker.Sound;
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
	private float m_BurstAnimationDelay;

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
	private WeaponSoundSizeType m_SoundSize;

	[SerializeField]
	private WeaponSoundType m_SoundType;

	[SerializeField]
	private AkSwitchReference m_SoundSizeSwitch;

	[SerializeField]
	private AkSwitchReference m_SoundTypeSwitch;

	[SerializeField]
	private AkSwitchReference m_MuffledTypeSwitch;

	[SerializeField]
	private WeaponMissSoundType m_MissSoundType;

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

	public WeaponVisualParameters Prototype { get; set; }

	public float BurstAnimationDelay => m_BurstAnimationDelay;

	public WeaponType WeaponType => m_WeaponType;

	public GameObject Model
	{
		get
		{
			if (Prototype == null || !(m_WeaponModel == null))
			{
				return m_WeaponModel;
			}
			return Prototype.Model;
		}
	}

	public GameObject BeltModel
	{
		get
		{
			GameObject gameObject = ((Prototype != null && m_WeaponBeltModelOverride == null) ? Prototype.BeltModel : m_WeaponBeltModelOverride);
			if (!m_CachedEquipLinksUpToDate)
			{
				m_CachedEquipLinks = ObjectExtensions.Or(ObjectExtensions.Or(Model, null)?.GetComponent<WeaponEquipLinks>(), null);
				m_CachedEquipLinksUpToDate = true;
			}
			return m_CachedEquipLinks?.BeltModel ?? gameObject;
		}
	}

	public GameObject SheathModel
	{
		get
		{
			GameObject gameObject = ((Prototype != null && m_WeaponSheathModelOverride == null) ? Prototype.SheathModel : m_WeaponSheathModelOverride);
			if (!m_CachedEquipLinksUpToDate)
			{
				m_CachedEquipLinks = ObjectExtensions.Or(ObjectExtensions.Or(Model, null)?.GetComponent<WeaponEquipLinks>(), null);
				m_CachedEquipLinksUpToDate = true;
			}
			return m_CachedEquipLinks?.SheathModel ?? gameObject;
		}
	}

	public WeaponSoundSizeType SoundSize
	{
		get
		{
			if (Prototype == null || m_SoundSize != 0)
			{
				return m_SoundSize;
			}
			return Prototype.SoundSize;
		}
	}

	public WeaponSoundType SoundType
	{
		get
		{
			if (Prototype == null || m_SoundType != 0)
			{
				return m_SoundType;
			}
			return Prototype.SoundType;
		}
	}

	public AkSwitchReference SoundSizeSwitch
	{
		get
		{
			if (Prototype == null || !(m_SoundSizeSwitch.Value == ""))
			{
				return m_SoundSizeSwitch;
			}
			return Prototype.SoundSizeSwitch;
		}
	}

	public AkSwitchReference SoundTypeSwitch
	{
		get
		{
			if (Prototype == null || !(m_SoundTypeSwitch.Value == ""))
			{
				return m_SoundTypeSwitch;
			}
			return Prototype.SoundTypeSwitch;
		}
	}

	public AkSwitchReference MuffledTypeSwitch
	{
		get
		{
			if (Prototype == null || !(m_MuffledTypeSwitch.Value == ""))
			{
				return m_MuffledTypeSwitch;
			}
			return Prototype.MuffledTypeSwitch;
		}
	}

	public WeaponMissSoundType MissSoundType
	{
		get
		{
			if (Prototype == null || m_MissSoundType != 0)
			{
				return m_MissSoundType;
			}
			return Prototype.MissSoundType;
		}
	}

	public string EquipSound
	{
		get
		{
			if (Prototype == null || !string.IsNullOrEmpty(m_EquipSound))
			{
				return m_EquipSound;
			}
			return Prototype.EquipSound;
		}
	}

	public string UnequipSound
	{
		get
		{
			if (Prototype == null || !string.IsNullOrEmpty(m_UnequipSound))
			{
				return m_UnequipSound;
			}
			return Prototype.UnequipSound;
		}
	}

	public string InventoryEquipSound
	{
		get
		{
			if (Prototype == null || !string.IsNullOrEmpty(m_InventoryEquipSound))
			{
				return m_InventoryEquipSound;
			}
			return Prototype.InventoryEquipSound;
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
			if (Prototype == null || !string.IsNullOrEmpty(m_InventoryPutSound))
			{
				return m_InventoryPutSound;
			}
			return Prototype.InventoryPutSound;
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
			if (Prototype == null || !string.IsNullOrEmpty(m_InventoryTakeSound))
			{
				return m_InventoryTakeSound;
			}
			return Prototype.InventoryTakeSound;
		}
		set
		{
			m_InventoryTakeSound = value;
		}
	}

	public bool IsBow => false;

	public bool IsTorch => false;

	public bool HasQuiver => false;

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
			case WeaponType.EldarHeavyHip:
			case WeaponType.EldarHeavyShoulder:
				return s_BackSlots;
			default:
				return s_AllSlots;
			}
		}
	}
}
