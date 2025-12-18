using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public interface IRankEntrySelectItem : IHasTooltipTemplates
{
	int EntryRank { get; }

	ReadOnlyReactiveProperty<bool> HasUnavailableFeatures { get; }

	FeatureGroup? GetFeatureGroup();

	bool CanSelect();

	void UpdateFeatures();

	void UpdateReadOnlyState();

	void ToggleShowUnavailableFeatures();

	bool ContainsFeature(string key);
}
