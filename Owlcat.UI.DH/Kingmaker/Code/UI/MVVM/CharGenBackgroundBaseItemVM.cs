using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenBackgroundBaseItemVM : SelectionGroupEntityVM
{
	private readonly FeatureSelectionItem m_SelectionItem;

	private readonly SelectionStateFeature m_SelectionStateFeature;

	private readonly Action<CharGenBackgroundBaseItemVM> m_OnHover;

	protected readonly ReactiveProperty<LEVEL_UP_ITEM_STATE> m_State = new ReactiveProperty<LEVEL_UP_ITEM_STATE>(LEVEL_UP_ITEM_STATE.Available);

	public readonly CharGenPhaseType PhaseType;

	public ReadOnlyReactiveProperty<LEVEL_UP_ITEM_STATE> State => m_State;

	public TooltipBaseTemplate Template { get; protected set; }

	public BlueprintFeature Feature => m_SelectionItem.Feature;

	public FeatureSelectionItem FeatureSelectionItem => m_SelectionItem;

	public CharGenBackgroundBaseItemVM(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType, Action<CharGenBackgroundBaseItemVM> onHover = null)
		: base(allowSwitchOff: true)
	{
		PhaseType = phaseType;
		m_SelectionItem = selectionItem;
		m_SelectionStateFeature = selectionStateFeature;
		m_OnHover = onHover;
	}

	protected override void DoSelectMe()
	{
	}

	public void SetHovered(bool value)
	{
		m_OnHover(value ? this : null);
	}

	public void ApplySelection()
	{
		m_SelectionStateFeature?.ClearSelection();
		m_SelectionStateFeature?.Select(m_SelectionItem);
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
	}

	public virtual void UpdateAccessibility(LEVEL_UP_ITEM_STATE value)
	{
		m_State.Value = value;
	}
}
