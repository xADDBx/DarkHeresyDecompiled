using Kingmaker.Blueprints.Attributes;
using Kingmaker.UI.Common;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Equipment;

[ComponentName("Items/BlueprintItemEquipmentUsable")]
[TypeId("fcf558235c4e3b747933f93af7617f7c")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemEquipmentUsable : BlueprintItemEquipment
{
	public bool RemoveFromSlotWhenNoCharges = true;

	[SerializeField]
	private int m_IdentifyDC;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryEquipSound;

	[SerializeField]
	[ValidateIsPrefab]
	private GameObject m_BeltItemPrefab;

	public override int IdentifyDC => m_IdentifyDC;

	public override ItemsItemType ItemType => ItemsItemType.Usable;

	public override string InventoryEquipSound => m_InventoryEquipSound;

	public GameObject BeltItemPrefab => m_BeltItemPrefab;
}
