using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IInventoryNotificationHandler : ISubscriber
{
	void HandleWarning(string text);
}
