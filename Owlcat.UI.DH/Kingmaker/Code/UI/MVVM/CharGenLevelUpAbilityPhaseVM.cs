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

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpAbilityPhaseVM : CharGenLevelUpBaseSelectionPhaseVM<CharGenLevelUpSelectorBaseItemVM>
{
	public CharGenLevelUpAbilityPhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionFeature, InfoSectionVM infoSectionVM, int rank = 0, CharGenPhaseType phaseType = CharGenPhaseType.LevelUpAbility, bool isAllowSwitchOff = true)
		: base(charGenContext, phaseType, selectionFeature, infoSectionVM, rank)
	{
	}

	protected override void CreateItemList()
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)base.Unit;
			List<CharGenLevelUpSelectorAbilityItemVM> list = (from x in SelectionFeature.Items
				group x by x.Feature into g
				select g.First() into f
				select new CharGenLevelUpSelectorAbilityItemVM(f, OnItemHovered, m_CharGenContext.LevelUpManager.CurrentValue)).ToList();
			list.ForEach(delegate(CharGenLevelUpSelectorAbilityItemVM i)
			{
				UpdateItem(i);
			});
			(from i in list
				orderby i.State.CurrentValue, i.CanShowFavorite.CurrentValue && i.IsFavorite.CurrentValue descending, i.Label.CurrentValue
				select i).ForEach(delegate(CharGenLevelUpSelectorAbilityItemVM i)
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
			charGenLevelUpSelectorAbilityItemVM.SetFavorite(m_CharGenContext.CharGenConfig.Mode == CharGenMode.LevelUp, UnitSaveData.FavoriteFeatures.Contains(charGenLevelUpSelectorAbilityItemVM.Blueprint.AssetGuidThreadSafe));
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
