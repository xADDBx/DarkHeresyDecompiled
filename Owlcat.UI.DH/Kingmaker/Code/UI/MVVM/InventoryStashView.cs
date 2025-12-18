using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InventoryStashView : View<InventoryStashVM>, IInitializable, IVendorBuyHandler, ISubscriber
{
	[SerializeField]
	private GameObject m_Background;

	[SerializeField]
	private OwlcatMultiButton m_CoinsTooltipParent;

	[SerializeField]
	private TextMeshProUGUI m_CoinsCounter;

	[SerializeField]
	protected ItemSlotsGroupView m_ItemSlotsGroup;

	[SerializeField]
	protected InsertableLootSlotsGroupView m_InsertableSlotsGroup;

	[SerializeField]
	protected ItemsFilterBaseView m_ItemsFilter;

	[SerializeField]
	private InventorySlotView m_InventorySlotPrefab;

	[SerializeField]
	private InsertableLootSlotView m_InsertableSlotPrefab;

	public void Initialize()
	{
		m_ItemSlotsGroup.Initialize(m_InventorySlotPrefab);
		m_InsertableSlotsGroup.Or(null)?.Initialize(m_InsertableSlotPrefab);
		m_ItemsFilter.Initialize();
		Hide();
	}

	protected override void OnBind()
	{
		Show();
		ISlotsGroupVM<ItemSlotVM, ItemEntity> itemSlotsGroup = base.ViewModel.ItemSlotsGroup;
		if (!(itemSlotsGroup is ItemSlotsGroupVM source))
		{
			if (itemSlotsGroup is InsertableLootSlotsGroupVM source2)
			{
				m_InsertableSlotsGroup.Bind(source2);
			}
			else
			{
				PFLog.UI.Error($"Failed to bind view to {base.ViewModel.ItemSlotsGroup.GetType()}");
			}
		}
		else
		{
			m_ItemSlotsGroup.Bind(source);
		}
		m_ItemsFilter.Bind(base.ViewModel.ItemsFilter);
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
		m_Background.Or(null)?.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
		m_Background.Or(null)?.SetActive(value: false);
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
			m_ItemSlotsGroup.ForceScrollToElement(visibleCollectionElement);
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				visibleCollectionElement.Blink();
			}).AddTo(this);
		}
	}

	public void CollectionChanged()
	{
		base.ViewModel.CollectionChanged();
	}

	public void HandleBuyItem(ItemEntity buyingItem)
	{
		TryScrollToObject(buyingItem);
	}

	public void SetFilter(ItemsFilterType type)
	{
		m_ItemsFilter.HandleFilterToggle(type);
	}
}
