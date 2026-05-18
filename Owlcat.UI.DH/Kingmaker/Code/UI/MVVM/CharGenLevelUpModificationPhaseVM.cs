using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpModificationPhaseVM : CharGenLevelUpBaseSelectionPhaseVM<CharGenLevelUpSelectorBaseItemVM>
{
	private ReactiveProperty<bool> m_IsGroupedBySource = new ReactiveProperty<bool>();

	private List<CharGenLevelUpSelectorModificationItemVM> m_Items = new List<CharGenLevelUpSelectorModificationItemVM>();

	public ReadOnlyReactiveProperty<bool> IsGroupedBySource => m_IsGroupedBySource;

	public CharGenLevelUpModificationPhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionFeature, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, CharGenPhaseType.LevelUpModification, selectionFeature, infoSectionVM, rank)
	{
	}

	public void ToggleGrouping()
	{
		SaveDropdownsState();
		SaveFavorites();
		m_IsGroupedBySource.Value = !m_IsGroupedBySource.Value;
		UnitSaveData.PreferredModifierSortingType = (m_IsGroupedBySource.CurrentValue ? CharGenUnitSaveData.ChargenDropdownType.ModifierSource : CharGenUnitSaveData.ChargenDropdownType.ModifierType);
		CreateItemList();
	}

	protected override void CreateItemList()
	{
		if (CareerPath == null || base.Unit == null)
		{
			Debug.LogError("CareerPath or Unit is null");
			return;
		}
		Items.Clear();
		if (m_Items.Empty())
		{
			(from x in SelectionFeature.Items
				group x by x.Feature into g
				select g.First() into g
				orderby g.Feature.Name
				select g).ForEach(delegate(FeatureSelectionItem f)
			{
				m_Items.Add(new CharGenLevelUpSelectorModificationItemVM(f, OnItemHovered, null, m_CharGenContext.LevelUpManager.CurrentValue).AddTo(this));
			});
		}
		m_Items.ForEach(delegate(CharGenLevelUpSelectorModificationItemVM i)
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
		if (base.SelectedItem.CurrentValue is CharGenLevelUpSelectorModificationItemVM charGenLevelUpSelectorModificationItemVM)
		{
			SelectionFeature.Select(charGenLevelUpSelectorModificationItemVM.FeatureSelectionItem);
		}
		else
		{
			SelectionFeature?.ClearSelection();
		}
	}

	protected override void CheckItems()
	{
		List<FeatureSelectionItem> first = m_Items.Select((CharGenLevelUpSelectorModificationItemVM i) => i.FeatureSelectionItem).ToList();
		List<FeatureSelectionItem> second = SelectionFeature.Items.OrderBy((FeatureSelectionItem g) => g.Feature.Name).ToList();
		if (!first.SequenceEqual(second))
		{
			ClearItemList();
			m_Items.Clear();
		}
		m_IsGroupedBySource.Value = UnitSaveData.PreferredModifierSortingType == CharGenUnitSaveData.ChargenDropdownType.ModifierSource;
		CreateItemList();
	}

	protected override void UpdateItem(CharGenLevelUpSelectorBaseItemVM itemVM)
	{
		itemVM.RefreshView.Execute(default(Unit));
		if (itemVM is CharGenLevelUpSelectorModificationItemVM charGenLevelUpSelectorModificationItemVM)
		{
			charGenLevelUpSelectorModificationItemVM.SetFavorite(m_CharGenContext.CharGenConfig.Mode == CharGenMode.LevelUp, UnitSaveData.FavoriteFeatures.Contains(charGenLevelUpSelectorModificationItemVM.Blueprint.AssetGuidThreadSafe));
			if (SelectionFeature.CanSelect(charGenLevelUpSelectorModificationItemVM.FeatureSelectionItem))
			{
				charGenLevelUpSelectorModificationItemVM.UpdateAccessibility(LEVEL_UP_ITEM_STATE.Available);
				return;
			}
			bool flag = SelectionFeature.GetCalculatedPrerequisite(charGenLevelUpSelectorModificationItemVM.FeatureSelectionItem) is CalculatedPrerequisiteMaxRankNotReached;
			charGenLevelUpSelectorModificationItemVM.UpdateAccessibility((!flag) ? LEVEL_UP_ITEM_STATE.NotAvailable : LEVEL_UP_ITEM_STATE.AlreadyExist);
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
				header.SetExpand(UnitSaveData.IsDropdownOpen(CharGenUnitSaveData.ChargenDropdownType.ModifierSource, header.Label.CurrentValue) ?? true);
				AddItem(header);
			}
			(from i in m_Items
				where i.FeatureSelectionItem.GetSourceBlueprint().Name == source
				orderby i.State.CurrentValue, i.CanShowFavorite.CurrentValue && i.IsFavorite.CurrentValue descending, i.FeatureSelectionItem.Feature.Name
				select i).ForEach(delegate(CharGenLevelUpSelectorModificationItemVM f)
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
			if (m_Items.Any((CharGenLevelUpSelectorModificationItemVM i) => i.FeatureSelectionItem.Feature.TalentIconInfo.MainGroup == type))
			{
				header.IsExpanded.Subscribe(delegate
				{
					CheckIsAllCollapsedHeaders();
				}).AddTo(header);
				header.SetExpand(UnitSaveData.IsDropdownOpen(CharGenUnitSaveData.ChargenDropdownType.ModifierType, header.Label.CurrentValue) ?? header.IsRecommended.CurrentValue);
				AddItem(header);
				(from i in m_Items
					where i.FeatureSelectionItem.Feature.TalentIconInfo.MainGroup == type
					orderby i.State.CurrentValue, i.Tags.First().Name.Text, i.CanShowFavorite.CurrentValue && i.IsFavorite.CurrentValue descending, i.FeatureSelectionItem.Feature.Name
					select i).ForEach(delegate(CharGenLevelUpSelectorModificationItemVM f)
				{
					f.SetParentNode(header);
					AddItem(f);
				});
			}
		}
		List<CharGenLevelUpSelectorModificationItemVM> list2 = m_Items.Where((CharGenLevelUpSelectorModificationItemVM i) => !i.FeatureSelectionItem.Feature.TalentIconInfo.HasGroups).ToList();
		if (list2.Any())
		{
			CharGenLevelUpNestedListHeaderVM header = new CharGenLevelUpNestedListHeaderVM(null, "No Talent Group").AddTo(this);
			AddItem(header);
			list2.ForEach(delegate(CharGenLevelUpSelectorModificationItemVM f)
			{
				f.SetParentNode(header);
				AddItem(f);
			});
		}
	}

	private void SaveDropdownsState()
	{
		List<CharGenLevelUpNestedListHeaderVM> source = Items.OfType<CharGenLevelUpNestedListHeaderVM>().ToList();
		UnitSaveData.SetDropDownList(UnitSaveData.PreferredModifierSortingType, source.ToDictionary((CharGenLevelUpNestedListHeaderVM h) => h.Label.CurrentValue, (CharGenLevelUpNestedListHeaderVM h) => h.IsExpanded.CurrentValue));
	}
}
