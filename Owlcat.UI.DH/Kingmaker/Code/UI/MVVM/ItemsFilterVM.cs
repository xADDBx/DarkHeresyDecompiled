using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ItemsFilterVM : ViewModel
{
	private List<ISlotsGroupVM> m_SlotsGroupVms;

	private readonly Action<bool> m_SetShowUnavailable;

	private readonly Action<ItemsFilterType> m_SetFilter;

	private readonly Action<ItemsSorterType> m_SetSorter;

	public readonly ItemsFilterSearchVM ItemsFilterSearchVM;

	private readonly ReactiveCommand<Unit> m_OnFilterReset = new ReactiveCommand<Unit>();

	public readonly OwlcatDropdownVM SorterDropdownVM;

	public Observable<Unit> OnFilterReset => m_OnFilterReset;

	public ReadOnlyReactiveProperty<bool> ShowUnavailable { get; }

	public ReadOnlyReactiveProperty<ItemsFilterType> CurrentFilter { get; }

	public ReadOnlyReactiveProperty<ItemsSorterType> CurrentSorter { get; }

	public ItemsFilterVM(ISlotsGroupVM slotsGroupVM)
		: this(new List<ISlotsGroupVM> { slotsGroupVM })
	{
	}

	public ItemsFilterVM(List<ISlotsGroupVM> slotsGroupVms)
	{
		ISlotsGroupVM slotsGroupVM = slotsGroupVms[0];
		m_SetFilter = slotsGroupVM.SetFilterType;
		m_SetSorter = slotsGroupVM.SetSorterType;
		m_SetShowUnavailable = slotsGroupVM.SetShowUnavailable;
		CurrentFilter = slotsGroupVM.FilterType;
		CurrentSorter = slotsGroupVM.SorterType;
		ShowUnavailable = slotsGroupVM.ShowUnavailable;
		ItemsFilterSearchVM = new ItemsFilterSearchVM(slotsGroupVM.SearchString, slotsGroupVM.SetSearchQuery).AddTo(this);
		CurrentFilter.Subscribe(delegate(ItemsFilterType value)
		{
			slotsGroupVms.ForEach(delegate(ISlotsGroupVM s)
			{
				s.SetFilterType(value);
			});
		}).AddTo(this);
		CurrentSorter.Subscribe(delegate(ItemsSorterType value)
		{
			slotsGroupVms.ForEach(delegate(ISlotsGroupVM s)
			{
				s.SetSorterType(value);
			});
		}).AddTo(this);
		GetSorterDropDownValues(out var itemsCollection, out var selectedId);
		SorterDropdownVM = new OwlcatDropdownVM(itemsCollection, selectedId).AddTo(this);
	}

	public void SetCurrentFilter(ItemsFilterType newFilter)
	{
		m_SetFilter(newFilter);
	}

	public void SetCurrentSorter(ItemsSorterType newSorter)
	{
		m_SetSorter(newSorter);
	}

	public void ResetFilter()
	{
		m_OnFilterReset?.Execute(Unit.Default);
	}

	public void SetShowUnavailable(bool show)
	{
		m_SetShowUnavailable(show);
	}

	private void GetSorterDropDownValues(out List<DropdownItemVM> itemsCollection, out int selectedId)
	{
		itemsCollection = new List<DropdownItemVM>();
		selectedId = 0;
		Array values = Enum.GetValues(typeof(ItemsSorterType));
		for (int i = 0; i < values.Length; i++)
		{
			ItemsSorterType itemsSorterType = (ItemsSorterType)values.GetValue(i);
			if (itemsSorterType != ItemsSorterType.CargoValue)
			{
				itemsCollection.Add(new DropdownItemVM(LocalizedTexts.Instance.ItemsFilter.GetText(itemsSorterType)));
				if (itemsSorterType == CurrentSorter.CurrentValue)
				{
					selectedId = i;
				}
			}
		}
	}
}
