using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Cohesion;

public interface IEntityExitCohesionRangeHandler : ISubscriber<IUnitEntity>, ISubscriber
{
	void HandleEntityExitCohesionRange(MechanicEntity entity);
}
public interface IEntityExitCohesionRangeHandler<TTag> : IEntityExitCohesionRangeHandler, ISubscriber<IUnitEntity>, ISubscriber, IEventTag<IEntityExitCohesionRangeHandler, TTag>
{
}
