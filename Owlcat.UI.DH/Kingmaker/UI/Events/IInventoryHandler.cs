using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface IInventoryHandler : ISubscriber
{
	void Refresh();

	void TryEquip(ItemSlotVM slot);

	void TryDrop(ItemSlotVM slot);

	void TryMoveToInventory(ItemSlotVM slot, bool immediately = false);
}
