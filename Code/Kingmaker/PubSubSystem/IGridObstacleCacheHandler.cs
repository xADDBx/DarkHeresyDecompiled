using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGridObstacleCacheHandler : ISubscriber
{
	void HandleGridObstacleCacheUpdated();
}
