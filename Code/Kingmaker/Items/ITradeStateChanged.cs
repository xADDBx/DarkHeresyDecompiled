using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Items;

public interface ITradeStateChanged : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleBeginTrading();

	void HandleEndTrading();

	void HandleVendorAboutToTrading();
}
