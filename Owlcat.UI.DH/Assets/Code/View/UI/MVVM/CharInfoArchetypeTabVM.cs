using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using ObservableCollections;
using R3;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoArchetypeTabVM : CharInfoComponentVM
{
	private readonly ReactiveProperty<CareerPathVM> m_FirstCareer = new ReactiveProperty<CareerPathVM>();

	private readonly ReactiveProperty<CareerPathVM> m_SecondCareer = new ReactiveProperty<CareerPathVM>();

	private readonly ReactiveProperty<CharInfoArchetypeFeatureVM> m_Homeworld = new ReactiveProperty<CharInfoArchetypeFeatureVM>();

	private readonly ReactiveProperty<CharInfoArchetypeFeatureVM> m_Background = new ReactiveProperty<CharInfoArchetypeFeatureVM>();

	private readonly ReactiveProperty<string> m_SearchString = new ReactiveProperty<string>(string.Empty);

	public ReadOnlyReactiveProperty<CareerPathVM> FirstCareer => m_FirstCareer;

	public ReadOnlyReactiveProperty<CareerPathVM> SecondCareer => m_SecondCareer;

	public ReadOnlyReactiveProperty<CharInfoArchetypeFeatureVM> Homeworld => m_Homeworld;

	public ReadOnlyReactiveProperty<CharInfoArchetypeFeatureVM> Background => m_Background;

	public ObservableList<CharInfoArchetypeFeatureVM> SpecializationList { get; } = new ObservableList<CharInfoArchetypeFeatureVM>();


	public ObservableList<CharInfoTalentGroupVM> TalentGroupList { get; } = new ObservableList<CharInfoTalentGroupVM>();


	public CharInfoArchetypeTabVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		SetupCareers();
		SetupSpecializaton();
		SetupTalents();
		SetupBackground();
		SetupHomeworld();
	}

	public void SearchChanged(string searchString)
	{
		m_SearchString.Value = searchString;
	}

	private void SetupHomeworld()
	{
		BlueprintFeature feature = Unit.CurrentValue.Facts.List.Where((EntityFact f) => f.Blueprint is BlueprintFeature)?.Select((EntityFact f) => f.Blueprint as BlueprintFeature).FirstOrDefault((BlueprintFeature f) => f.FeatureTypes.Contains(BlueprintFeature.FeatureType.Homeworld));
		m_Homeworld.Value = new CharInfoArchetypeFeatureVM(feature, null, Unit.CurrentValue);
	}

	private void SetupBackground()
	{
		BlueprintFeature feature = Unit.CurrentValue.Facts.List.Where((EntityFact f) => f.Blueprint is BlueprintFeature)?.Select((EntityFact f) => f.Blueprint as BlueprintFeature).FirstOrDefault((BlueprintFeature f) => f.FeatureTypes.Contains(BlueprintFeature.FeatureType.Background));
		m_Background.Value = new CharInfoArchetypeFeatureVM(feature, null, Unit.CurrentValue);
	}

	private void SetupTalents()
	{
		List<CharInfoTalentGroupVM> list = new List<CharInfoTalentGroupVM>();
		List<AddFeaturesToLevelUp> addFeatureTolevelUps = (from a in Unit.CurrentValue.Facts.GetComponents<AddFeaturesToLevelUp>()
			where a.Group == FeatureGroup.Talent
			select a).Distinct().ToList();
		IEnumerable<BlueprintFeature> source = from b in Unit.CurrentValue.Facts.List.Select((EntityFact f) => f.Blueprint).OfType<BlueprintFeature>()
			where !b.HideInCharacterSheetAndLevelUp && !b.HideInUI
			select b;
		List<BlueprintFeature> source2 = source.Where((BlueprintFeature f) => f.FeatureTypes.Contains(BlueprintFeature.FeatureType.Talent)).ToList();
		foreach (AddFeaturesToLevelUp addFeatureTolevelUp in addFeatureTolevelUps)
		{
			List<BlueprintFeature> list2 = source2.Where((BlueprintFeature t) => addFeatureTolevelUp.Features.Contains(t)).ToList();
			if (list2.Count != 0)
			{
				string title = ((addFeatureTolevelUp.OwnerBlueprint is IUIDataProvider iUIDataProvider && !string.IsNullOrEmpty(iUIDataProvider.Name)) ? iUIDataProvider.Name : addFeatureTolevelUp.OwnerBlueprint.NameSafe());
				list.Add(new CharInfoTalentGroupVM(title, list2, m_SearchString, Unit.CurrentValue));
			}
		}
		List<BlueprintFeature> list3 = source.Where((BlueprintFeature b) => b.FeatureTypes == null || b.FeatureTypes.Count == 0).ToList();
		list3.AddRange(source2.Where((BlueprintFeature t) => !addFeatureTolevelUps.Any((AddFeaturesToLevelUp f) => f.Features.Contains(t))));
		list.Add(new CharInfoTalentGroupVM(UIStrings.Instance.InventoryScreen.FilterTextOther, list3, m_SearchString, Unit.CurrentValue));
		TalentGroupList.Clear();
		TalentGroupList.AddRange(list.Distinct().ToList());
	}

	private void SetupSpecializaton()
	{
		IEnumerable<CharInfoArchetypeFeatureVM> items = from f in Unit.CurrentValue.Facts.List
			where f.Blueprint is BlueprintFeature
			select f.Blueprint as BlueprintFeature into f
			where f.FeatureTypes.Contains(BlueprintFeature.FeatureType.Specialization)
			select new CharInfoArchetypeFeatureVM(f, null, Unit.CurrentValue);
		SpecializationList.Clear();
		SpecializationList.AddRange(items);
	}

	private void SetupCareers()
	{
		List<CareerPathVM> list = new List<CareerPathVM>(2);
		(BlueprintCareerPath, int)[] array = Unit.CurrentValue.Progression.AllCareerPaths.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(new CareerPathVM(Unit.CurrentValue, array[i].Item1, null));
		}
		m_FirstCareer.Value = ((list.Count > 0) ? list[0] : null);
		m_SecondCareer.Value = ((list.Count > 1) ? list[1] : null);
	}
}
