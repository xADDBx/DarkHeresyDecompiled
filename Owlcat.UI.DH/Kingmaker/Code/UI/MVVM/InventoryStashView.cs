using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InventoryStashView : View<InventoryStashVM>, IInitializable, IVendorBuyHandler, ISubscriber
{
	[Serializable]
	protected class SlotGroupsData
	{
		[SerializeField]
		private SlotGroupData<ItemSlotVM> ItemSlotGroup;

		[SerializeField]
		private SlotGroupData<InsertableLootSlotVM> InsertableItemSlotGroup;

		public IScrollGroup CurrentGroup { get; private set; }

		public void Initialize()
		{
			ItemSlotGroup.Initialize();
			InsertableItemSlotGroup.Initialize();
		}

		public void Bind(ISlotsGroupVM<ItemSlotVM, ItemEntity> slots)
		{
			if (!(slots is ItemSlotsGroupVM slotsGroupVM))
			{
				if (slots is InsertableLootSlotsGroupVM slotsGroupVM2)
				{
					InsertableItemSlotGroup.Bind(slotsGroupVM2);
					CurrentGroup = InsertableItemSlotGroup;
				}
				else
				{
					PFLog.UI.Error($"Failed to bind view to {slots.GetType()}");
				}
			}
			else
			{
				ItemSlotGroup.Bind(slotsGroupVM);
				CurrentGroup = ItemSlotGroup;
			}
		}
	}

	[Serializable]
	protected class SlotGroupData<T> : IScrollGroup where T : ItemSlotVM
	{
		[SerializeField]
		private SlotsGroupView<T> m_SlotsGroupView;

		[SerializeField]
		private ItemSlotView<T> m_ItemSlotPrefab;

		public void Initialize()
		{
			m_SlotsGroupView.Or(null)?.Initialize(m_ItemSlotPrefab);
		}

		public void Bind(SlotsGroupVM<T> slotsGroupVM)
		{
			m_SlotsGroupView.Or(null)?.Bind(slotsGroupVM);
		}

		public void ForceScrollToElement(IVirtualListElementData data)
		{
			m_SlotsGroupView?.ForceScrollToElement(data);
		}
	}

	protected interface IScrollGroup
	{
		void ForceScrollToElement(IVirtualListElementData data);
	}

	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_StashTitle;

	[SerializeField]
	private TMP_Text m_CoinsCounter;

	[SerializeField]
	private OwlcatMultiButton m_CoinsTooltipParent;

	[Header("Views")]
	[SerializeField]
	protected ItemsFilterBaseView m_ItemsFilter;

	[SerializeField]
	protected SlotGroupsData SlotGroups;

	public void Initialize()
	{
		SlotGroups.Initialize();
		m_ItemsFilter.Initialize();
		Hide();
	}

	protected override void OnBind()
	{
		Show();
		SlotGroups.Bind(base.ViewModel.ItemSlotsGroup);
		m_ItemsFilter.Bind(base.ViewModel.ItemsFilter);
		m_StashTitle.Or(null)?.SetText(GetStashLabel(base.ViewModel.ItemSlotsGroup));
		base.ViewModel.Money.Subscribe(delegate(long value)
		{
			m_CoinsCounter.text = value.ToString();
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		m_CoinsTooltipParent.SetTooltip(new TooltipTemplateGlossary("Money")).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void TryScrollToObject(ItemEntity element)
	{
		if (base.ViewModel.ItemSlotsGroup.TryGetValidItem((ItemEntity i) => i?.Blueprint == element.Blueprint, out var item))
		{
			base.ViewModel.ResetFilter();
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				ScrollToElement(item);
			}).AddTo(this);
		}
	}

	public void ScrollToElement(ItemEntity element)
	{
		ISlotsGroupVM<ItemSlotVM, ItemEntity> itemSlotsGroup = base.ViewModel.ItemSlotsGroup;
		ItemSlotVM visibleCollectionElement = itemSlotsGroup.GetVisibleElementOrDefault((ItemSlotVM slot) => slot.ItemEntity?.Blueprint == element.Blueprint);
		if (visibleCollectionElement != null)
		{
			SlotGroups.CurrentGroup?.ForceScrollToElement(visibleCollectionElement);
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				visibleCollectionElement.Blink();
			}).AddTo(this);
		}
	}

	public void HandleBuyItem(ItemEntity buyingItem)
	{
		TryScrollToObject(buyingItem);
	}

	private string GetStashLabel(ISlotsGroupVM<ItemSlotVM, ItemEntity> slots)
	{
		if (!(slots is ItemSlotsGroupVM))
		{
			if (slots is InsertableLootSlotsGroupVM)
			{
				return UIStrings.Instance.LootWindow.LootSharedStash.Text;
			}
			PFLog.UI.Error($"Failed to get stash label for {slots.GetType()}");
			return string.Empty;
		}
		return UIStrings.Instance.InventoryScreen.InventorySharedStash.Text;
	}
}
