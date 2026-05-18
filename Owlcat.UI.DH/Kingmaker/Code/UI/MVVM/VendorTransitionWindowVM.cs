using System;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorTransitionWindowVM : ViewModel, IVendorDealHandler, ISubscriber
{
	public readonly int MaxValue;

	public readonly ItemSlotVM Slot;

	private readonly Action m_Close;

	public int CurrentValue;

	public VendorTransitionWindowVM(TradeLogic _, ItemEntity itemEntity, Action close)
	{
		m_Close = close;
		Slot = new ItemSlotVM(itemEntity, 0, null, compareTooltipEnabled: false).AddTo(this);
		if (itemEntity != null)
		{
			MaxValue = itemEntity.Count;
		}
		if (itemEntity != null)
		{
			CurrentValue = ((!itemEntity.Collection.IsPlayerInventory) ? 1 : itemEntity.Count);
		}
		EventBus.Subscribe(this).AddTo(this);
	}

	public void Deal()
	{
		if (Slot == null)
		{
			Close();
			return;
		}
		if (Slot.ItemEntity.Collection.IsPlayerInventory)
		{
			Game.Instance.GameCommandQueue.AddForSellVendor(Slot.ItemEntity, CurrentValue);
		}
		else
		{
			Game.Instance.GameCommandQueue.AddForBuyVendor(Slot.ItemEntity, CurrentValue);
		}
		FullScreenSounds.Instance.Vendor.MoveFromVendorToTrade.Play();
		Close();
	}

	public void Close()
	{
		m_Close?.Invoke();
	}

	public void HandleVendorDeal()
	{
		Close();
	}

	public void HandleCancelVendorDeal()
	{
	}
}
