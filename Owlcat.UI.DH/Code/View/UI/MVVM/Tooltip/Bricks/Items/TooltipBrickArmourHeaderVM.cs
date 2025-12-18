using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Components.Features;
using Owlcat.UI;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class TooltipBrickArmourHeaderVM : TooltipBaseBrickVM
{
	public readonly string MainTitle;

	public readonly Sprite Image;

	public readonly string ItemType;

	public readonly string ItemLabel;

	public readonly bool HasUpgrade;

	public readonly StatData ArmourDurability;

	public readonly StatData DamageReduction;

	public readonly List<ArmourTagUISettings> TagSettings;

	public TooltipBrickArmourHeaderVM(string mainTitle, Sprite image, StatData armourDurability, StatData damageReduction, bool hasUpgrade, List<ArmourTagUISettings> tagSettings, string itemType, string itemLabel)
	{
		MainTitle = mainTitle;
		Image = image;
		HasUpgrade = hasUpgrade;
		TagSettings = tagSettings ?? new List<ArmourTagUISettings>();
		ItemType = itemType;
		ItemLabel = itemLabel;
		ArmourDurability = armourDurability;
		DamageReduction = damageReduction;
	}
}
