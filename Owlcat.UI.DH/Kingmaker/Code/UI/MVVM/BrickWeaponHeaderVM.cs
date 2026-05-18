using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickWeaponHeaderVM : TooltipBrickVM
{
	public readonly string MainTitle;

	public readonly Sprite Image;

	public readonly StatData DamageValue;

	public readonly string ItemType;

	public readonly string ItemLabel;

	public readonly string ItemSubtitle;

	public readonly bool HasUpgrade;

	public readonly List<WeaponTagUISettings> TagSettings;

	public readonly BlueprintItemWeapon BlueprintItem;

	[CanBeNull]
	public readonly ItemEntityWeapon Item;

	public BrickWeaponHeaderVM(string mainTitle, Sprite image, StatData damageValue, bool hasUpgrade, List<WeaponTagUISettings> tagSettings, string itemType, string itemLabel, string itemSubtitle, BlueprintItemWeapon blueprintItem = null, [CanBeNull] ItemEntityWeapon item = null)
	{
		MainTitle = mainTitle;
		Image = image;
		DamageValue = damageValue;
		HasUpgrade = hasUpgrade;
		TagSettings = tagSettings ?? new List<WeaponTagUISettings>();
		ItemType = itemType;
		ItemLabel = itemLabel;
		ItemSubtitle = itemSubtitle;
		BlueprintItem = blueprintItem;
		Item = item;
	}
}
