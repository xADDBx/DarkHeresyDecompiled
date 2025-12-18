using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface ILootHandler : ISubscriber
{
	void HandleChangeLoot(ItemSlotVM slot);
}
