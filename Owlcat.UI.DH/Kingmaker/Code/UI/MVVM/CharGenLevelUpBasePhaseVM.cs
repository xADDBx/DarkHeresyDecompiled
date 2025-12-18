using System.Linq;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Progression.Paths;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpBasePhaseVM<TSelectorItem> : CharGenPhaseBaseVM where TSelectorItem : SelectionGroupEntityVM
{
	protected readonly BlueprintCareerPath CareerPath;

	protected readonly TooltipBaseTemplate DefaultPhaseTooltip;

	private readonly ReactiveProperty<TSelectorItem> m_HoveredItem = new ReactiveProperty<TSelectorItem>();

	protected readonly InfoSectionVM InfoSectionVM;

	protected readonly ObservableList<TSelectorItem> Items = new ObservableList<TSelectorItem>();

	private CharGenLevelUpSelectorBaseItemVM m_LastSelectedItem;

	private readonly ReactiveProperty<TSelectorItem> m_SelectedItem = new ReactiveProperty<TSelectorItem>();

	public readonly SelectionGroupRadioVM<TSelectorItem> SelectionGroup;

	public ReadOnlyReactiveProperty<TSelectorItem> HoveredItem => m_HoveredItem;

	public ReadOnlyReactiveProperty<TSelectorItem> SelectedItem => m_SelectedItem;

	protected BaseUnitEntity Unit => CharGenContext.LevelUpManager.CurrentValue.PreviewUnit;

	public CharGenLevelUpBasePhaseVM(CharGenContext charGenContext, CharGenPhaseType phaseType, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, phaseType)
	{
		base.Rank = rank;
		CareerPath = Unit.Progression.AllCareerPaths?.FirstOrDefault().Blueprint;
		SelectionGroup = new SelectionGroupRadioVM<TSelectorItem>(Items, m_SelectedItem);
		InfoSectionVM = infoSectionVM;
		DefaultPhaseTooltip = new TooltipTemplateLevelUpPhaseDescription(phaseType);
		AddDisposable(SelectionGroup);
		AddDisposable(SelectedItem.Subscribe(HandleSelectedItem));
		Metrics.Interface.InterfaceType(InterfaceMetricsEvent.InterfaceTypes.LevelUp).InterfaceState(InterfaceMetricsEvent.InterfaceStates.Open).Send();
	}

	protected override bool CheckIsCompleted()
	{
		bool flag = false;
		if (SelectedItem.CurrentValue is CharGenLevelUpSelectorBaseItemVM charGenLevelUpSelectorBaseItemVM)
		{
			flag = charGenLevelUpSelectorBaseItemVM.State.CurrentValue == LEVEL_UP_ITEM_STATE.Available;
		}
		return SelectedItem.CurrentValue != null && flag;
	}

	protected override void OnBeginDetailedView()
	{
		UpdateTooltip();
	}

	protected virtual void OnItemHovered(SelectionGroupEntityVM item)
	{
		m_HoveredItem.Value = item as TSelectorItem;
		UpdateTooltip();
	}

	protected virtual void AddItem(TSelectorItem item)
	{
		AddDisposable(item);
		Items.Add(item);
	}

	protected virtual void SaveSelection()
	{
	}

	protected virtual void CreateItemList()
	{
	}

	protected void TrySelectItem()
	{
		if (SelectedItem.CurrentValue == null)
		{
			SelectionGroup.TrySelectFirstValidEntity();
		}
	}

	protected virtual void HandleSelectedItem(TSelectorItem item)
	{
		UpdateIsCompleted();
		m_LastSelectedItem?.UpdateSelectionInParent(value: false);
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
		UpdateTooltip();
		if (!(item is CharGenLevelUpNestedListHeaderVM))
		{
			if (item is CharGenLevelUpSelectorBaseItemVM lastSelectedItem)
			{
				m_LastSelectedItem = lastSelectedItem;
				m_LastSelectedItem?.UpdateSelectionInParent(CheckIsCompleted());
			}
			SaveSelection();
		}
	}

	protected void UpdateTooltip()
	{
		if (HoveredItem.CurrentValue is CharGenLevelUpSelectorBaseItemVM charGenLevelUpSelectorBaseItemVM)
		{
			InfoSectionVM.SetTemplate(charGenLevelUpSelectorBaseItemVM.Template);
		}
		else if (SelectedItem.CurrentValue is CharGenLevelUpSelectorBaseItemVM charGenLevelUpSelectorBaseItemVM2)
		{
			InfoSectionVM.SetTemplate(charGenLevelUpSelectorBaseItemVM2.Template);
		}
		else
		{
			InfoSectionVM.SetTemplate(DefaultPhaseTooltip);
		}
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
	}
}
