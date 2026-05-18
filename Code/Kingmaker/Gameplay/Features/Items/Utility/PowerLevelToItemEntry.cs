using Kingmaker.Blueprints.Items;

namespace Kingmaker.Gameplay.Features.Items.Utility;

public readonly struct PowerLevelToItemEntry
{
	public readonly ItemPowerLevel PowerLevel;

	public readonly BlueprintItem Item;

	public PowerLevelToItemEntry(ItemPowerLevel powerLevel, BlueprintItem item)
	{
		PowerLevel = powerLevel;
		Item = item;
	}
}
