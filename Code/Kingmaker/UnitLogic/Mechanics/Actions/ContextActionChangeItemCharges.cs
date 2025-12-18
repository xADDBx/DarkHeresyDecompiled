using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("6a102fe0ab870d34b8b5153dd90e8bea")]
public class ContextActionChangeItemCharges : ContextAction
{
	public enum ChangingType
	{
		Set,
		Add,
		Substract
	}

	[SerializeField]
	private ChangingType m_Type;

	[SerializeField]
	private int m_Value;

	[SerializeField]
	private BlueprintItemEquipmentReference m_Item;

	public BlueprintItemEquipment Item => m_Item?.Get();

	public override string GetCaption()
	{
		string result = "";
		string text = Item.name;
		switch (m_Type)
		{
		case ChangingType.Set:
			result = $"Set {text} charges equal to {m_Value}";
			break;
		case ChangingType.Add:
			result = $"Add {m_Value} charges to {text}";
			break;
		case ChangingType.Substract:
			result = $"Substract {m_Value} charges from {text}";
			break;
		}
		return result;
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = base.Context.ClickedTarget?.Entity;
		if (mechanicEntity == null)
		{
			Element.LogError(this, "Target is missing");
			return;
		}
		ItemEntity item = GetItem(mechanicEntity, (BlueprintItemEquipment)m_Item);
		if (item == null)
		{
			Element.LogError(this, "Target has no specified item");
			return;
		}
		switch (m_Type)
		{
		case ChangingType.Set:
			item.Charges = Math.Max(0, m_Value);
			break;
		case ChangingType.Add:
			item.Charges = Math.Max(0, item.Charges + m_Value);
			break;
		case ChangingType.Substract:
			item.Charges = Math.Max(0, item.Charges - m_Value);
			break;
		}
	}

	private ItemEntity GetItem(MechanicEntity entity, BlueprintItem bpItem)
	{
		if (entity is UnitEntity unitEntity)
		{
			foreach (ItemEntity item in unitEntity.Body.Items)
			{
				if (item.Blueprint == bpItem)
				{
					return item;
				}
			}
		}
		return null;
	}
}
