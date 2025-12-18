using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Encounter.Events;

public interface IStartEncounterHandler : ISubscriber<ActiveEncounter>, ISubscriber
{
	void HandleStartEncounter();
}
