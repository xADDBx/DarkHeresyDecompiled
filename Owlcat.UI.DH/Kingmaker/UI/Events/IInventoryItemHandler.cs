using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface IInventoryItemHandler : ISubscriber
{
	void HandleChangeItem(EquipSlotVM slot);
}
