using Kingmaker.Blueprints.Items.Weapons;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Equipment;

public abstract class BlueprintItemEquipmentHand : BlueprintItemEquipment
{
	[SerializeField]
	protected WeaponVisualParameters m_VisualParameters;

	public WeaponVisualParameters VisualParameters => m_VisualParameters;

	public override string InventoryEquipSound
	{
		get
		{
			return VisualParameters.InventoryEquipSound;
		}
		set
		{
			VisualParameters.InventoryEquipSound = value;
		}
	}

	public override string InventoryPutSound
	{
		get
		{
			return VisualParameters.InventoryPutSound;
		}
		set
		{
			VisualParameters.InventoryPutSound = value;
		}
	}

	public override string InventoryTakeSound
	{
		get
		{
			return VisualParameters.InventoryTakeSound;
		}
		set
		{
			VisualParameters.InventoryTakeSound = value;
		}
	}
}
