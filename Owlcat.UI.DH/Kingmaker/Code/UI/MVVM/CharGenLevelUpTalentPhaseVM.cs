using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using UnityEngine;

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
		m_IsGroupedBySource.Value = !m_IsGroupedBySource.Value;
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
				m_Items.Add(new CharGenLevelUpSelectorTalentItemVM(f, OnItemHovered, CharGenContext.LevelUpManager.CurrentValue).AddTo(this));
			});
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
		CreateItemList();
	}

	protected override void UpdateItem(CharGenLevelUpSelectorBaseItemVM itemVM)
	{
		itemVM.RefreshView.Execute(default(Unit));
		if (itemVM is CharGenLevelUpSelectorTalentItemVM charGenLevelUpSelectorTalentItemVM)
		{
			if (SelectionFeature.CanSelect(charGenLevelUpSelectorTalentItemVM.FeatureSelectionItem))
			{
				charGenLevelUpSelectorTalentItemVM.UpdateAccessibility(LEVEL_UP_ITEM_STATE.Available);
				return;
			}
			bool flag = SelectionFeature.GetCalculatedPrerequisite(charGenLevelUpSelectorTalentItemVM.FeatureSelectionItem) is CalculatedPrerequisiteMaxRankNotReached;
			charGenLevelUpSelectorTalentItemVM.UpdateAccessibility((!flag) ? LEVEL_UP_ITEM_STATE.NotAvailable : LEVEL_UP_ITEM_STATE.AlreadyExist);
		}
	}

	private void CreateListBySource()
	{
		foreach (BlueprintUnitFact source in (from g in SelectionFeature.Items.Select((FeatureSelectionItem i) => i.GetSourceBlueprint()).Distinct()
			orderby g?.Name
			select g).ToList())
		{
			CharGenLevelUpNestedListHeaderVM header = null;
			if (source != null)
			{
				header = new CharGenLevelUpNestedListHeaderVM(source).AddTo(this);
				AddItem(header);
			}
			(from i in m_Items
				where i.FeatureSelectionItem.GetSourceBlueprint() == source
				orderby i.State.CurrentValue, i.FeatureSelectionItem.Feature.Name
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
		List<CharGenLevelUpNestedListHeaderVM> list2 = new List<CharGenLevelUpNestedListHeaderVM>();
		foreach (TalentGroup type in list)
		{
			CharGenLevelUpNestedListHeaderVM header = new CharGenLevelUpNestedListHeaderVM(null, type.ToString(), null, 0, (recommendedTalentGroup & type) != 0).AddTo(this);
			if (m_Items.Any((CharGenLevelUpSelectorTalentItemVM i) => i.FeatureSelectionItem.Feature.TalentIconInfo.MainGroup == type))
			{
				list2.Add(header);
				AddItem(header);
				(from i in m_Items
					where i.FeatureSelectionItem.Feature.TalentIconInfo.MainGroup == type
					orderby i.State.CurrentValue, i.FeatureSelectionItem.Feature.Name
					select i).ForEach(delegate(CharGenLevelUpSelectorTalentItemVM f)
				{
					f.SetParentNode(header);
					AddItem(f);
				});
			}
		}
		if (recommendedTalentGroup != 0)
		{
			list2.ForEach(delegate(CharGenLevelUpNestedListHeaderVM h)
			{
				h.SetExpand(h.IsRecommended.CurrentValue);
			});
		}
	}
}
