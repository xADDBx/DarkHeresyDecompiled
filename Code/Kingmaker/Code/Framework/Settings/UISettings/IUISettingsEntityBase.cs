using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Localization;

namespace Kingmaker.Code.Framework.Settings.UISettings;

public interface IUISettingsEntityBase
{
	LocalizedString Description { get; }

	LocalizedString TooltipDescription { get; }

	bool ShowVisualConnection { get; }

	bool IAmSetHandler { get; }

	bool HasCoopTooltipDescription { get; }

	bool NoDefaultReset { get; }

	List<BlueprintEncyclopediaPageReference> EncyclopediaDescription { get; }
}
