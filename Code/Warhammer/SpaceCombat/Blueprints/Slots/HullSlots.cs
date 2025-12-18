using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Warhammer.SpaceCombat.Blueprints.Slots;

[Serializable]
[Obsolete]
public class HullSlots
{
	[Header("Main Slots")]
	[SerializeField]
	[FormerlySerializedAs("PlasmaDrives")]
	private BlueprintItemPlasmaDrives.Reference m_PlasmaDrives;

	[SerializeField]
	private BlueprintItemVoidShieldGenerator.Reference m_VoidShieldGenerator;

	[SerializeField]
	private BlueprintItemWarpDrives.Reference m_WarpDrives;

	[SerializeField]
	private BlueprintItemGellerFieldDevice.Reference m_GellerFieldDevice;

	[SerializeField]
	private BlueprintItemLifeSustainer.Reference m_LifeSustainer;

	[SerializeField]
	private BlueprintItemBridge.Reference m_Bridge;

	[SerializeField]
	private BlueprintItemAugerArray.Reference m_AugerArray;

	[SerializeField]
	private BlueprintItemArmorPlating.Reference m_ArmorPlating;

	[SerializeField]
	private BlueprintItemArsenal.Reference[] m_Arsenals;

	public List<WeaponSlotData> Weapons;

	public BlueprintItemPlasmaDrives PlasmaDrives
	{
		get
		{
			return m_PlasmaDrives?.Get();
		}
		set
		{
			m_PlasmaDrives = value.ToReference<BlueprintItemPlasmaDrives.Reference>();
		}
	}

	public BlueprintItemVoidShieldGenerator VoidShieldGenerator
	{
		get
		{
			return m_VoidShieldGenerator?.Get();
		}
		set
		{
			m_VoidShieldGenerator = value.ToReference<BlueprintItemVoidShieldGenerator.Reference>();
		}
	}

	public BlueprintItemWarpDrives WarpDrives
	{
		get
		{
			return m_WarpDrives?.Get();
		}
		set
		{
			m_WarpDrives = value.ToReference<BlueprintItemWarpDrives.Reference>();
		}
	}

	public BlueprintItemGellerFieldDevice GellerFieldDevice
	{
		get
		{
			return m_GellerFieldDevice?.Get();
		}
		set
		{
			m_GellerFieldDevice = value.ToReference<BlueprintItemGellerFieldDevice.Reference>();
		}
	}

	public BlueprintItemLifeSustainer LifeSustainer
	{
		get
		{
			return m_LifeSustainer?.Get();
		}
		set
		{
			m_LifeSustainer = value.ToReference<BlueprintItemLifeSustainer.Reference>();
		}
	}

	public BlueprintItemBridge Bridge
	{
		get
		{
			return m_Bridge?.Get();
		}
		set
		{
			m_Bridge = value.ToReference<BlueprintItemBridge.Reference>();
		}
	}

	public BlueprintItemAugerArray AugerArray
	{
		get
		{
			return m_AugerArray?.Get();
		}
		set
		{
			m_AugerArray = value.ToReference<BlueprintItemAugerArray.Reference>();
		}
	}

	public BlueprintItemArmorPlating ArmorPlating
	{
		get
		{
			return m_ArmorPlating?.Get();
		}
		set
		{
			m_ArmorPlating = value.ToReference<BlueprintItemArmorPlating.Reference>();
		}
	}

	public ReferenceArrayProxy<BlueprintItemArsenal> Arsenals
	{
		get
		{
			BlueprintReference<BlueprintItemArsenal>[] arsenals = m_Arsenals;
			return arsenals;
		}
	}
}
