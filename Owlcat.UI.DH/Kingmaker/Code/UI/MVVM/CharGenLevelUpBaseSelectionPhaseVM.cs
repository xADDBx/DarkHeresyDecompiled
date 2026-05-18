using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpBaseSelectionPhaseVM<TSelectorItem> : CharGenLevelUpBasePhaseVM<TSelectorItem> where TSelectorItem : CharGenLevelUpSelectorBaseItemVM
{
	protected readonly ReactiveProperty<bool> m_IsAllCollapsed = new ReactiveProperty<bool>();

	protected readonly SelectionStateFeature SelectionFeature;

	protected readonly CharGenUnitSaveData UnitSaveData;

	public ReadOnlyReactiveProperty<bool> IsAllCollapsed => m_IsAllCollapsed;

	public CharGenLevelUpBaseSelectionPhaseVM(CharGenContext charGenContext, CharGenPhaseType phaseType, SelectionStateFeature selectionFeature, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, phaseType, infoSectionVM, rank)
	{
		SelectionFeature = selectionFeature;
		base.BlueprintSelectionWithUI = selectionFeature?.Blueprint;
		m_PhaseName.Value = base.BlueprintSelectionWithUI?.Title;
		UnitSaveData = Game.Instance.Player.UISettings.ChargenData.GetDataForUnit(base.Unit?.Blueprint.AssetGuidThreadSafe);
	}

	public void CollapseItemGroups()
	{
		bool currentValue = m_IsAllCollapsed.CurrentValue;
		foreach (TSelectorItem item in Items)
		{
			if (item is CharGenLevelUpNestedListHeaderVM charGenLevelUpNestedListHeaderVM)
			{
				charGenLevelUpNestedListHeaderVM.SetExpand(currentValue);
			}
		}
	}

	protected override void OnBeginDetailedView()
	{
		base.OnBeginDetailedView();
		CheckItems();
		Items.ForEach(delegate(TSelectorItem i)
		{
			UpdateItem(i);
		});
	}

	protected override void OnEndDetailedView()
	{
		base.OnEndDetailedView();
		SaveFavorites();
	}

	protected override bool CheckIsCompleted()
	{
		if (base.CheckIsCompleted())
		{
			SelectionStateFeature selectionFeature = SelectionFeature;
			if (selectionFeature != null && selectionFeature.IsMade)
			{
				return selectionFeature.IsValid;
			}
			return false;
		}
		return false;
	}

	protected override void CreateItemList()
	{
		if (CareerPath == null || base.Unit == null)
		{
			Debug.LogError("CareerPath or Unit is null");
			return;
		}
		List<TSelectorItem> list = SelectionFeature.Items.Select((FeatureSelectionItem f) => new CharGenLevelUpSelectorItemSelectionVM(f, OnItemHovered) as TSelectorItem).ToList();
		list.ForEach(delegate(TSelectorItem i)
		{
			UpdateItem(i);
		});
		(from i in list
			orderby i.State.CurrentValue, i.Label.CurrentValue
			select i).ForEach(delegate(TSelectorItem i)
		{
			AddItem(i);
		});
		if (base.SelectedItem.CurrentValue != null && list.Contains(base.SelectedItem.CurrentValue))
		{
			SelectionGroup.TrySelectEntity(Items.FirstOrDefault((TSelectorItem i) => i.Blueprint == base.SelectedItem.CurrentValue?.Blueprint));
		}
	}

	protected override void SaveSelection()
	{
		base.SaveSelection();
		if (base.SelectedItem.CurrentValue is CharGenLevelUpSelectorItemSelectionVM charGenLevelUpSelectorItemSelectionVM)
		{
			SelectionFeature.Select(charGenLevelUpSelectorItemSelectionVM.FeatureSelectionItem);
		}
		else
		{
			SelectionFeature?.ClearSelection();
		}
	}

	protected virtual void UpdateItem(TSelectorItem itemVM)
	{
		itemVM.RefreshView.Execute(default(Unit));
		if (itemVM is CharGenLevelUpSelectorItemSelectionVM charGenLevelUpSelectorItemSelectionVM)
		{
			if (SelectionFeature.CanSelect(charGenLevelUpSelectorItemSelectionVM.FeatureSelectionItem))
			{
				charGenLevelUpSelectorItemSelectionVM.UpdateAccessibility(LEVEL_UP_ITEM_STATE.Available);
				return;
			}
			bool flag = SelectionFeature.GetCalculatedPrerequisite(charGenLevelUpSelectorItemSelectionVM.FeatureSelectionItem) is CalculatedPrerequisiteMaxRankNotReached;
			charGenLevelUpSelectorItemSelectionVM.UpdateAccessibility((!flag) ? LEVEL_UP_ITEM_STATE.NotAvailable : LEVEL_UP_ITEM_STATE.AlreadyExist);
		}
	}

	protected virtual void CheckItems()
	{
		List<FeatureSelectionItem> featureSelections = new List<FeatureSelectionItem>();
		foreach (TSelectorItem item in Items)
		{
			if (item is CharGenLevelUpSelectorItemSelectionVM charGenLevelUpSelectorItemSelectionVM)
			{
				featureSelections.Add(charGenLevelUpSelectorItemSelectionVM.FeatureSelectionItem);
			}
		}
		if (!SelectionFeature.Items.All((FeatureSelectionItem i) => featureSelections.Contains(i)))
		{
			ClearItemList();
			CreateItemList();
		}
	}

	protected void ClearItemList()
	{
		Items.ForEach(delegate(TSelectorItem i)
		{
			i.Dispose();
		});
		Items.Clear();
	}

	protected void SaveFavorites()
	{
		foreach (TSelectorItem item in Items.Where((TSelectorItem i) => i.Blueprint != null))
		{
			if (item.IsFavorite.CurrentValue)
			{
				UnitSaveData.FavoriteFeatures.Add(item.Blueprint.AssetGuidThreadSafe);
			}
			else
			{
				UnitSaveData.FavoriteFeatures.Remove(item.Blueprint.AssetGuidThreadSafe);
			}
		}
	}

	protected void CheckIsAllCollapsedHeaders()
	{
		m_IsAllCollapsed.Value = Items.OfType<CharGenLevelUpNestedListHeaderVM>().All((CharGenLevelUpNestedListHeaderVM h) => !h.IsExpanded.CurrentValue);
	}

	protected override void DisposeImplementation()
	{
		SaveFavorites();
		base.DisposeImplementation();
		SelectionFeature?.ClearSelection();
	}
}
