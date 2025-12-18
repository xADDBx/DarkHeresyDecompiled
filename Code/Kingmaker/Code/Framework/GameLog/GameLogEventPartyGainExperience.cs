using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventPartyGainExperience : GameLogEvent<GameLogEventPartyGainExperience>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IPartyGainExperienceHandler, ISubscriber
	{
		public void HandlePartyGainExperience(int gained, bool isExperienceForEncounter)
		{
			AddEvent(new GameLogEventPartyGainExperience(gained, isExperienceForEncounter));
		}
	}

	public readonly int Experience;

	public readonly bool IsExperienceForDeath;

	public GameLogEventPartyGainExperience(int experience, bool isExperienceForDeath)
	{
		Experience = experience;
		IsExperienceForDeath = isExperienceForDeath;
	}
}
