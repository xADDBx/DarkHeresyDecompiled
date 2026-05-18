using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("edd2027da0a9d0a40af243a2a9e5de6f")]
public class TutorialTriggerMoraleVictoryConfirmation : TutorialTrigger, IMoraleVictoryConfirmationRequest, ISubscriber
{
	public void HandleMoraleVictoryConfirmationRequest(IMoraleVictoryConfirmationRequest.Callback callback)
	{
		TryToTrigger(null);
	}
}
