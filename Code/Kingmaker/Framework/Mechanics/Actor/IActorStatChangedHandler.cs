using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Framework.Mechanics.Actor;

public interface IActorStatChangedHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleActorStatChanged(StatChangeSet stats);
}
public interface IActorStatChangedHandler<TTag> : IActorStatChangedHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IActorStatChangedHandler, TTag>, IEntitySubscriber
{
}
