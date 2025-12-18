using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/ItemsEnough")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("4976252585b024c499c47cd56966ab2b")]
public class ItemsEnough : Condition
{
	[ValidateNotNull]
	public bool Money;

	[HideIf("Money")]
	[SerializeField]
	private BlueprintItemReference m_ItemToCheck;

	[ValidatePositiveNumber]
	public int Quantity;

	public BlueprintItem ItemToCheck => m_ItemToCheck?.Get();

	protected override string GetConditionCaption()
	{
		return $"Player has at least {Quantity} {ItemToCheck} in inventory";
	}

	protected override bool CheckCondition()
	{
		if (Quantity < 1)
		{
			throw new Exception($"{Quantity} must be positive");
		}
		if (Money || (bool)ItemToCheck.GetComponent<MoneyReplacement>())
		{
			return Game.Instance.Player.Money >= Quantity;
		}
		return GameHelper.GetPlayerCharacter().Inventory.Contains(ItemToCheck, Quantity);
	}
}
