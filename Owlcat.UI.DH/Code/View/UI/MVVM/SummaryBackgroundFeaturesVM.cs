using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Code.View.UI.MVVM;

public class SummaryBackgroundFeaturesVM : ViewModel
{
	public readonly ObservableList<BackgroundFeatureVM> Features = new ObservableList<BackgroundFeatureVM>();

	private readonly ReadOnlyReactiveProperty<BaseUnitEntity> m_Unit;

	private readonly ReadOnlyReactiveProperty<LevelUpManager> m_LevelUpManager;

	public SummaryBackgroundFeaturesVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager)
	{
		m_Unit = unit;
		m_LevelUpManager = levelUpManager;
		UpdateFeatures();
	}

	public void UpdateFeatures()
	{
		Features.Clear();
		Features.Add(GetBackgroundFeature(FeatureGroup.ChargenHomeworld));
		Features.Add(GetBackgroundFeature(FeatureGroup.ChargenOccupation));
		Features.Add(GetBackgroundFeature(FeatureGroup.ChargenCareerPath));
	}

	private BackgroundFeatureVM GetBackgroundFeature(FeatureGroup group)
	{
		BlueprintPath path = m_LevelUpManager.CurrentValue.Path;
		if (path == null)
		{
			return null;
		}
		if (UtilityChargen.GetFeatureSelectionsByGroup(path, group).FirstOrDefault() == null)
		{
			return null;
		}
		BlueprintFeature blueprintFeature = (m_LevelUpManager.CurrentValue.Selections.FirstOrDefault(delegate(SelectionState s)
		{
			BlueprintSelectionFeature obj = s.Blueprint as BlueprintSelectionFeature;
			return obj != null && obj.Group == group;
		}) as SelectionStateFeature)?.SelectionItem?.Feature;
		if (blueprintFeature == null)
		{
			return null;
		}
		HomeworldUISettings component;
		TooltipBaseTemplate tooltip = ((group != FeatureGroup.ChargenHomeworld) ? new TooltipTemplateFeature(blueprintFeature) : (blueprintFeature.TryGetComponent<HomeworldUISettings>(out component) ? ((TooltipBaseTemplate)new TooltipTemplateSimple(blueprintFeature.LocalizedName.Text, component.PlanetDescription.Text)) : ((TooltipBaseTemplate)new TooltipTemplateFeature(blueprintFeature))));
		UICharGen charGen = UIStrings.Instance.CharGen;
		string featureTypeName = group switch
		{
			FeatureGroup.ChargenHomeworld => charGen.Homeworld.Text, 
			FeatureGroup.ChargenOccupation => charGen.Occupation.Text, 
			FeatureGroup.ChargenCareerPath => charGen.Careers.Text, 
			_ => string.Empty, 
		};
		return new BackgroundFeatureVM(blueprintFeature.Icon, blueprintFeature.Name, featureTypeName, group, tooltip);
	}
}
