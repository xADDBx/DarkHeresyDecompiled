using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class SlotsGroupVM<TViewModel> : ViewModel, ISlotsGroupVM<TViewModel, ItemEntity>, ISlotsGroupVM, IDisposable, ITransferItemHandler, ISubscriber, ICollectLootHandler where TViewModel : ItemSlotVM
{
	private struct EntityIndexPair
	{
		public ItemEntity Item;

		public int Index;
	}

	private readonly ReactiveProperty<ItemsFilterType> m_FilterType = new ReactiveProperty<ItemsFilterType>();

	private readonly ReactiveProperty<ItemsSorterType> m_SorterType = new ReactiveProperty<ItemsSorterType>();

	private readonly ReactiveProperty<bool> m_ShowUnavailable = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<string> m_SearchString = new ReactiveProperty<string>();

	private readonly ReactiveCommand<Unit> m_CollectionChangedCommand = new ReactiveCommand<Unit>();

	private IEnumerable<ItemEntity> m_ItemEntities;

	private readonly Func<ItemEntity, bool> m_ShowPredicate;

	private readonly int m_MinSlots;

	private readonly int? m_MaxSlots;

	private readonly int m_SlotsInRow;

	private readonly bool m_ShowSlotHoldItems;

	private readonly List<ItemEntity> m_SortedItemsCache = new List<ItemEntity>();

	private readonly List<EntityIndexPair> m_EntityIndexPairsCache = new List<EntityIndexPair>();

	private readonly bool m_RemoveEmptySlots;

	private readonly Func<ItemsSorterType, ItemsFilterType, Comparison<ItemEntity>> m_GetItemsComparison;

	private static readonly Comparison<ItemEntity> m_CompareBySlotIndex = (ItemEntity a, ItemEntity b) => a.InventorySlotIndex.CompareTo(b.InventorySlotIndex);

	public ItemsCollection MechanicCollection { get; }

	public ObservableList<TViewModel> VisibleCollection { get; } = new ObservableList<TViewModel>();


	public ReadOnlyReactiveProperty<ItemsFilterType> FilterType => m_FilterType;

	public ReadOnlyReactiveProperty<ItemsSorterType> SorterType => m_SorterType;

	public ReadOnlyReactiveProperty<bool> ShowUnavailable => m_ShowUnavailable;

	public ReadOnlyReactiveProperty<string> SearchString => m_SearchString;

	public Observable<Unit> CollectionChangedCommand => m_CollectionChangedCommand;

	public List<ItemEntity> Items => m_ItemEntities?.ToList() ?? MechanicCollection?.Items.ToTempList() ?? new List<ItemEntity>();

	public List<ItemEntity> ValidItems => Items.Where(ShouldShowItem).ToList();

	public ItemSlotsGroupType Type { get; }

	protected SlotsGroupVM(ItemsCollection collection, int slotsInRow, int minSlots, int? maxSlots = null, IEnumerable<ItemEntity> items = null, ItemsSortingContext sortingCtx = default(ItemsSortingContext), bool showUnavailableItems = true, bool showSlotHoldItemsInSlots = false, ItemSlotsGroupType type = ItemSlotsGroupType.Unknown, Func<ItemEntity, bool> showPredicate = null)
	{
		MechanicCollection = collection;
		m_ItemEntities = items;
		Type = type;
		m_ShowPredicate = showPredicate;
		m_MinSlots = minSlots;
		m_MaxSlots = maxSlots;
		m_SlotsInRow = slotsInRow;
		m_ShowSlotHoldItems = showSlotHoldItemsInSlots;
		m_SorterType.Value = sortingCtx.SorterType;
		m_FilterType.Value = sortingCtx.FilterType;
		m_GetItemsComparison = sortingCtx.GetComparisonFunc ?? new Func<ItemsSorterType, ItemsFilterType, Comparison<ItemEntity>>(ItemsFilter.GetItemsDefaultComparison);
		m_RemoveEmptySlots = sortingCtx.RemoveEmptySlots;
		m_ShowUnavailable.Value = showUnavailableItems;
		FilterType.Skip(1).Subscribe(delegate
		{
			UpdateVisibleCollection();
		}).AddTo(this);
		SorterType.Skip(1).Subscribe(delegate
		{
			UpdateVisibleCollection();
		}).AddTo(this);
		ShowUnavailable.Skip(1).Subscribe(delegate
		{
			UpdateVisibleCollection();
		}).AddTo(this);
		SearchString.Skip(1).Subscribe(delegate
		{
			UpdateVisibleCollection();
		}).AddTo(this);
		UpdateVisibleCollection();
		EventBus.Subscribe(this).AddTo(this);
	}

	public void SetNewItems(IEnumerable<ItemEntity> items)
	{
		m_ItemEntities = items;
		UpdateVisibleCollection();
	}

	public void UpdateVisibleCollection()
	{
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame().DebounceFrame(0), delegate
		{
			InternalUpdate();
		}).AddTo(this);
	}

	public TViewModel GetFirstEmptySlot()
	{
		foreach (TViewModel item in VisibleCollection)
		{
			if (!item.HasItem)
			{
				return item;
			}
		}
		return null;
	}

	public bool TryGetValidItem(Func<ItemEntity, bool> predicate, out ItemEntity result)
	{
		foreach (ItemEntity validItem in ValidItems)
		{
			if (predicate(validItem))
			{
				result = validItem;
				return true;
			}
		}
		result = null;
		return false;
	}

	public TViewModel GetVisibleElementOrDefault(Func<TViewModel, bool> predicate)
	{
		foreach (TViewModel item in VisibleCollection)
		{
			if (predicate(item))
			{
				return item;
			}
		}
		return null;
	}

	public void SetFilterType(ItemsFilterType filter)
	{
		m_FilterType.Value = filter;
	}

	public void SetSorterType(ItemsSorterType sorter)
	{
		m_SorterType.Value = sorter;
	}

	public void SetSearchQuery(string query)
	{
		m_SearchString.Value = query;
	}

	public void SetShowUnavailable(bool show)
	{
		m_ShowUnavailable.Value = show;
	}

	public void TriggerCollectionChanged()
	{
		m_CollectionChangedCommand.Execute();
	}

	private void InternalUpdate()
	{
		m_SortedItemsCache.Clear();
		m_EntityIndexPairsCache.Clear();
		foreach (ItemEntity item in Items)
		{
			if (ShouldShowItem(item))
			{
				m_SortedItemsCache.Add(item);
			}
		}
		Comparison<ItemEntity> comparison = m_GetItemsComparison(SorterType.CurrentValue, FilterType.CurrentValue);
		m_SortedItemsCache.Sort(comparison ?? m_CompareBySlotIndex);
		bool insertEmptySlots = !NeedRemoveEmptySlot() && comparison == null;
		bool flag = SorterType.CurrentValue != ItemsSorterType.NotSorted;
		int num = -1;
		for (int i = 0; i < m_SortedItemsCache.Count; i++)
		{
			ItemEntity itemEntity = m_SortedItemsCache[i];
			if (flag)
			{
				itemEntity.SetSlotIndex(i);
			}
			if (TryAddItemIndexPair(itemEntity, insertEmptySlots, in m_EntityIndexPairsCache))
			{
				num = itemEntity.InventorySlotIndex;
			}
		}
		int b = m_MaxSlots ?? (m_SlotsInRow * (Mathf.CeilToInt((float)m_EntityIndexPairsCache.Count / (float)m_SlotsInRow) + 1));
		int num2 = Mathf.Max(m_MinSlots, b);
		while (m_EntityIndexPairsCache.Count < num2)
		{
			num++;
			m_EntityIndexPairsCache.Add(new EntityIndexPair
			{
				Item = null,
				Index = num
			});
		}
		SetNewVisibleCollection(m_EntityIndexPairsCache);
	}

	private bool TryAddItemIndexPair(ItemEntity item, bool insertEmptySlots, in List<EntityIndexPair> listToFill)
	{
		if (IsExcludedByFilters(item) || IsExcludedByAvailability(item))
		{
			return false;
		}
		int inventorySlotIndex = item.InventorySlotIndex;
		if (!insertEmptySlots)
		{
			listToFill.Add(new EntityIndexPair
			{
				Item = item,
				Index = inventorySlotIndex
			});
			return true;
		}
		int num;
		if (listToFill.Count <= 0)
		{
			num = 0;
		}
		else
		{
			List<EntityIndexPair> obj = listToFill;
			num = obj[obj.Count - 1].Index + 1;
		}
		for (int i = num; i < inventorySlotIndex; i++)
		{
			listToFill.Add(new EntityIndexPair
			{
				Item = null,
				Index = i
			});
		}
		listToFill.Add(new EntityIndexPair
		{
			Item = item,
			Index = inventorySlotIndex
		});
		return true;
	}

	protected virtual bool ShouldShowItem(ItemEntity item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.InventorySlotIndex < 0)
		{
			return false;
		}
		if (!item.IsLootable)
		{
			return false;
		}
		if (!m_ShowSlotHoldItems)
		{
			return item.HoldingSlot == null;
		}
		return true;
	}

	private bool IsExcludedByFilters(ItemEntity item)
	{
		if (item == null)
		{
			return true;
		}
		if (!ItemsFilter.ShouldShowItem(item, FilterType.CurrentValue))
		{
			return true;
		}
		if (!ItemsFilter.IsMatchSearchRequest(item, SearchString.CurrentValue))
		{
			return true;
		}
		if (m_ShowPredicate != null)
		{
			return !m_ShowPredicate(item);
		}
		return false;
	}

	private bool IsExcludedByAvailability(ItemEntity item)
	{
		if (!ShowUnavailable.CurrentValue)
		{
			return !UIUtilityItem.IsEquipPossible(item);
		}
		return false;
	}

	private bool NeedRemoveEmptySlot()
	{
		if (!m_RemoveEmptySlots && FilterType.CurrentValue == ItemsFilterType.NoFilter && SorterType.CurrentValue == ItemsSorterType.NotSorted && string.IsNullOrEmpty(SearchString.CurrentValue) && m_ShowPredicate == null)
		{
			return !ShowUnavailable.CurrentValue;
		}
		return true;
	}

	private void SetNewVisibleCollection(IReadOnlyList<EntityIndexPair> newCollection)
	{
		if (newCollection.Empty())
		{
			VisibleCollection.Clear();
			return;
		}
		for (int i = 0; i < newCollection.Count; i++)
		{
			EntityIndexPair entityIndexPair = newCollection[i];
			if (VisibleCollection.Count <= i)
			{
				VisibleCollection.Add(GetVirtualSlot(entityIndexPair.Item, entityIndexPair.Index));
				continue;
			}
			VisibleCollection[i].Index = entityIndexPair.Index;
			VisibleCollection[i].SetItem(entityIndexPair.Item);
		}
		while (VisibleCollection.Count > newCollection.Count)
		{
			int index = VisibleCollection.Count - 1;
			VisibleCollection[index].Dispose();
			VisibleCollection.RemoveAt(index);
		}
		m_CollectionChangedCommand.Execute();
	}

	private TViewModel GetVirtualSlot(ItemEntity item, int index)
	{
		return GetItem(item, index).AddTo(this);
	}

	protected abstract TViewModel GetItem(ItemEntity item, int index);

	void ITransferItemHandler.HandleTransferItem(ItemsCollection from, ItemsCollection to)
	{
		if (from == MechanicCollection || to == MechanicCollection)
		{
			UpdateVisibleCollection();
		}
	}

	void ICollectLootHandler.HandleCollectAll(ItemsCollection from, ItemsCollection to)
	{
		if (from == MechanicCollection || to == MechanicCollection)
		{
			UpdateVisibleCollection();
		}
	}
}
