using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitGainMovementPoints : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitGainMovementPoints(float movementPoints, [CanBeNull] IEvalContext context);
}
public interface IUnitGainMovementPoints<TTag> : IUnitGainMovementPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitGainMovementPoints, TTag>
{
}
