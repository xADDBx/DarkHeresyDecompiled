using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Morale;

public interface IMoraleValueHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleMoraleValueChanged(int delta, bool hasCriticalEffect);
}
public interface IMoraleValueHandler<TTag> : IMoraleValueHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IMoraleValueHandler, TTag>
{
}
