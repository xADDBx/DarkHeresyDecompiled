using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class RankEntrySelectionVM : VirtualListElementVMBase, IRankEntrySelectItem, IHasTooltipTemplates, IRankEntryFocusHandler, ISubscriber
{
	public static string SelectableHighlighterKey = "SelectableHighlighterKey";

	public readonly int Rank;

	public readonly FeatureGroup FeatureGroup;

	private readonly ReactiveProperty<RankEntryState> m_EntryState = new ReactiveProperty<RankEntryState>(RankEntryState.NotSelectable);

	private readonly ReactiveProperty<RankEntrySelectionFeatureVM> m_SelectedFeature = new ReactiveProperty<RankEntrySelectionFeatureVM>();

	private readonly ReactiveProperty<bool> m_IsCurrentRankEntryItem = new ReactiveProperty<bool>();

	public FeaturesFilterVM FeaturesFilterVM;

	public List<VirtualListElementVMBase> FilteredGroupList = new List<VirtualListElementVMBase>();

	private readonly ReactiveCommand<Unit> m_OnFilterChange = new ReactiveCommand<Unit>();

	private TooltipBaseTemplate m_HintTooltip;

	private TooltipBaseTemplate m_Tooltip;

	public readonly string GlossaryEntryKey;

	private readonly CareerPathVM m_CareerPathVM;

	private readonly BlueprintSelectionFeature m_SelectionFeature;

	private readonly Action<IRankEntrySelectItem> m_SelectAction;

	private List<RankEntryFeatureGroupVM> m_ShowGroupList;

	private readonly ReactiveProperty<SelectionStateFeature> m_SelectionStateFeature = new ReactiveProperty<SelectionStateFeature>();

	private readonly List<FeatureGroup> m_AscensionGroups = new List<FeatureGroup>
	{
		FeatureGroup.FirstCareerAbility,
		FeatureGroup.FirstCareerTalent,
		FeatureGroup.SecondCareerAbility,
		FeatureGroup.SecondCareerTalent,
		FeatureGroup.FirstOrSecondCareerAbility,
		FeatureGroup.FirstOrSecondCareerTalent
	};

	private readonly ReactiveProperty<bool> m_HasUnavailableFeatures = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<RankEntryState> EntryState => m_EntryState;

	public ReadOnlyReactiveProperty<RankEntrySelectionFeatureVM> SelectedFeature => m_SelectedFeature;

	public ReadOnlyReactiveProperty<bool> IsCurrentRankEntryItem => m_IsCurrentRankEntryItem;

	public Observable<Unit> OnFilterChange => m_OnFilterChange;

	public CharInfoFeatureVM UltimateFeature { get; }

	public TooltipBaseTemplate HintTooltip => m_HintTooltip ?? (m_HintTooltip = new TooltipTemplateGlossary(GlossaryEntryKey));

	public TooltipBaseTemplate Tooltip => m_Tooltip ?? (m_Tooltip = SelectedFeature.CurrentValue?.TooltipTemplate());

	public bool IsValidSelection
	{
		get
		{
			if (SelectedFeature.CurrentValue != null)
			{
				if (SelectedFeature.CurrentValue != null)
				{
					return EntryState.CurrentValue != RankEntryState.NotValid;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsFirstSelectable => m_CareerPathVM.FirstSelectable == this;

	public bool NeedToSelect
	{
		get
		{
			if (SelectedFeature.CurrentValue == null)
			{
				RankEntryState currentValue = EntryState.CurrentValue;
				return currentValue == RankEntryState.Selectable || currentValue == RankEntryState.FirstSelectable || currentValue == RankEntryState.WaitPreviousToSelect || currentValue == RankEntryState.Selected || currentValue == RankEntryState.NotValid;
			}
			return false;
		}
	}

	public bool SelectionMade => SelectedFeature.CurrentValue != null;

	public bool SelectionMadeAndValid
	{
		get
		{
			if (IsValidSelection)
			{
				return !NeedToSelect;
			}
			return false;
		}
	}

	public bool HasFeatures => m_ShowGroupList?.SelectMany((RankEntryFeatureGroupVM g) => g.FeatureList).Any() ?? false;

	public bool CanChangeSelection
	{
		get
		{
			RankEntryState currentValue = EntryState.CurrentValue;
			return currentValue == RankEntryState.Selectable || currentValue == RankEntryState.FirstSelectable || currentValue == RankEntryState.NotValid || currentValue == RankEntryState.WaitPreviousToSelect;
		}
	}

	public BaseUnitProgressionVM UnitProgressionVM => m_CareerPathVM.UnitProgressionVM;

	public CareerPathVM CareerPathVM => m_CareerPathVM;

	public int EntryRank => Rank;

	public ReadOnlyReactiveProperty<bool> HasUnavailableFeatures => m_HasUnavailableFeatures;

	public RankEntrySelectionVM(int rank, CareerPathVM careerPathVM, BlueprintSelectionFeature selectionFeature, Action<IRankEntrySelectItem> selectAction)
	{
		Rank = rank;
		FeatureGroup = selectionFeature.Group;
		m_CareerPathVM = careerPathVM;
		m_SelectAction = selectAction;
		m_SelectionFeature = selectionFeature;
		if (FeatureGroup == FeatureGroup.UltimateAbility && careerPathVM.CareerPathUIMetaData != null && careerPathVM.CareerPathUIMetaData.UltimateFeatures.NotNull().Any())
		{
			AddDisposable(UltimateFeature = new CharInfoFeatureVM(new UIFeature(careerPathVM.CareerPathUIMetaData.UltimateFeatures.FirstOrDefault()), careerPathVM.Unit));
		}
		if (RankEntrySelectionUtils.HasFilter(FeatureGroup))
		{
			FeaturesFilterVM = new FeaturesFilterVM();
			FeaturesFilterVM.CurrentFilter.Subscribe(delegate(FeatureFilterType value)
			{
				HandleFilterChange(value);
			});
		}
		else
		{
			UpdateFeatures();
		}
		GlossaryEntryKey = $"{FeatureGroup}_CareerPath_Selection";
		AddDisposable(UnitProgressionVM.CurrentRankEntryItem.Subscribe(delegate(IRankEntrySelectItem item)
		{
			m_IsCurrentRankEntryItem.Value = item == this;
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	private List<RankEntryFeatureGroupVM> CreateGroups()
	{
		List<RankEntryFeatureGroupVM> list = new List<RankEntryFeatureGroupVM>();
		foreach (var item3 in CreateSelectionItems(m_CareerPathVM))
		{
			BlueprintScriptableObject item = item3.Item1;
			List<RankEntrySelectionFeatureVM> item2 = item3.Item2;
			BlueprintFeature owner = (BlueprintFeature)item;
			List<BaseRankEntryFeatureVM> list2 = SortSelectionFeatures(item2);
			list2.ForEach(delegate(BaseRankEntryFeatureVM e)
			{
				e.SetHasFavorites(RankEntrySelectionUtils.HasFilter(FeatureGroup));
			});
			list.Add(new RankEntryFeatureGroupVM(list2, owner));
		}
		TryAddEmptyAscensionGroup(list);
		return list;
	}

	private static List<BaseRankEntryFeatureVM> SortSelectionFeatures(List<RankEntrySelectionFeatureVM> features)
	{
		return (from f in features
			orderby (!f.IsRecommended) ? 1 : (-1), f.DisplayName
			select f).Cast<BaseRankEntryFeatureVM>().ToList();
	}

	private List<(BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>)> CreateSelectionItems(CareerPathVM careerPathVM)
	{
		BaseUnitEntity baseUnitEntity = careerPathVM.Unit;
		if (careerPathVM.IsInLevelupProcess)
		{
			baseUnitEntity = UnitProgressionVM.LevelUpManager?.PreviewUnit ?? careerPathVM.Unit;
		}
		List<FeatureSelectionItem> list = m_SelectionFeature.GetSelectionItems(baseUnitEntity, careerPathVM.CareerPath).ToList();
		int entryId = m_CareerPathVM.RankEntriesScan.IndexOf(this);
		m_CareerPathVM.AddedOnLevelUpFeatures?.ExcludeUnavailableFeatures(FeatureGroup, entryId, list);
		List<BlueprintFact> source = (from f in baseUnitEntity.Facts.List
			where f.FirstSource?.Blueprint is BlueprintCareerPath blueprintCareerPath && blueprintCareerPath.Tier > careerPathVM.CareerPath.Tier
			select f.Blueprint).ToList();
		Dictionary<BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>> dictionary = new Dictionary<BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>>();
		List<BlueprintFeature> list2 = new List<BlueprintFeature>();
		RankEntrySelectionFeatureVM.UpdateUnitFacts(baseUnitEntity);
		List<RankEntrySelectionFeatureVM> value2;
		foreach (FeatureSelectionItem item4 in list)
		{
			if (!dictionary.TryGetValue(item4.SourceBlueprint, out var value))
			{
				value2 = (dictionary[item4.SourceBlueprint] = new List<RankEntrySelectionFeatureVM>());
				value = value2;
			}
			if (!list2.Contains(item4.Feature) && !source.Contains(item4.SourceBlueprint))
			{
				RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM = ((!(item4.Feature is BlueprintStatAdvancement)) ? new RankEntrySelectionFeatureVM(this, careerPathVM, item4, m_SelectionStateFeature, SelectFeature) : new RankEntrySelectionStatVM(this, careerPathVM, item4, m_SelectionStateFeature, SelectFeature));
				RankEntrySelectionFeatureVM item = rankEntrySelectionFeatureVM;
				list2.Add(item4.Feature);
				value.Add(item);
			}
		}
		List<(BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>)> list4 = new List<(BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>)>();
		foreach (KeyValuePair<BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>> item5 in dictionary)
		{
			item5.Deconstruct(out var key, out value2);
			BlueprintScriptableObject item2 = key;
			List<RankEntrySelectionFeatureVM> item3 = value2;
			list4.Add((item2, item3));
		}
		return list4.OrderBy(delegate((BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>) i)
		{
			if (!(i.Item1 is BlueprintFeature blueprintFeature))
			{
				return -1;
			}
			return blueprintFeature.HideInUI ? 1 : (-1);
		}).ToList();
	}

	protected override void DisposeImplementation()
	{
		m_ShowGroupList?.ForEach(delegate(RankEntryFeatureGroupVM vm)
		{
			vm.Dispose();
		});
		m_ShowGroupList?.Clear();
	}

	private void TryAddEmptyAscensionGroup(List<RankEntryFeatureGroupVM> rankEntryFeatureGroupVms)
	{
		if (rankEntryFeatureGroupVms.Empty() && m_AscensionGroups.Contains(FeatureGroup))
		{
			string featureGroupDescription = UIStrings.Instance.CharacterSheet.GetFeatureGroupDescription(FeatureGroup);
			rankEntryFeatureGroupVms.Add(new RankEntryEmptyFeaturesGroupVM(featureGroupDescription));
		}
		else if (rankEntryFeatureGroupVms.Count == 1)
		{
			switch (FeatureGroup)
			{
			case FeatureGroup.FirstOrSecondCareerAbility:
			{
				string description2 = UIStrings.Instance.CharacterSheet.AscensionMissingOnlySecondCareerAbilityFeatureGroupDescription;
				rankEntryFeatureGroupVms.Add(new RankEntryEmptyFeaturesGroupVM(description2));
				break;
			}
			case FeatureGroup.FirstOrSecondCareerTalent:
			{
				string description = UIStrings.Instance.CharacterSheet.AscensionMissingOnlySecondCareerTalentFeatureGroupDescription;
				rankEntryFeatureGroupVms.Add(new RankEntryEmptyFeaturesGroupVM(description));
				break;
			}
			}
		}
	}

	public void UpdateState(LevelUpManager levelUpManager)
	{
		m_ShowGroupList?.ForEach(delegate(RankEntryFeatureGroupVM vm)
		{
			vm.UpdateState(levelUpManager);
		});
		m_SelectionStateFeature.Value = (SelectionStateFeature)(levelUpManager?.GetSelectionState(m_CareerPathVM.CareerPath, m_SelectionFeature, Rank));
		(BlueprintFeature, int)? selectedFeature = m_CareerPathVM.Unit.Progression.GetSelectedFeature(m_CareerPathVM.CareerPath, Rank, m_SelectionFeature);
		if (selectedFeature.HasValue)
		{
			m_EntryState.Value = RankEntryState.Committed;
			SetSelectedFeature(selectedFeature.Value.Item1);
		}
		else if (m_SelectionStateFeature.Value != null)
		{
			if (!m_SelectionStateFeature.Value.IsValid)
			{
				m_EntryState.Value = RankEntryState.NotValid;
			}
			else if (m_SelectionStateFeature.Value.SelectionItem.HasValue)
			{
				m_EntryState.Value = RankEntryState.Selected;
			}
			else if (!m_SelectionStateFeature.Value.CanSelectAny)
			{
				m_EntryState.Value = RankEntryState.NotSelectable;
			}
			else if (m_CareerPathVM.FirstSelectable == this)
			{
				m_EntryState.Value = RankEntryState.FirstSelectable;
			}
			else if (Rank == m_CareerPathVM.FirstSelectable?.Rank)
			{
				m_EntryState.Value = RankEntryState.Selectable;
			}
			else
			{
				m_EntryState.Value = RankEntryState.WaitPreviousToSelect;
			}
			SetSelectedFeature(m_SelectionStateFeature.Value.SelectionItem);
		}
		else
		{
			m_EntryState.Value = RankEntryState.NotSelectable;
			SetSelectedFeature((FeatureSelectionItem?)null);
		}
	}

	public void HandleClick()
	{
		if (m_ShowGroupList == null)
		{
			m_ShowGroupList = CreateGroups();
		}
		m_SelectAction?.Invoke(this);
		HandleFilterChange(FeaturesFilterVM?.CurrentFilter.CurrentValue);
	}

	public void ClearSelectedFeature()
	{
		m_SelectionStateFeature.Value?.ClearSelection();
		SetSelectedFeature((FeatureSelectionItem?)null);
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
	}

	private void SelectFeature(FeatureSelectionItem? featureSelectionItem)
	{
		if (!featureSelectionItem.HasValue)
		{
			ClearSelectedFeature();
			return;
		}
		SelectionStateFeature value = m_SelectionStateFeature.Value;
		if (value != null && value.Select(featureSelectionItem.Value))
		{
			SetSelectedFeature(m_SelectionStateFeature.Value.SelectionItem);
			int entryId = m_CareerPathVM.RankEntriesScan.IndexOf(this);
			m_CareerPathVM.AddedOnLevelUpFeatures?.RefreshSelectedFeatureAtRank(SelectedFeature.CurrentValue, entryId);
			EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
			{
				h.HandleUISelectionChanged();
			});
		}
	}

	private void SetSelectedFeature(FeatureSelectionItem? selectionItem)
	{
		BlueprintFeature owner = (BlueprintFeature)(selectionItem?.SourceBlueprint);
		SetSelectedFeature(selectionItem?.Feature, owner);
	}

	private void SetSelectedFeature(BlueprintFeature feature, BlueprintFeature owner = null)
	{
		SelectedFeature.CurrentValue?.SetSelectedAndUpdate(isSelected: false);
		m_SelectedFeature.Value = GetFeatureForSelection(feature, owner);
		SelectedFeature.CurrentValue?.SetSelectedAndUpdate(isSelected: true);
	}

	private RankEntrySelectionFeatureVM GetFeatureForSelection(BlueprintFeature feature, BlueprintFeature owner = null)
	{
		RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM = null;
		if (feature == null)
		{
			return null;
		}
		if (m_ShowGroupList != null)
		{
			foreach (RankEntryFeatureGroupVM item in m_ShowGroupList.Where((RankEntryFeatureGroupVM i) => owner == null || i.Owner == owner))
			{
				rankEntrySelectionFeatureVM = Enumerable.FirstOrDefault(item.FeatureList, (BaseRankEntryFeatureVM vm) => vm.Feature == feature) as RankEntrySelectionFeatureVM;
				if (rankEntrySelectionFeatureVM != null)
				{
					break;
				}
			}
		}
		else
		{
			FeatureSelectionItem featureSelectionItem = m_SelectionFeature.GetSelectionItems(m_CareerPathVM.Unit, m_CareerPathVM.CareerPath).FirstOrDefault((FeatureSelectionItem i) => i.Feature == feature);
			if (featureSelectionItem.Feature == null)
			{
				return null;
			}
			RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM2 = ((!(featureSelectionItem.Feature is BlueprintStatAdvancement)) ? new RankEntrySelectionFeatureVM(this, m_CareerPathVM, featureSelectionItem, m_SelectionStateFeature, SelectFeature) : new RankEntrySelectionStatVM(this, m_CareerPathVM, featureSelectionItem, m_SelectionStateFeature, SelectFeature));
			rankEntrySelectionFeatureVM = rankEntrySelectionFeatureVM2;
			AddDisposable(rankEntrySelectionFeatureVM);
		}
		return rankEntrySelectionFeatureVM;
	}

	private void HandleFilterChange(FeatureFilterType? filter)
	{
		if (m_ShowGroupList == null)
		{
			m_ShowGroupList = CreateGroups();
		}
		if (filter == FeatureFilterType.FavoritesFilter)
		{
			m_ShowGroupList = CreateGroups();
		}
		bool flag = false;
		bool flag2 = false;
		FilteredGroupList.Clear();
		foreach (RankEntryFeatureGroupVM showGroup in m_ShowGroupList)
		{
			List<VirtualListElementVMBase> filtered = showGroup.GetFiltered(filter);
			flag2 |= filtered.Any(delegate(VirtualListElementVMBase f)
			{
				BaseRankEntryFeatureVM obj2 = f as BaseRankEntryFeatureVM;
				return obj2 != null && obj2.FeatureState.CurrentValue == RankFeatureState.NotSelectable;
			});
			if (!Game.Instance.Player.UISettings.ShowUnavailableFeatures)
			{
				filtered.RemoveAll(delegate(VirtualListElementVMBase f)
				{
					BaseRankEntryFeatureVM obj = f as BaseRankEntryFeatureVM;
					return obj != null && obj.FeatureState.CurrentValue == RankFeatureState.NotSelectable;
				});
			}
			if (flag && RankEntrySelectionUtils.HasFilter(FeatureGroup) && filtered.Any())
			{
				FilteredGroupList.Add(AddDisposableAndReturn(new SeparatorElementVM()));
			}
			FilteredGroupList.AddRange(filtered);
			flag = filtered.Any();
		}
		m_OnFilterChange?.Execute();
		m_HasUnavailableFeatures.Value = flag2;
	}

	public string GetHintText()
	{
		return UIStrings.Instance.CharacterSheet.GetFeatureGroupHint(FeatureGroup, CanChangeSelection);
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return new List<TooltipBaseTemplate> { HintTooltip, Tooltip };
	}

	public FeatureGroup? GetFeatureGroup()
	{
		return FeatureGroup;
	}

	public bool CanSelect()
	{
		if (EntryState.CurrentValue != 0)
		{
			return EntryState.CurrentValue != RankEntryState.Committed;
		}
		return false;
	}

	public void UpdateFeatures()
	{
		m_ShowGroupList = CreateGroups();
		HandleFilterChange(FeaturesFilterVM?.CurrentFilter.CurrentValue);
		SetSelectedFeature(SelectedFeature.CurrentValue?.Feature);
	}

	public void UpdateReadOnlyState()
	{
		m_ShowGroupList.ForEach(delegate(RankEntryFeatureGroupVM g)
		{
			g.UpdateReadOnlyState();
		});
	}

	public void SetFocusOn(BaseRankEntryFeatureVM featureVM)
	{
		m_ShowGroupList?.SelectMany((RankEntryFeatureGroupVM g) => g.FeatureList).ForEach(delegate(BaseRankEntryFeatureVM rankEntry)
		{
			rankEntry.SetFocusOn(featureVM);
		});
	}

	public void ToggleShowUnavailableFeatures()
	{
		PlayerUISettings uISettings = Game.Instance.Player.UISettings;
		uISettings.ShowUnavailableFeatures = !uISettings.ShowUnavailableFeatures;
		HandleFilterChange(FeaturesFilterVM?.CurrentFilter.CurrentValue);
	}

	public bool ContainsFeature(string key)
	{
		List<RankEntryFeatureGroupVM> showGroupList = m_ShowGroupList;
		if (showGroupList == null)
		{
			return false;
		}
		return showGroupList.SelectMany((RankEntryFeatureGroupVM l) => l.FeatureList).FindIndex((BaseRankEntryFeatureVM f) => f.Feature.AssetGuid == key) >= 0;
	}
}
