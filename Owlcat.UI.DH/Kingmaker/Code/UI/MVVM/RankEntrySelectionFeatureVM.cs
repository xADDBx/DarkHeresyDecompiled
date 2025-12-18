using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class RankEntrySelectionFeatureVM : BaseRankEntryFeatureVM, IVirtualListElementIdentifier
{
	protected readonly ReadOnlyReactiveProperty<SelectionStateFeature> SelectionState;

	protected readonly FeatureSelectionItem SelectionItem;

	private readonly Action<FeatureSelectionItem?> m_SelectFeatureSelectionAction;

	protected readonly RankEntrySelectionVM Owner;

	private bool m_IsSelected;

	private bool m_UnitHasFeature;

	private static List<BlueprintFact> s_UnitFacts = new List<BlueprintFact>();

	public int VirtualListTypeId
	{
		get
		{
			if (Owner.FeatureGroup != FeatureGroup.UltimateAbility)
			{
				return 0;
			}
			return 1;
		}
	}

	public bool IsCommonFeature => RankEntrySelectionFeaturesUtils.IsCommonSelectionItem(SelectionItem);

	public bool UnitHasFeature => m_UnitHasFeature;

	public RankEntrySelectionFeatureVM(RankEntrySelectionVM owner, CareerPathVM careerPathVM, FeatureSelectionItem featureSelectionItem, ReadOnlyReactiveProperty<SelectionStateFeature> selectionState, Action<FeatureSelectionItem?> selectFeature)
		: base(careerPathVM, new UIFeature(featureSelectionItem.Feature))
	{
		Owner = owner;
		SelectionItem = featureSelectionItem;
		SelectionState = selectionState;
		m_SelectFeatureSelectionAction = selectFeature;
		OverrideTooltip();
		AddDisposable(SelectionState.Subscribe(delegate
		{
			UpdateFeatureState();
		}));
		AddDisposable(owner.EntryState.Subscribe(delegate
		{
			UpdateFeatureState();
		}));
	}

	public override void Select()
	{
		if (!m_IsSelected)
		{
			m_SelectFeatureSelectionAction?.Invoke(SelectionItem);
		}
		UpdateUnitFacts(base.UnitProgressionVM?.LevelUpManager?.PreviewUnit ?? base.UnitProgressionVM?.Unit.CurrentValue);
	}

	public override bool CanSelect()
	{
		if (SelectionState.CurrentValue != null)
		{
			return SelectionState.CurrentValue.CanSelect(SelectionItem);
		}
		return false;
	}

	public void SetSelectedAndUpdate(bool isSelected)
	{
		m_IsSelected = isSelected;
		UpdateFeatureState();
	}

	protected override void UpdateFeatureState()
	{
		UpdateUnitHasFeature();
		if (m_IsSelected)
		{
			if (Owner.EntryState.CurrentValue == RankEntryState.NotValid)
			{
				m_FeatureState.Value = RankFeatureState.NotValid;
				return;
			}
			(int, int) currentLevelupRange = CareerPathVM.GetCurrentLevelupRange();
			if (Owner.Rank >= currentLevelupRange.Item1 && Owner.Rank <= currentLevelupRange.Item2)
			{
				m_FeatureState.Value = RankFeatureState.Selected;
			}
			else
			{
				m_FeatureState.Value = RankFeatureState.Committed;
			}
		}
		else if (IsReadOnlyItem())
		{
			m_FeatureState.Value = RankFeatureState.NotActive;
		}
		else if (SelectionState.CurrentValue != null)
		{
			bool flag = SelectionState.CurrentValue.CanSelect(SelectionItem);
			m_FeatureState.Value = ((!flag) ? RankFeatureState.NotSelectable : RankFeatureState.Selectable);
		}
	}

	public void UpdateStateForReadOnly()
	{
		if (!IsReadOnlyItem() || m_IsSelected)
		{
			return;
		}
		if (m_UnitHasFeature)
		{
			m_FeatureState.Value = RankFeatureState.NotSelectable;
			return;
		}
		BaseUnitEntity baseUnitEntity = base.UnitProgressionVM?.LevelUpManager?.PreviewUnit ?? base.UnitProgressionVM?.Unit.CurrentValue;
		if (baseUnitEntity != null)
		{
			CalculatedPrerequisite calculatedPrerequisite = CalculatedPrerequisite.Calculate(null, SelectionItem, baseUnitEntity);
			m_FeatureState.Value = ((calculatedPrerequisite == null || calculatedPrerequisite.Value) ? RankFeatureState.NotActive : RankFeatureState.NotSelectable);
		}
		else
		{
			m_FeatureState.Value = RankFeatureState.NotActive;
		}
	}

	private bool IsReadOnlyItem()
	{
		if (Owner.IsFirstSelectable || Owner.SelectedFeature.CurrentValue != null)
		{
			return Owner.CareerPathVM.ReadOnly.CurrentValue;
		}
		return true;
	}

	private void UpdateUnitHasFeature()
	{
		m_UnitHasFeature = s_UnitFacts.Contains(base.Feature);
	}

	public static void UpdateUnitFacts(BaseUnitEntity unit)
	{
		if (unit != null)
		{
			s_UnitFacts = unit.Facts.List.Select((EntityFact f) => f.Blueprint).ToList();
		}
	}

	private void OverrideTooltip()
	{
		BlueprintAbility abilityFromFeature = RankEntrySelectionFeaturesUtils.GetAbilityFromFeature(UIFeature.Feature);
		m_Tooltip.Value = ((abilityFromFeature != null) ? ((TooltipBaseTemplate)new TooltipTemplateRankEntryAbility(abilityFromFeature, SelectionItem, SelectionState, Owner, CareerPathVM.Unit)) : ((TooltipBaseTemplate)new TooltipTemplateRankEntryFeature(UIFeature, SelectionItem, SelectionState, Owner, CareerPathVM.Unit)));
	}
}
