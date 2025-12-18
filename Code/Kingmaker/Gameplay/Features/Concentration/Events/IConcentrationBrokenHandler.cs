using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Concentration.Events;

public interface IConcentrationBrokenHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleConcentrationBroken([CanBeNull] MechanicEntity reason);
}
public interface IConcentrationBrokenHandler<TTag> : IConcentrationBrokenHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IConcentrationBrokenHandler, TTag>
{
}
