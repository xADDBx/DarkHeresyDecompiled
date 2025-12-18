using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Cohesion;

public interface IEntityMoveInCohesionRangeHandler : ISubscriber<IUnitEntity>, ISubscriber
{
	void HandleEntityMoveInCohesionRange(MechanicEntity entity);
}
public interface IEntityMoveInCohesionRangeHandler<TTag> : IEntityMoveInCohesionRangeHandler, ISubscriber<IUnitEntity>, ISubscriber, IEventTag<IEntityMoveInCohesionRangeHandler, TTag>
{
}
