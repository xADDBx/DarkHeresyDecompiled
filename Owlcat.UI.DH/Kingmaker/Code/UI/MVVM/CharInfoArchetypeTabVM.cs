using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using R3;

namespace Kingmaker.Code.UI.MVVM;

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
		Dictionary<SimpleBlueprint, List<BlueprintFeature>> facts = new Dictionary<SimpleBlueprint, List<BlueprintFeature>>();
		(from a in Unit.CurrentValue.Facts.GetComponents<AddFeaturesToLevelUp>()
			where a.Group == FeatureGroup.Talent
			select (OwnerBlueprint: a.OwnerBlueprint, Features: a.Features)).ForEach(delegate((BlueprintScriptableObject OwnerBlueprint, ReferenceArrayProxy<BlueprintFeature> Features) a)
		{
			if (!facts.ContainsKey(a.OwnerBlueprint))
			{
				facts[a.OwnerBlueprint] = new List<BlueprintFeature>();
			}
			facts[a.OwnerBlueprint].AddRange(a.Features);
		});
		(from f in Unit.CurrentValue.Facts.GetComponents<AddFacts>()
			select (OwnerBlueprint: f.OwnerBlueprint, f.Facts.OfType<BlueprintFeature>())).ForEach(delegate((BlueprintScriptableObject OwnerBlueprint, IEnumerable<BlueprintFeature>) a)
		{
			if (!facts.ContainsKey(a.OwnerBlueprint))
			{
				facts[a.OwnerBlueprint] = new List<BlueprintFeature>();
			}
			facts[a.OwnerBlueprint].AddRange(a.Item2);
		});
		List<BlueprintFeature> source = (from b in Unit.CurrentValue.Facts.List.Select((EntityFact f) => f.Blueprint).OfType<BlueprintFeature>()
			where !b.HideInCharacterSheetAndLevelUp && !b.HideInUI
			select b).Distinct().ToList();
		List<BlueprintFeature> source2 = source.Where((BlueprintFeature f) => f.FeatureTypes.Contains(BlueprintFeature.FeatureType.Talent)).ToList();
		HashSet<BlueprintFeature> assignedTalents = new HashSet<BlueprintFeature>();
		foreach (KeyValuePair<SimpleBlueprint, List<BlueprintFeature>> fact in facts)
		{
			List<BlueprintFeature> list2 = source2.Where((BlueprintFeature t) => fact.Value.Contains(t) && !assignedTalents.Contains(t)).Distinct().ToList();
			if (list2.Count != 0)
			{
				assignedTalents.UnionWith(list2);
				string title = ((fact.Key is IUIDataProvider iUIDataProvider && !string.IsNullOrEmpty(iUIDataProvider.Name)) ? iUIDataProvider.Name : fact.Key.NameSafe());
				list.Add(new CharInfoTalentGroupVM(title, list2, m_SearchString, Unit.CurrentValue));
			}
		}
		List<BlueprintFeature> talents = source.Where((BlueprintFeature t) => !assignedTalents.Contains(t)).ToList();
		list.Add(new CharInfoTalentGroupVM(UIStrings.Instance.InventoryScreen.FilterTextOther, talents, m_SearchString, Unit.CurrentValue));
		TalentGroupList.Clear();
		TalentGroupList.AddRange(list);
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
