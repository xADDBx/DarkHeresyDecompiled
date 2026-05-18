using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Encounter;

public interface IEncounterObjectiveHandler : ISubscriber
{
	void HandleObjectiveStateChanged(int objectiveIndex, EncounterObjectiveState newState);

	void HandleObjectiveCounterChanged(int objectiveIndex, int newValue, int? targetValue);
}
