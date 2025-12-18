using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface INewSlotsHandler : ISubscriber
{
	void HandleTryInsertSlot(InsertableLootSlotVM slot);

	void HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to);

	void HandleTrySplitSlot(ItemSlotVM slot);
}
