using System.Collections.Generic;
using Kingmaker.Blueprints.Items;

namespace Kingmaker.Gameplay.Features.Items.Utility;

public interface IBlueprintItemContainer
{
	BlueprintItem DefaultConcreteItem { get; }

	CRToPowerLevelEntry[] CRToPowerLevelOverride { get; }

	IReadOnlyList<PowerLevelToItemEntry> PowerLevelToItemOverride { get; }

	ItemFaction OverrideFaction { get; }
}
