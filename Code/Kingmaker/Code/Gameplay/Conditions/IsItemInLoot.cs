using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Conditions;

[TypeId("4fdcb828363248a5bdd36afce51762cb")]
public class IsItemInLoot : Condition
{
	[SerializeField]
	public BlueprintItemReference? TargetItem;

	[SerializeField]
	[ValidatePositiveNumber]
	public int Quantity = 1;

	[AllowedEntityType(typeof(MapObjectView))]
	public EntityReference? LootObject;

	protected override string GetConditionCaption()
	{
		string text = TargetItem?.Get()?.name;
		string text2 = LootObject?.EntityNameInEditor;
		return string.Format("{0} contains at least {1} ", string.IsNullOrEmpty(text2) ? "<no loot>" : text2, Quantity) + (string.IsNullOrEmpty(text) ? "<no item>" : text);
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
		return (LootObject?.FindData()?.ToEntity().GetOptional<InteractionLootPart>()?.Loot.Contains(blueprintItem, Quantity)).GetValueOrDefault();
	}
}
