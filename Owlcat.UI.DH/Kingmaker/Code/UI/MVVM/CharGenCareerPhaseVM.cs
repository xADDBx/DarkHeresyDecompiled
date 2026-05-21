using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Paths;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenCareerPhaseVM : CharGenPhaseBaseVM
{
	private readonly ObservableList<CharGenCareerSelectionItemVM> m_Items = new ObservableList<CharGenCareerSelectionItemVM>();

	private readonly ReactiveProperty<CharGenCareerSelectionItemVM> m_SelectedItem = new ReactiveProperty<CharGenCareerSelectionItemVM>();

	public readonly SelectionGroupRadioVM<CharGenCareerSelectionItemVM> SelectionGroup;

	private readonly SelectionStateFeature m_SelectionState;

	private CharGenCareerSelectionItemVM m_HoveredItem;

	private BaseUnitEntity m_Unit => m_CharGenContext.CurrentUnit.CurrentValue;

	public ReadOnlyReactiveProperty<CharGenCareerSelectionItemVM> SelectedItem => m_SelectedItem;

	public CharGenCareerPhaseVM(CharGenContext charGenContext, InfoSectionVM infoSectionVM, SelectionStateFeature selectionState)
		: base(charGenContext, CharGenPhaseType.Career, allowSwitchOff: true)
	{
		m_SelectionState = selectionState;
		m_PhaseName.Value = selectionState.Blueprint.Title;
		base.DisplayMode = CharGenDisplayMode.PortraitOnly;
		base.BlueprintSelectionWithUI = selectionState.Blueprint;
		SetPhaseHint(base.BlueprintSelectionWithUI?.CallToAction?.Text ?? string.Empty);
		InfoVM = infoSectionVM;
		SelectionGroup = new SelectionGroupRadioVM<CharGenCareerSelectionItemVM>(m_Items, m_SelectedItem).AddTo(this);
		SelectedItem.Subscribe(HandleSelectedItem).AddTo(this);
		RefreshItemList();
	}

	protected override bool CheckIsCompleted()
	{
		if (m_IsAvailable.CurrentValue && m_SelectionState.IsMade)
		{
			return m_SelectionState.IsValid;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
		UpdateTooltip();
	}

	protected override void OnEndDetailedView()
	{
		base.OnEndDetailedView();
		InfoVM.SetTemplate(null);
	}

	private void RefreshItemList()
	{
		List<BlueprintCareerPath> paths = m_SelectionState.Items.Select((FeatureSelectionItem i) => i.Feature).OfType<BlueprintCareerPath>().ToList();
		foreach (CharGenCareerSelectionItemVM item in m_Items.Where((CharGenCareerSelectionItemVM i) => !paths.Contains(i.CareerPath)).ToList())
		{
			m_Items.Remove(item);
		}
		foreach (BlueprintCareerPath path in paths)
		{
			if (!m_Items.Any((CharGenCareerSelectionItemVM i) => i.CareerPath == path))
			{
				CharGenCareerSelectionItemVM charGenCareerSelectionItemVM = new CharGenCareerSelectionItemVM(path, OnHoverItem, base.BlueprintSelectionWithUI);
				AddDisposable(charGenCareerSelectionItemVM);
				m_Items.Add(charGenCareerSelectionItemVM);
			}
		}
	}

	private void HandleSelectedItem(CharGenCareerSelectionItemVM item)
	{
		if (m_CharGenContext.CharGenConfig.Mode == CharGenMode.LevelUp)
		{
			m_CharGenContext.LevelUpManager.CurrentValue.SelectCareerPath(item.CareerPath);
		}
		else
		{
			m_SelectionState.ClearSelection();
			if (item != null)
			{
				FeatureSelectionItem selectionItem = m_SelectionState.Items.First((FeatureSelectionItem f) => f.Feature == item.CareerPath);
				m_SelectionState.Select(selectionItem);
			}
		}
		UpdateIsCompleted();
		UpdateTooltip();
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectCareerPath();
		});
	}

	private void OnHoverItem(CharGenCareerSelectionItemVM item)
	{
		m_HoveredItem = item;
		UpdateTooltip();
	}

	private void UpdateTooltip()
	{
		if (base.IsInDetailedView.CurrentValue)
		{
			CharGenCareerSelectionItemVM obj = m_HoveredItem ?? SelectedItem.CurrentValue;
			InfoVM.SetTemplate(obj?.Template ?? new TooltipTemplateLevelUpPhaseDescription(base.BlueprintSelectionWithUI));
		}
	}
}
