using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Components.Features;
using Owlcat.UI;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class TooltipBrickWeaponHeaderVM : TooltipBaseBrickVM
{
	public readonly string MainTitle;

	public readonly Sprite Image;

	public readonly StatData DamageValue;

	public readonly string ItemType;

	public readonly string ItemLabel;

	public readonly string ItemSubtitle;

	public readonly bool HasUpgrade;

	public readonly List<WeaponTagUISettings> TagSettings;

	public readonly Dictionary<WeaponTagProperty, int> SpecialTagsValues;

	public static List<WeaponTagProperty> SpecialTags = new List<WeaponTagProperty>
	{
		WeaponTagProperty.Brutal,
		WeaponTagProperty.Destructive
	};

	public TooltipBrickWeaponHeaderVM(string mainTitle, Sprite image, StatData damageValue, bool hasUpgrade, List<WeaponTagUISettings> tagSettings, string itemType, string itemLabel, string itemSubtitle, Dictionary<WeaponTagProperty, int> specialTagsValues)
	{
		MainTitle = mainTitle;
		Image = image;
		DamageValue = damageValue;
		HasUpgrade = hasUpgrade;
		TagSettings = tagSettings ?? new List<WeaponTagUISettings>();
		ItemType = itemType;
		ItemLabel = itemLabel;
		ItemSubtitle = itemSubtitle;
		SpecialTagsValues = specialTagsValues;
	}
}
