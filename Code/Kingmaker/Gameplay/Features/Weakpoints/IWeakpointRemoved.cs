using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Weakpoints;

public interface IWeakpointRemoved : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleWeakpointRemoved(WeakpointSide side);
}
public interface IWeakpointRemoved<TTag> : IWeakpointRemoved, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IWeakpointRemoved, TTag>
{
}
