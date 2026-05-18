using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpTalentPhaseVM : CharGenLevelUpBaseSelectionPhaseVM<CharGenLevelUpSelectorBaseItemVM>
{
	private ReactiveProperty<bool> m_IsGroupedBySource = new ReactiveProperty<bool>();

	private List<CharGenLevelUpSelectorTalentItemVM> m_Items = new List<CharGenLevelUpSelectorTalentItemVM>();

	public ReadOnlyReactiveProperty<bool> IsGroupedBySource => m_IsGroupedBySource;

	public CharGenLevelUpTalentPhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionFeature, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, CharGenPhaseType.LevelUpTalent, selectionFeature, infoSectionVM, rank)
	{
	}

	public void ToggleGrouping()
	{
		SaveDropdownsState();
		SaveFavorites();
		m_IsGroupedBySource.Value = !m_IsGroupedBySource.Value;
		UnitSaveData.PreferredTalentSortingType = (m_IsGroupedBySource.CurrentValue ? CharGenUnitSaveData.ChargenDropdownType.TalentSource : CharGenUnitSaveData.ChargenDropdownType.TalentType);
		CreateItemList();
	}

	protected override void CreateItemList()
	{
		if (CareerPath == null || base.Unit == null)
		{
			PFLog.UI.Error("CareerPath or Unit is null");
			return;
		}
		Items.Clear();
		if (m_Items.Empty())
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)base.Unit;
				(from x in SelectionFeature.Items
					group x by x.Feature into g
					select g.First() into g
					orderby g.Feature.Name
					select g).ForEach(delegate(FeatureSelectionItem f)
				{
					m_Items.Add(new CharGenLevelUpSelectorTalentItemVM(f, OnItemHovered, m_CharGenContext.LevelUpManager.CurrentValue).AddTo(this));
				});
			}
		}
		m_Items.ForEach(delegate(CharGenLevelUpSelectorTalentItemVM i)
		{
			UpdateItem(i);
		});
		if (m_IsGroupedBySource.Value)
		{
			CreateListBySource();
		}
		else
		{
			CreateListByType();
		}
		if (base.SelectedItem.CurrentValue != null)
		{
			SelectionGroup.TrySelectEntity(Items.FirstOrDefault((CharGenLevelUpSelectorBaseItemVM i) => i.Blueprint == base.SelectedItem.CurrentValue?.Blueprint));
		}
		CheckIsAllCollapsedHeaders();
	}

	protected override void SaveSelection()
	{
		base.SaveSelection();
		if (base.SelectedItem.CurrentValue is CharGenLevelUpSelectorTalentItemVM charGenLevelUpSelectorTalentItemVM)
		{
			SelectionFeature.Select(charGenLevelUpSelectorTalentItemVM.FeatureSelectionItem);
		}
		else
		{
			SelectionFeature?.ClearSelection();
		}
	}

	protected override void CheckItems()
	{
		List<FeatureSelectionItem> first = m_Items.Select((CharGenLevelUpSelectorTalentItemVM i) => i.FeatureSelectionItem).ToList();
		List<FeatureSelectionItem> second = SelectionFeature.Items.OrderBy((FeatureSelectionItem g) => g.Feature.Name).ToList();
		if (!first.SequenceEqual(second))
		{
			ClearItemList();
			m_Items.Clear();
		}
		m_IsGroupedBySource.Value = UnitSaveData.PreferredTalentSortingType == CharGenUnitSaveData.ChargenDropdownType.TalentSource;
		CreateItemList();
	}

	protected override void UpdateItem(CharGenLevelUpSelectorBaseItemVM itemVM)
	{
		itemVM.RefreshView.Execute(default(Unit));
		if (itemVM is CharGenLevelUpSelectorTalentItemVM charGenLevelUpSelectorTalentItemVM)
		{
			charGenLevelUpSelectorTalentItemVM.SetFavorite(m_CharGenContext.CharGenConfig.Mode == CharGenMode.LevelUp, UnitSaveData.FavoriteFeatures.Contains(charGenLevelUpSelectorTalentItemVM.Blueprint.AssetGuidThreadSafe));
			if (SelectionFeature.CanSelect(charGenLevelUpSelectorTalentItemVM.FeatureSelectionItem))
			{
				charGenLevelUpSelectorTalentItemVM.UpdateAccessibility(LEVEL_UP_ITEM_STATE.Available);
				return;
			}
			bool flag = SelectionFeature.GetCalculatedPrerequisite(charGenLevelUpSelectorTalentItemVM.FeatureSelectionItem) is CalculatedPrerequisiteMaxRankNotReached;
			charGenLevelUpSelectorTalentItemVM.UpdateAccessibility((!flag) ? LEVEL_UP_ITEM_STATE.NotAvailable : LEVEL_UP_ITEM_STATE.AlreadyExist);
		}
	}

	protected override void DisposeImplementation()
	{
		SaveDropdownsState();
		SaveFavorites();
		base.DisposeImplementation();
	}

	protected override void OnEndDetailedView()
	{
		base.OnEndDetailedView();
		SaveDropdownsState();
	}

	private void CreateListBySource()
	{
		foreach (string source in (from g in SelectionFeature.Items.Select((FeatureSelectionItem i) => i.GetSourceBlueprint().Name).Distinct()
			orderby g
			select g).ToList())
		{
			CharGenLevelUpNestedListHeaderVM header = null;
			if (source != null)
			{
				header = new CharGenLevelUpNestedListHeaderVM(null, source).AddTo(this);
				header.IsExpanded.Subscribe(delegate
				{
					CheckIsAllCollapsedHeaders();
				}).AddTo(header);
				header.SetExpand(UnitSaveData.IsDropdownOpen(CharGenUnitSaveData.ChargenDropdownType.TalentSource, header.Label.CurrentValue) ?? true);
				AddItem(header);
			}
			(from i in m_Items
				where i.FeatureSelectionItem.GetSourceBlueprint().Name == source
				orderby i.State.CurrentValue, i.CanShowFavorite.CurrentValue && i.IsFavorite.CurrentValue descending, i.FeatureSelectionItem.Feature.Name
				select i).ForEach(delegate(CharGenLevelUpSelectorTalentItemVM f)
			{
				f.SetParentNode(header);
				AddItem(f);
			});
		}
	}

	private void CreateListByType()
	{
		List<TalentGroup> list = Enum.GetValues(typeof(TalentGroup)).Cast<TalentGroup>().ToList();
		TalentGroup recommendedTalentGroup = base.Unit.Progression.RecommendedTalentGroup;
		foreach (TalentGroup type in list)
		{
			CharGenLevelUpNestedListHeaderVM header = new CharGenLevelUpNestedListHeaderVM(null, type.ToString(), null, 0, (recommendedTalentGroup & type) != 0).AddTo(this);
			if (m_Items.Any((CharGenLevelUpSelectorTalentItemVM i) => i.FeatureSelectionItem.Feature.TalentIconInfo.MainGroup == type))
			{
				header.IsExpanded.Subscribe(delegate
				{
					CheckIsAllCollapsedHeaders();
				}).AddTo(header);
				header.SetExpand(UnitSaveData.IsDropdownOpen(CharGenUnitSaveData.ChargenDropdownType.TalentType, header.Label.CurrentValue) ?? header.IsRecommended.CurrentValue);
				AddItem(header);
				(from i in m_Items
					where i.FeatureSelectionItem.Feature.TalentIconInfo.MainGroup == type
					orderby i.State.CurrentValue, i.CanShowFavorite.CurrentValue && i.IsFavorite.CurrentValue descending, i.FeatureSelectionItem.Feature.Name
					select i).ForEach(delegate(CharGenLevelUpSelectorTalentItemVM f)
				{
					f.SetParentNode(header);
					AddItem(f);
				});
			}
		}
		List<CharGenLevelUpSelectorTalentItemVM> list2 = m_Items.Where((CharGenLevelUpSelectorTalentItemVM i) => !i.FeatureSelectionItem.Feature.TalentIconInfo.HasGroups).ToList();
		if (list2.Any())
		{
			CharGenLevelUpNestedListHeaderVM header = new CharGenLevelUpNestedListHeaderVM(null, "No Talent Group").AddTo(this);
			AddItem(header);
			list2.ForEach(delegate(CharGenLevelUpSelectorTalentItemVM f)
			{
				f.SetParentNode(header);
				AddItem(f);
			});
		}
	}

	private void SaveDropdownsState()
	{
		List<CharGenLevelUpNestedListHeaderVM> source = Items.OfType<CharGenLevelUpNestedListHeaderVM>().ToList();
		UnitSaveData.SetDropDownList(UnitSaveData.PreferredTalentSortingType, source.ToDictionary((CharGenLevelUpNestedListHeaderVM h) => h.Label.CurrentValue, (CharGenLevelUpNestedListHeaderVM h) => h.IsExpanded.CurrentValue));
	}
}
