using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IItemChargesHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleItemChargeSpent(ItemEntity item);
}
public interface IItemChargesHandler<TTag> : IItemChargesHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<IItemChargesHandler, TTag>
{
}
