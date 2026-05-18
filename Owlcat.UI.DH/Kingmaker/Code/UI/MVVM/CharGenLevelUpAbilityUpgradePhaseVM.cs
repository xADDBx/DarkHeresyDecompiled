using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpAbilityUpgradePhaseVM : CharGenLevelUpBaseSelectionPhaseVM<CharGenLevelUpSelectorBaseItemVM>
{
	public CharGenLevelUpAbilityUpgradePhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionFeature, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, CharGenPhaseType.LevelUpUpgrade, selectionFeature, infoSectionVM, rank)
	{
	}

	protected override void CreateItemList()
	{
		if (CareerPath == null || base.Unit == null)
		{
			Debug.LogError("CareerPath or Unit is null");
			return;
		}
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)base.Unit;
			List<CharGenLevelUpSelectorAbilityUpgradeItemVM> list = (from x in SelectionFeature.Items
				group x by x.Feature into g
				select g.First() into f
				select new CharGenLevelUpSelectorAbilityUpgradeItemVM(f, OnItemHovered, m_CharGenContext.LevelUpManager.CurrentValue)).ToList();
			list.ForEach(delegate(CharGenLevelUpSelectorAbilityUpgradeItemVM i)
			{
				UpdateItem(i);
			});
			(from i in list
				orderby i.State.CurrentValue, i.CanShowFavorite.CurrentValue && i.IsFavorite.CurrentValue descending, i.BaseAbilityData?.Name, i.Label.CurrentValue
				select i).ForEach(delegate(CharGenLevelUpSelectorAbilityUpgradeItemVM i)
			{
				AddItem(i);
			});
			if (base.SelectedItem.CurrentValue != null)
			{
				SelectionGroup.TrySelectEntity(Items.FirstOrDefault((CharGenLevelUpSelectorBaseItemVM i) => i.Blueprint == base.SelectedItem.CurrentValue?.Blueprint));
			}
		}
	}

	protected override void SaveSelection()
	{
		base.SaveSelection();
		if (base.SelectedItem.CurrentValue is CharGenLevelUpSelectorAbilityUpgradeItemVM charGenLevelUpSelectorAbilityUpgradeItemVM)
		{
			SelectionFeature.Select(charGenLevelUpSelectorAbilityUpgradeItemVM.FeatureSelectionItem);
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
			if (item is CharGenLevelUpSelectorAbilityUpgradeItemVM charGenLevelUpSelectorAbilityUpgradeItemVM)
			{
				featureSelections.Add(charGenLevelUpSelectorAbilityUpgradeItemVM.FeatureSelectionItem);
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
		if (itemVM is CharGenLevelUpSelectorAbilityUpgradeItemVM charGenLevelUpSelectorAbilityUpgradeItemVM)
		{
			charGenLevelUpSelectorAbilityUpgradeItemVM.SetFavorite(m_CharGenContext.CharGenConfig.Mode == CharGenMode.LevelUp, UnitSaveData.FavoriteFeatures.Contains(charGenLevelUpSelectorAbilityUpgradeItemVM.Blueprint.AssetGuidThreadSafe));
			if (SelectionFeature.CanSelect(charGenLevelUpSelectorAbilityUpgradeItemVM.FeatureSelectionItem))
			{
				charGenLevelUpSelectorAbilityUpgradeItemVM.UpdateAccessibility(LEVEL_UP_ITEM_STATE.Available);
				return;
			}
			bool flag = SelectionFeature.GetCalculatedPrerequisite(charGenLevelUpSelectorAbilityUpgradeItemVM.FeatureSelectionItem) is CalculatedPrerequisiteMaxRankNotReached;
			charGenLevelUpSelectorAbilityUpgradeItemVM.UpdateAccessibility((!flag) ? LEVEL_UP_ITEM_STATE.NotAvailable : LEVEL_UP_ITEM_STATE.AlreadyExist);
		}
	}
}
