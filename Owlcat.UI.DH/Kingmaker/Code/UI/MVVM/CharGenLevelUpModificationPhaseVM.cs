using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Framework.Abilities.Components;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
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
				m_Items.Add(new CharGenLevelUpSelectorModificationItemVM(f, OnItemHovered, null, CharGenContext.LevelUpManager.CurrentValue).AddTo(this));
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
		CreateItemList();
	}

	protected override void UpdateItem(CharGenLevelUpSelectorBaseItemVM itemVM)
	{
		itemVM.RefreshView.Execute(default(Unit));
		if (itemVM is CharGenLevelUpSelectorModificationItemVM charGenLevelUpSelectorModificationItemVM)
		{
			if (SelectionFeature.CanSelect(charGenLevelUpSelectorModificationItemVM.FeatureSelectionItem))
			{
				charGenLevelUpSelectorModificationItemVM.UpdateAccessibility(LEVEL_UP_ITEM_STATE.Available);
				return;
			}
			bool flag = SelectionFeature.GetCalculatedPrerequisite(charGenLevelUpSelectorModificationItemVM.FeatureSelectionItem) is CalculatedPrerequisiteMaxRankNotReached;
			charGenLevelUpSelectorModificationItemVM.UpdateAccessibility((!flag) ? LEVEL_UP_ITEM_STATE.NotAvailable : LEVEL_UP_ITEM_STATE.AlreadyExist);
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
				select i).ForEach(delegate(CharGenLevelUpSelectorModificationItemVM f)
			{
				f.SetParentNode(header);
				AddItem(f);
			});
		}
	}

	private void CreateListByType()
	{
		BpRef<BlueprintAbilityTag>[] allTags = ConfigRoot.Instance.AbilityRoot.AllTags;
		foreach (BpRef<BlueprintAbilityTag> tag in allTags)
		{
			CharGenLevelUpNestedListHeaderVM header = new CharGenLevelUpNestedListHeaderVM(null, tag.Blueprint.Name).AddTo(this);
			if (!(from i in SelectionFeature.Items
				where IsSameTags(i.Feature, tag)
				orderby i.Feature.Name
				select i).ToList().Empty())
			{
				AddItem(header);
				(from i in m_Items
					where IsSameTags(i.FeatureSelectionItem.Feature, tag)
					orderby i.State.CurrentValue, i.FeatureSelectionItem.Feature.Name
					select i).ForEach(delegate(CharGenLevelUpSelectorModificationItemVM f)
				{
					f.SetParentNode(header);
					AddItem(f);
				});
			}
		}
	}

	private bool IsSameTags(BlueprintFeature feature, BlueprintAbilityTag tag)
	{
		if (feature.ComponentsArray.FirstOrDefault((BlueprintComponent c) => c is AddAvailableAbilityModifier) is AddAvailableAbilityModifier addAvailableAbilityModifier && addAvailableAbilityModifier.Modifier != null && addAvailableAbilityModifier.Modifier.Blueprint.Tags.Count() > 0)
		{
			return addAvailableAbilityModifier.Modifier.Blueprint.Tags.First() == tag;
		}
		return false;
	}
}
