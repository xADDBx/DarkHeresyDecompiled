using System;
using Kingmaker.Items;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InsertableLootSlotVM : ItemSlotVM
{
	private readonly ReactiveProperty<bool> m_CanInsert = new ReactiveProperty<bool>();

	private readonly Func<ItemEntity, bool> m_CanInsertItem;

	public ReadOnlyReactiveProperty<bool> CanInsert => m_CanInsert;

	public bool CanTransfer
	{
		get
		{
			if (CanInsert.CurrentValue)
			{
				return base.HasItem;
			}
			return false;
		}
	}

	public InsertableLootSlotVM(ItemEntity item, int index, ISlotsGroupVM group = null, Func<ItemEntity, bool> canInsertItem = null)
		: base(item, index, group)
	{
		m_CanInsertItem = canInsertItem;
		UpdateCanInsert();
	}

	protected override void ItemChangedHandler(ItemEntity item)
	{
		base.ItemChangedHandler(item);
		UpdateCanInsert();
	}

	public void UpdateCanInsert()
	{
		ItemEntity currentValue = base.Item.CurrentValue;
		m_CanInsert.Value = !base.HasItem || (m_CanInsertItem?.Invoke(currentValue) ?? true);
	}
}
