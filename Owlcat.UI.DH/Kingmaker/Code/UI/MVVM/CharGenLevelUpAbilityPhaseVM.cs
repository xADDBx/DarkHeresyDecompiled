using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpAbilityPhaseVM : CharGenLevelUpBaseSelectionPhaseVM<CharGenLevelUpSelectorBaseItemVM>
{
	public CharGenLevelUpAbilityPhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionFeature, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, CharGenPhaseType.LevelUpAbility, selectionFeature, infoSectionVM, rank)
	{
	}

	protected override void CreateItemList()
	{
		if (CareerPath == null || base.Unit == null)
		{
			Debug.LogError("CareerPath or Unit is null");
			return;
		}
		List<CharGenLevelUpSelectorAbilityItemVM> list = (from x in SelectionFeature.Items
			group x by x.Feature into g
			select g.First() into f
			select new CharGenLevelUpSelectorAbilityItemVM(f, OnItemHovered, CharGenContext.LevelUpManager.CurrentValue)).ToList();
		list.ForEach(delegate(CharGenLevelUpSelectorAbilityItemVM i)
		{
			UpdateItem(i);
		});
		(from i in list
			orderby i.State.CurrentValue, i.Label.CurrentValue
			select i).ForEach(delegate(CharGenLevelUpSelectorAbilityItemVM i)
		{
			AddItem(i);
		});
		if (base.SelectedItem.CurrentValue != null)
		{
			SelectionGroup.TrySelectEntity(Items.FirstOrDefault((CharGenLevelUpSelectorBaseItemVM i) => i.Blueprint == base.SelectedItem.CurrentValue?.Blueprint));
		}
	}

	protected override void SaveSelection()
	{
		base.SaveSelection();
		if (base.SelectedItem.CurrentValue is CharGenLevelUpSelectorAbilityItemVM charGenLevelUpSelectorAbilityItemVM)
		{
			SelectionFeature.Select(charGenLevelUpSelectorAbilityItemVM.FeatureSelectionItem);
		}
		else
		{
			SelectionFeature?.ClearSelection();
		}
	}

	protected override void CheckItems()
	{
		List<FeatureSelectionItem> featureSelections = new List<FeatureSelectionItem>();
		foreach (CharGenLevelUpSelectorBaseItemVM item in Items)
		{
			if (item is CharGenLevelUpSelectorAbilityItemVM charGenLevelUpSelectorAbilityItemVM)
			{
				featureSelections.Add(charGenLevelUpSelectorAbilityItemVM.FeatureSelectionItem);
			}
		}
		if (!SelectionFeature.Items.All((FeatureSelectionItem i) => featureSelections.Contains(i)))
		{
			ClearItemList();
			CreateItemList();
		}
	}

	protected override void UpdateItem(CharGenLevelUpSelectorBaseItemVM itemVM)
	{
		itemVM.RefreshView.Execute(default(Unit));
		if (itemVM is CharGenLevelUpSelectorAbilityItemVM charGenLevelUpSelectorAbilityItemVM)
		{
			if (SelectionFeature.CanSelect(charGenLevelUpSelectorAbilityItemVM.FeatureSelectionItem))
			{
				charGenLevelUpSelectorAbilityItemVM.UpdateAccessibility(LEVEL_UP_ITEM_STATE.Available);
				return;
			}
			bool flag = SelectionFeature.GetCalculatedPrerequisite(charGenLevelUpSelectorAbilityItemVM.FeatureSelectionItem) is CalculatedPrerequisiteMaxRankNotReached;
			charGenLevelUpSelectorAbilityItemVM.UpdateAccessibility((!flag) ? LEVEL_UP_ITEM_STATE.NotAvailable : LEVEL_UP_ITEM_STATE.AlreadyExist);
		}
	}
}
