using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Cohesion;

public interface IEntityStartTurnInCohesionRangeHandler : ISubscriber<IUnitEntity>, ISubscriber
{
	void HandleEntityStartTurnInCohesionRange(MechanicEntity entity);
}
public interface IEntityStartTurnInCohesionRangeHandler<TTag> : IEntityStartTurnInCohesionRangeHandler, ISubscriber<IUnitEntity>, ISubscriber, IEventTag<IEntityStartTurnInCohesionRangeHandler, TTag>
{
}
