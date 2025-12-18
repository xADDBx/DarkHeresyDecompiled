using System;
using System.Collections.Generic;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SelectorWindowVM<TEntityVM> : SelectionGroupRadioVM<TEntityVM> where TEntityVM : SelectionGroupEntityVM
{
	public readonly InfoSectionVM InfoSectionVM;

	public TEntityVM CurrentSelected;

	private readonly Action<TEntityVM> m_OnConfirm;

	private readonly Action m_OnDecline;

	public readonly EquipSlotVM Slot;

	public SelectorWindowVM(Action<TEntityVM> onConfirm, Action onDecline, ObservableList<TEntityVM> entitiesCollection, ReactiveProperty<TEntityVM> entity = null, EquipSlotVM equippedSlot = null)
		: base(entitiesCollection, entity, cyclical: false)
	{
		m_OnConfirm = onConfirm;
		m_OnDecline = onDecline;
		Slot = equippedSlot;
		AddDisposable(InfoSectionVM = new InfoSectionVM());
	}

	public SelectorWindowVM(Action<TEntityVM> onConfirm, Action onDecline, List<TEntityVM> visibleCollection, ReactiveProperty<TEntityVM> entity = null, EquipSlotVM equippedSlot = null)
		: base(visibleCollection, entity, cyclical: false)
	{
		m_OnConfirm = onConfirm;
		m_OnDecline = onDecline;
		Slot = equippedSlot;
		AddDisposable(InfoSectionVM = new InfoSectionVM());
	}

	public void Confirm(TEntityVM entityVM)
	{
		m_OnConfirm?.Invoke(entityVM);
	}

	public void Back()
	{
		m_OnDecline?.Invoke();
	}

	public void SetCurrentSelected(TEntityVM entity)
	{
		CurrentSelected = entity;
	}
}
