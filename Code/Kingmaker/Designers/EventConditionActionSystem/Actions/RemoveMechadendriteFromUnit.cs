using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[ComponentName("Actions/RemoveMechadendriteFromUnit")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("ce2f8c08b3f83cd49a3085b27a4c10bc")]
public class RemoveMechadendriteFromUnit : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("ItemToGive")]
	private BlueprintItemReference m_ItemToGive;

	[SerializeReference]
	public AbstractUnitEvaluator UnequipFrom;

	public BlueprintItem ItemToGive => m_ItemToGive?.Get();

	public override string GetDescription()
	{
		return "Удаляет игроку указанный мехадендрит.\n";
	}

	protected override void RunAction()
	{
		ItemEntity item = ItemToGive.CreateEntity();
		RemoveItem(item);
	}

	private void RemoveItem(ItemEntity item)
	{
		BaseUnitEntity baseUnitEntity = GameHelper.GetPlayerCharacter();
		if (UnequipFrom != null)
		{
			if (!(UnequipFrom.GetValue() is BaseUnitEntity baseUnitEntity2))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {UnequipFrom} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return;
			}
			baseUnitEntity = baseUnitEntity2;
		}
		if (item is ItemEntityMechadendrite && baseUnitEntity?.Body.EquipmentSlots.FirstOrDefault((ItemSlot s) => s.MaybeItem?.Blueprint == item.Blueprint) is EquipmentSlot<BlueprintItemMechadendrite> equipmentSlot)
		{
			baseUnitEntity?.Body.EquipmentSlots.Remove(equipmentSlot);
			baseUnitEntity?.Body.AllSlots.Remove(equipmentSlot);
			baseUnitEntity?.Body.Mechadendrites.Remove(equipmentSlot);
			if (equipmentSlot.HasItem)
			{
				equipmentSlot.RemoveItem();
			}
		}
	}

	public override string GetCaption()
	{
		return $"Remove Item ({ItemToGive}) to player";
	}
}
