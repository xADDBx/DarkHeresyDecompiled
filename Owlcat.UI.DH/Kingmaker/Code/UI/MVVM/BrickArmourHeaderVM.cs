using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Gameplay.Components.Features;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickArmourHeaderVM : TooltipBrickVM
{
	public readonly string MainTitle;

	public readonly Sprite Image;

	public readonly string ItemType;

	public readonly string ItemLabel;

	public readonly bool HasUpgrade;

	public readonly StatData ArmourDurability;

	public readonly StatData DamageReduction;

	public readonly List<ArmourTagUISettings> TagSettings;

	public readonly BlueprintItem BlueprintItem;

	public BrickArmourHeaderVM(string mainTitle, Sprite image, StatData armourDurability, StatData damageReduction, bool hasUpgrade, List<ArmourTagUISettings> tagSettings, BlueprintItem blueprintItem = null, string itemType = null, string itemLabel = null)
	{
		MainTitle = mainTitle;
		Image = image;
		HasUpgrade = hasUpgrade;
		TagSettings = tagSettings ?? new List<ArmourTagUISettings>();
		ItemType = itemType;
		ItemLabel = itemLabel;
		ArmourDurability = armourDurability;
		DamageReduction = damageReduction;
		BlueprintItem = blueprintItem;
	}
}
