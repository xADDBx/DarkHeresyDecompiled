using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Equipment;

public abstract class BlueprintItemEquipmentSimple : BlueprintItemEquipment
{
	[SerializeField]
	[AkEventReference]
	private string m_InventoryEquipSound;

	public override string InventoryEquipSound => m_InventoryEquipSound;
}
