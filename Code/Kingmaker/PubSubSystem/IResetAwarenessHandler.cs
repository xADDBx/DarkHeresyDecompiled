using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IResetAwarenessHandler : ISubscriber<IMapObjectEntity>, ISubscriber
{
	void HandleAwarenessCheckReset();
}
public interface IResetAwarenessHandler<TTag> : IResetAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IEventTag<IResetAwarenessHandler, TTag>
{
}
