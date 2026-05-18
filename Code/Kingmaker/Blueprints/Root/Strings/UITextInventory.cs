using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITextInventory
{
	[Header("Filters")]
	public LocalizedString FilterTextAll;

	public LocalizedString FilterTextWeapon;

	public LocalizedString FilterTextArmor;

	public LocalizedString FilterTextAcessories;

	public LocalizedString FilterTextUsable;

	public LocalizedString FilterTextNotable;

	public LocalizedString FilterTextOther;

	public LocalizedString FilterTextTrash;

	public LocalizedString FilterTextBuyBack;

	[Header("Another")]
	public LocalizedString OneHandWeapon;

	public LocalizedString TwoHandWeapon;

	public LocalizedString Buy;

	public LocalizedString Sell;

	public LocalizedString ShowUnavailableItems;

	[Header("Vendor filters")]
	public LocalizedString HideUnavailableItems;

	[Header("Console")]
	public LocalizedString ChooseItem;

	public LocalizedString ToggleStats;

	public LocalizedString ChangeWeaponSet;

	[Header("Misc")]
	public LocalizedString CantInsertThisWeapon;

	public LocalizedString StackPrice;

	public LocalizedString RulerHint;

	public LocalizedString InventorySharedStash;
}
