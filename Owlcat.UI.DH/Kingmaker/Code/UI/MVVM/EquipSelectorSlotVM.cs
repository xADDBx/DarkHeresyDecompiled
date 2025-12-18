using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EquipSelectorSlotVM : SelectionGroupEntityVM
{
	public readonly Sprite Icon;

	public readonly string DisplayName;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<int> m_UsableCount = new ReactiveProperty<int>(0);

	public readonly ItemEntity Item;

	private readonly ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> Tooltip => m_Tooltip;

	public ReadOnlyReactiveProperty<int> UsableCount => m_UsableCount;

	public EquipSelectorSlotVM(ItemEntity item)
		: base(allowSwitchOff: false)
	{
		AddDisposable(EventBus.Subscribe(this));
		Item = item;
		Icon = item.Icon;
		m_UsableCount.Value = item.Charges;
		DisplayName = item.Name;
		AddDisposable(RefreshView.Subscribe(UnSelect));
		RefreshTooltip();
	}

	protected override void DoSelectMe()
	{
		m_IsSelected.Value = true;
	}

	private void UnSelect()
	{
		m_IsSelected.Value = false;
	}

	public void RefreshTooltip(bool forceUpdate = false)
	{
		m_Tooltip.Value = new TooltipTemplateItem(Item, null, forceUpdate, replenishing: false, null, isScreenTooltip: true);
	}
}
