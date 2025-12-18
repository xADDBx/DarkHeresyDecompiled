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

public class CharGenLevelUpSpecializationPhaseVM : CharGenLevelUpBaseSelectionPhaseVM<CharGenLevelUpSelectorBaseItemVM>
{
	public CharGenLevelUpSpecializationPhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionFeature, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, CharGenPhaseType.LevelUpSpecialization, selectionFeature, infoSectionVM, rank)
	{
	}

	protected override void CreateItemList()
	{
		if (CareerPath == null || base.Unit == null)
		{
			Debug.LogError("CareerPath or Unit is null");
			return;
		}
		List<CharGenLevelUpSelectorSpecializationItemVM> list = (from x in SelectionFeature.Items
			group x by x.Feature into g
			select g.First() into f
			select new CharGenLevelUpSelectorSpecializationItemVM(f, OnItemHovered)).ToList();
		list.ForEach(delegate(CharGenLevelUpSelectorSpecializationItemVM i)
		{
			UpdateItem(i);
		});
		(from i in list
			orderby i.State.CurrentValue, i.Label.CurrentValue
			select i).ForEach(delegate(CharGenLevelUpSelectorSpecializationItemVM i)
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
		if (base.SelectedItem.CurrentValue is CharGenLevelUpSelectorSpecializationItemVM charGenLevelUpSelectorSpecializationItemVM)
		{
			SelectionFeature.Select(charGenLevelUpSelectorSpecializationItemVM.FeatureSelectionItem);
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
			if (item is CharGenLevelUpSelectorSpecializationItemVM charGenLevelUpSelectorSpecializationItemVM)
			{
				featureSelections.Add(charGenLevelUpSelectorSpecializationItemVM.FeatureSelectionItem);
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
		if (itemVM is CharGenLevelUpSelectorSpecializationItemVM charGenLevelUpSelectorSpecializationItemVM)
		{
			if (SelectionFeature.CanSelect(charGenLevelUpSelectorSpecializationItemVM.FeatureSelectionItem))
			{
				charGenLevelUpSelectorSpecializationItemVM.UpdateAccessibility(LEVEL_UP_ITEM_STATE.Available);
				return;
			}
			bool flag = SelectionFeature.GetCalculatedPrerequisite(charGenLevelUpSelectorSpecializationItemVM.FeatureSelectionItem) is CalculatedPrerequisiteMaxRankNotReached;
			charGenLevelUpSelectorSpecializationItemVM.UpdateAccessibility((!flag) ? LEVEL_UP_ITEM_STATE.NotAvailable : LEVEL_UP_ITEM_STATE.AlreadyExist);
		}
	}
}
