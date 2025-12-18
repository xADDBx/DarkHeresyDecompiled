using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDestructibleHoverUIHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleHoverChange(bool isHover);
}
public interface IDestructibleHoverUIHandler<TTag> : IDestructibleHoverUIHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<IDestructibleHoverUIHandler, TTag>
{
}
