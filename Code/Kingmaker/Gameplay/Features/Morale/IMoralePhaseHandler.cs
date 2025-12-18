using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Morale;

public interface IMoralePhaseHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleMoralePhaseChanged(MoralePhaseType phase);
}
public interface IMoralePhaseHandler<TTag> : IMoralePhaseHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IMoralePhaseHandler, TTag>
{
}
