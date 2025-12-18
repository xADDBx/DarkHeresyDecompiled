using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Cohesion;

public interface IEntityEndTurnInCohesionRangeHandler : ISubscriber<IUnitEntity>, ISubscriber
{
	void HandleEntityEndTurnInCohesionRange(MechanicEntity entity);
}
public interface IEntityEndTurnInCohesionRangeHandler<TTag> : IEntityEndTurnInCohesionRangeHandler, ISubscriber<IUnitEntity>, ISubscriber, IEventTag<IEntityEndTurnInCohesionRangeHandler, TTag>
{
}
