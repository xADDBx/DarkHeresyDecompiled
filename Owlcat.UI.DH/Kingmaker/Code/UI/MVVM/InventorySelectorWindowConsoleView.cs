using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Items;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InventorySelectorWindowConsoleView : SelectorWindowConsoleView<EquipSelectionSlotConsoleView, EquipSelectorSlotVM>
{
	private Coroutine m_UnequippedItemTooltipCo;

	protected override LocalizedString ConfirmText => UIStrings.Instance.ContextMenu.Equip;

	protected override bool ShouldCloseOnConfirm => false;

	private InventorySelectorWindowVM InventorySelectorWindowVM => base.ViewModel as InventorySelectorWindowVM;

	protected override void OnBind()
	{
		base.OnBind();
		m_Header.text = UIStrings.Instance.InventoryScreen.ChooseItem;
		m_SelectedEquipped.Subscribe(delegate(bool value)
		{
			CanEquip.Value = !value && TakeControllableCharacter();
		}).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Unequip();
		}, 11, m_SelectedEquipped), UIStrings.Instance.ContextMenu.TakeOff).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		if (m_UnequippedItemTooltipCo != null)
		{
			StopCoroutine(m_UnequippedItemTooltipCo);
			m_UnequippedItemTooltipCo = null;
		}
	}

	protected override void EntityFocused(IConsoleEntity entity)
	{
		base.EntityFocused(entity);
		RefreshSelectedState(entity);
		if (m_UnequippedItemTooltipCo != null)
		{
			StopCoroutine(m_UnequippedItemTooltipCo);
		}
	}

	protected void Unequip()
	{
		InventorySelectorWindowVM?.Unequip();
		if (m_NavigationBehaviour.DeepestNestedFocus is EquipSelectionSlotConsoleView equipSelectionSlotConsoleView)
		{
			m_UnequippedItemTooltipCo = StartCoroutine(UpdateUnequippedItemTooltipCo(equipSelectionSlotConsoleView.EquipVM.Item));
		}
	}

	private void RefreshSelectedState(IConsoleEntity entity)
	{
		m_SelectedEquipped.Value = CanUnequip();
		bool CanUnequip()
		{
			if (entity is EquipSelectionSlotConsoleView equipSelectionSlotConsoleView && equipSelectionSlotConsoleView.EquipVM.Item.Owner != null)
			{
				return equipSelectionSlotConsoleView.EquipVM.Item.Owner.CanBeControlled();
			}
			return false;
		}
	}

	private IEnumerator UpdateUnequippedItemTooltipCo(ItemEntity item)
	{
		while (item.Owner != null)
		{
			yield return null;
		}
		if (m_NavigationBehaviour.DeepestNestedFocus is EquipSelectionSlotConsoleView equipSelectionSlotConsoleView && equipSelectionSlotConsoleView.EquipVM.Item == item)
		{
			equipSelectionSlotConsoleView.EquipVM.RefreshTooltip(forceUpdate: true);
			EntityFocused(equipSelectionSlotConsoleView);
		}
	}
}
