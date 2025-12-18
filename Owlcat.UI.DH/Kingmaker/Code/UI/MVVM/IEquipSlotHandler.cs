using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface IEquipSlotHandler : ISubscriber
{
	void ChooseSlotToItem(InventorySlotConsoleView item);
}
