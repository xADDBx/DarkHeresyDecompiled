using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Conditions;

[TypeId("2b6eff61d5e043f890492498ee8a3080")]
public class IsItemInInventory : Condition
{
	[SerializeField]
	public BlueprintItemReference? TargetItem;

	[SerializeField]
	[ValidatePositiveNumber]
	public int Quantity = 1;

	[SerializeReference]
	public AbstractUnitEvaluator? UnitEvaluator;

	protected override string GetConditionCaption()
	{
		string text = TargetItem?.Get()?.name;
		string text2 = UnitEvaluator?.GetCaption();
		return string.Format("{0} has at least {1} ", string.IsNullOrEmpty(text2) ? "<no unit>" : text2, Quantity) + (string.IsNullOrEmpty(text) ? "<no item>" : text) + " in inventory";
	}

	protected override bool CheckCondition()
	{
		if (Quantity < 1)
		{
			throw new Exception($"{Quantity} must be positive");
		}
		BlueprintItem blueprintItem = TargetItem?.Get();
		if (blueprintItem == null)
		{
			return false;
		}
		if (UnitEvaluator?.GetValue() is BaseUnitEntity baseUnitEntity)
		{
			return baseUnitEntity.Inventory.Contains(blueprintItem, Quantity);
		}
		return false;
	}
}
