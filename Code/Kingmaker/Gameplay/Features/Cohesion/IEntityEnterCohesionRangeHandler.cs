using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Cohesion;

public interface IEntityEnterCohesionRangeHandler : ISubscriber<IUnitEntity>, ISubscriber
{
	void HandleEntityEnterCohesionRange(MechanicEntity entity);
}
public interface IEntityEnterCohesionRangeHandler<TTag> : IEntityEnterCohesionRangeHandler, ISubscriber<IUnitEntity>, ISubscriber, IEventTag<IEntityEnterCohesionRangeHandler, TTag>
{
}
