using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Parts;

public interface IActionBarSlotsUpdatedHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleActionBarSlotsUpdated();
}
public interface IActionBarSlotsUpdatedHandler<TTag> : IActionBarSlotsUpdatedHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IActionBarSlotsUpdatedHandler, TTag>
{
}
