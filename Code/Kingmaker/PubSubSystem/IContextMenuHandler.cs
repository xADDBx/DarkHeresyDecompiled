using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IContextMenuHandler : ISubscriber
{
	void HandleContextMenuRequest(IContextMenuCollection collection);
}
