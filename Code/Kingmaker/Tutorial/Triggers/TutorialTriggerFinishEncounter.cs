using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("99d3144b0c304834b963313dc4033829")]
public class TutorialTriggerFinishEncounter : TutorialTrigger, ICombatEndHandler, ISubscriber
{
	public EncounterCompletionType CompletionType;

	public void HandleCombatEnd(EncounterCompletionType reason)
	{
		if (reason == CompletionType)
		{
			TryToTrigger(null);
		}
	}
}
