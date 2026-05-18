using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IInventoryUIHandler : ISubscriber
{
	void HandleOpenInventory();
}
