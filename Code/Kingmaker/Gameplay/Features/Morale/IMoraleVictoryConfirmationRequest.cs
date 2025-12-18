using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Gameplay.Features.Morale;

public interface IMoraleVictoryConfirmationRequest : ISubscriber
{
	public delegate void Callback(bool confirmed);

	void HandleMoraleVictoryConfirmationRequest(Callback callback);
}
