using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IViewAttachedHandler : ISubscriber<IEntity>, ISubscriber
{
	void OnViewAttached(IEntityView view);
}
public interface IViewAttachedHandler<TTag> : IViewAttachedHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IViewAttachedHandler, TTag>
{
}
