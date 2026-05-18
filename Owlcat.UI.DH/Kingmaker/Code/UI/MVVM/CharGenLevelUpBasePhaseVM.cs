using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
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

	private readonly ReactiveProperty<TSelectorItem> m_HoveredItem = new ReactiveProperty<TSelectorItem>();

	protected readonly InfoSectionVM InfoSectionVM;

	protected readonly ObservableList<TSelectorItem> Items = new ObservableList<TSelectorItem>();

	protected TooltipBaseTemplate DefaultPhaseTooltip;

	private CharGenLevelUpSelectorBaseItemVM m_LastSelectedItem;

	private readonly ReactiveProperty<TSelectorItem> m_SelectedItem = new ReactiveProperty<TSelectorItem>();

	public readonly SelectionGroupRadioVM<TSelectorItem> SelectionGroup;

	public ReadOnlyReactiveProperty<TSelectorItem> HoveredItem => m_HoveredItem;

	public ReadOnlyReactiveProperty<TSelectorItem> SelectedItem => m_SelectedItem;

	protected BaseUnitEntity Unit => m_CharGenContext.LevelUpManager.CurrentValue.PreviewUnit;

	public CharGenLevelUpBasePhaseVM(CharGenContext charGenContext, CharGenPhaseType phaseType, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, phaseType)
	{
		base.Rank = rank;
		CareerPath = Unit.Progression.AllCareerPaths?.FirstOrDefault().Blueprint;
		SelectionGroup = new SelectionGroupRadioVM<TSelectorItem>(Items, m_SelectedItem);
		InfoSectionVM = infoSectionVM;
		base.DisplayMode = ((charGenContext.CharGenConfig.Mode != CharGenMode.LevelUp) ? CharGenDisplayMode.DollOnly : CharGenDisplayMode.PortraitOnly);
		AddDisposable(SelectionGroup);
		AddDisposable(SelectedItem.Subscribe(HandleSelectedItem));
	}

	protected override bool CheckIsCompleted()
	{
		bool flag = false;
		if (SelectedItem.CurrentValue is CharGenLevelUpSelectorBaseItemVM charGenLevelUpSelectorBaseItemVM)
		{
			flag = charGenLevelUpSelectorBaseItemVM.State.CurrentValue == LEVEL_UP_ITEM_STATE.Available;
		}
		return m_IsAvailable.CurrentValue && SelectedItem.CurrentValue != null && flag;
	}

	protected override void OnBeginDetailedView()
	{
		UpdateTooltip();
	}

	protected override void OnEndDetailedView()
	{
		base.OnEndDetailedView();
		InfoSectionVM.SetTemplate(null);
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
		m_LastSelectedItem?.UpdateSelectionInParent(value: false);
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
		if (!(item is CharGenLevelUpNestedListHeaderVM))
		{
			if (item is CharGenLevelUpSelectorBaseItemVM lastSelectedItem)
			{
				m_LastSelectedItem = lastSelectedItem;
				m_LastSelectedItem?.UpdateSelectionInParent(CheckIsCompleted());
			}
			SaveSelection();
			UpdateTooltip();
			UpdateIsCompleted();
		}
	}

	protected void UpdateTooltip()
	{
		if (!base.IsInDetailedView.CurrentValue)
		{
			return;
		}
		if (HoveredItem.CurrentValue != null && !HoveredItem.CurrentValue.Equals(SelectedItem.CurrentValue) && HoveredItem.CurrentValue is CharGenLevelUpSelectorBaseItemVM charGenLevelUpSelectorBaseItemVM)
		{
			InfoSectionVM.SetTemplate(charGenLevelUpSelectorBaseItemVM.Template);
			return;
		}
		if (SelectedItem.CurrentValue is CharGenLevelUpSelectorBaseItemVM charGenLevelUpSelectorBaseItemVM2)
		{
			InfoSectionVM.SetTemplate(charGenLevelUpSelectorBaseItemVM2.Template);
			return;
		}
		if (DefaultPhaseTooltip == null)
		{
			DefaultPhaseTooltip = new TooltipTemplateLevelUpPhaseDescription(base.BlueprintSelectionWithUI);
		}
		InfoSectionVM.SetTemplate(DefaultPhaseTooltip);
	}

	protected virtual void Clear()
	{
		Items.ForEach(delegate(TSelectorItem vm)
		{
			vm.Dispose();
		});
		Items.Clear();
		m_SelectedItem.Value = null;
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Clear();
	}
}
