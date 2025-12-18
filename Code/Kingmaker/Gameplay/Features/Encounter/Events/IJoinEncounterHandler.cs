using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Encounter.Events;

public interface IJoinEncounterHandler : ISubscriber<MechanicEntity>, ISubscriber
{
	void HandleJoinEncounter();
}
public interface IJoinEncounterHandler<TTag> : IJoinEncounterHandler, ISubscriber<MechanicEntity>, ISubscriber, IEventTag<IJoinEncounterHandler, TTag>
{
}
