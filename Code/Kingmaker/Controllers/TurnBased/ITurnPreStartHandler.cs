using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface ITurnPreStartHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitPreStartTurn(bool isTurnBased);
}
public interface ITurnPreStartHandler<TTag> : ITurnPreStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ITurnPreStartHandler, TTag>
{
}
