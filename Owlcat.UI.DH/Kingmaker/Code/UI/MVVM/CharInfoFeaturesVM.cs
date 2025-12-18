using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFeaturesVM : CharInfoComponentVM, IActionBarPartAbilitiesHandler, ISubscriber
{
	public readonly AutoDisposingList<CharInfoFeatureGroupVM> ActiveAbilities = new AutoDisposingList<CharInfoFeatureGroupVM>();

	public readonly AutoDisposingList<CharInfoFeatureGroupVM> PassiveAbilities = new AutoDisposingList<CharInfoFeatureGroupVM>();

	private readonly ReactiveProperty<bool> m_ChooseAbilityMode = new ReactiveProperty<bool>();

	public readonly ActionBarPartAbilitiesVM ActionBarPartAbilitiesVM;

	public int TargetSlotIndex = -1;

	private ReactiveProperty<BaseUnitEntity> m_Unit;

	public ReadOnlyReactiveProperty<bool> ChooseAbilityMode => m_ChooseAbilityMode;

	private static UITextCharSheet Strings => UIStrings.Instance.CharacterSheet;

	public CharInfoFeaturesVM(ReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		Disposable.Create(DisposeImplementation).AddTo(this);
		ActionBarPartAbilitiesVM = new ActionBarPartAbilitiesVM(isInCharScreen: true).AddTo(this);
		ActionBarPartAbilitiesVM.SetUnit(Unit.CurrentValue);
		m_Unit = unit;
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		AssembleActiveAbilities();
		AssemblePassiveAbilities();
		Unit.CurrentValue?.ActionBar.TryToInitialize();
		ActionBarPartAbilitiesVM?.SetUnit(Unit.CurrentValue);
	}

	public void SetChooseAbilityMode(bool chooseAbilityMode)
	{
		m_ChooseAbilityMode.Value = chooseAbilityMode;
	}

	private void AssembleActiveAbilities()
	{
		List<Ability> first = UIUtilityUnit.CollectAbilities(Unit.CurrentValue).ToList();
		(List<Ability>, string) abilitiesWithTier = GetAbilitiesWithTier(CareerPathTier.Three);
		(List<Ability>, string) abilitiesWithTier2 = GetAbilitiesWithTier(CareerPathTier.Two);
		(List<Ability>, string) abilitiesWithTier3 = GetAbilitiesWithTier(CareerPathTier.One);
		List<Ability> itemsAndSoulMarksAbilities = GetItemsAndSoulMarksAbilities();
		ActiveAbilities.Clear();
		ActiveAbilities.Add(ToFeatureGroup(abilitiesWithTier.Item1, abilitiesWithTier.Item2));
		ActiveAbilities.Add(ToFeatureGroup(abilitiesWithTier2.Item1, abilitiesWithTier2.Item2));
		ActiveAbilities.Add(ToFeatureGroup(abilitiesWithTier3.Item1, abilitiesWithTier3.Item2));
		List<Ability> abilities = first.Except(abilitiesWithTier.Item1).Except(abilitiesWithTier2.Item1).Except(abilitiesWithTier3.Item1)
			.Except(itemsAndSoulMarksAbilities)
			.ToList();
		ActiveAbilities.Add(ToFeatureGroup(abilities, UIStrings.Instance.CharacterSheet.BackgroundAbilities));
		ActiveAbilities.Add(ToFeatureGroup(itemsAndSoulMarksAbilities, Strings.ItemsAbilities));
	}

	private (List<Ability>, string) GetAbilitiesWithTier(CareerPathTier tier)
	{
		IEnumerable<Ability> source = UIUtilityUnit.CollectAbilities(Unit.CurrentValue).Where(HasTier);
		return new ValueTuple<List<Ability>, string>(item2: string.Format(arg0: Unit.CurrentValue.Facts.List.FirstOrDefault((EntityFact f) => f.Blueprint is BlueprintCareerPath blueprintCareerPath && blueprintCareerPath.Tier == tier)?.Name, format: Strings.CareerAbilities), item1: source.ToList());
		bool HasTier(Ability ability)
		{
			BlueprintCareerPath obj = ability.FirstSource?.Fact?.FirstSource?.Path as BlueprintCareerPath;
			if (obj == null)
			{
				return false;
			}
			return obj.Tier == tier;
		}
	}

	private CharInfoFeatureGroupVM ToFeatureGroup(List<Ability> abilities, string name)
	{
		return new CharInfoFeatureGroupVM(abilities.Select((Ability a) => new CharInfoFeatureVM(a, Unit.CurrentValue)).ToList(), name, CharInfoFeatureGroupVM.FeatureGroupType.Abilities, name);
	}

	private CharInfoFeatureGroupVM ToFeatureGroup(List<Feature> features, string name)
	{
		return new CharInfoFeatureGroupVM(features.Select((Feature f) => new CharInfoFeatureVM(f, Unit.CurrentValue)).ToList(), name, CharInfoFeatureGroupVM.FeatureGroupType.Talents, name);
	}

	private List<Ability> GetItemsAndSoulMarksAbilities()
	{
		IEnumerable<Ability> visible = Unit.CurrentValue.Abilities.Visible;
		List<Ability> list = (from a in visible
			where a.SourceItem != null
			where a.SourceItem != Unit.CurrentValue.GetFirstWeapon() && a.SourceItem != Unit.CurrentValue.GetSecondWeapon()
			select a).ToList();
		List<Ability> collection = visible.Where((Ability f) => f.FirstSource?.Blueprint is BlueprintAlignmentMark).ToList();
		list.AddRange(collection);
		return list;
	}

	private void AssemblePassiveAbilities()
	{
		List<Feature> first = UIUtilityUnit.CollectFeatures(Unit.CurrentValue).ToList();
		(List<Feature>, string) featuresWithTier = GetFeaturesWithTier(CareerPathTier.Three);
		(List<Feature>, string) featuresWithTier2 = GetFeaturesWithTier(CareerPathTier.Two);
		(List<Feature>, string) featuresWithTier3 = GetFeaturesWithTier(CareerPathTier.One);
		List<Feature> itemsAndSoulMarkFeatures = GetItemsAndSoulMarkFeatures();
		PassiveAbilities.Clear();
		PassiveAbilities.Add(ToFeatureGroup(featuresWithTier.Item1, featuresWithTier.Item2));
		PassiveAbilities.Add(ToFeatureGroup(featuresWithTier2.Item1, featuresWithTier2.Item2));
		PassiveAbilities.Add(ToFeatureGroup(featuresWithTier3.Item1, featuresWithTier3.Item2));
		List<Feature> features = first.Except(featuresWithTier.Item1).Except(featuresWithTier2.Item1).Except(featuresWithTier3.Item1)
			.Except(itemsAndSoulMarkFeatures)
			.ToList();
		PassiveAbilities.Add(ToFeatureGroup(features, Strings.BackgroundAbilities));
		PassiveAbilities.Add(ToFeatureGroup(itemsAndSoulMarkFeatures, Strings.SoulMarkAbilities));
	}

	private List<Feature> GetItemsAndSoulMarkFeatures()
	{
		IEnumerable<Feature> source = UIUtilityUnit.CollectFeatures(Unit.CurrentValue);
		List<Feature> list = source.Where((Feature f) => f.FirstSource?.Blueprint is BlueprintAlignmentMark).ToList();
		list.AddRange(source.Where((Feature f) => f.FirstSource?.Blueprint is BlueprintItem));
		return list;
	}

	private (List<Feature>, string) GetFeaturesWithTier(CareerPathTier tier)
	{
		IEnumerable<Feature> source = UIUtilityUnit.CollectFeatures(Unit.CurrentValue).Where(HasTier);
		return new ValueTuple<List<Feature>, string>(item2: string.Format(arg0: Unit.CurrentValue.Facts.List.FirstOrDefault((EntityFact f) => f.Blueprint is BlueprintCareerPath blueprintCareerPath && blueprintCareerPath.Tier == tier)?.Name, format: Strings.CareerAbilities), item1: source.ToList());
		bool HasTier(Feature feature)
		{
			BlueprintCareerPath obj = (feature.Blueprint as BlueprintCareerPath) ?? (feature.FirstSource?.Path as BlueprintCareerPath);
			if (obj == null)
			{
				return false;
			}
			return obj.Tier == tier;
		}
	}

	private void DisposeImplementation()
	{
		ActiveAbilities.Clear();
		PassiveAbilities.Clear();
	}

	public void MoveSlot(Ability sourceAbility, int targetIndex)
	{
	}

	public void MoveSlot(MechanicActionBarSlot sourceSlot, int sourceIndex, int targetIndex)
	{
	}

	public void SetSlot(MechanicEntityFact ability, int targetIndex)
	{
	}

	public void DeleteSlot(int sourceIndex)
	{
	}

	public void ChooseAbilityToSlot(int targetIndex)
	{
		m_ChooseAbilityMode.Value = true;
		TargetSlotIndex = targetIndex;
	}

	public void SetMoveAbilityMode(bool on)
	{
	}
}
