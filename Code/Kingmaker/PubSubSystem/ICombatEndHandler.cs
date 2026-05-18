using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICombatEndHandler : ISubscriber
{
	void HandleCombatEnd(EncounterCompletionType reason);
}
