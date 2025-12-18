using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Morale;

public interface IMoraleVictoryConfirmationHandler : ISubscriber
{
	void HandleMoraleVictoryConfirmation(bool confirmed);
}
