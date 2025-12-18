using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Weakpoints;

public interface IWeakpointAdded : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleWeakpointAdded(WeakpointSide side);
}
public interface IWeakpointAdded<TTag> : IWeakpointAdded, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IWeakpointAdded, TTag>
{
}
