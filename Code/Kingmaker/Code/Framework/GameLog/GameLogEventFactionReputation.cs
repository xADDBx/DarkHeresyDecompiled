using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventFactionReputation : GameLogEvent<GameLogEventFactionReputation>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IGainFactionReputationHandler, ISubscriber
	{
		public void HandleGainFactionReputation(FactionType factionTypeType, ReputationType reputationType, int count)
		{
			AddEvent(new GameLogEventFactionReputation(factionTypeType, reputationType, count));
		}
	}

	public readonly FactionType FactionType;

	public readonly ReputationType ReputationType;

	public readonly int Points;

	private GameLogEventFactionReputation(FactionType factionType, ReputationType reputationType, int reputationPoints)
	{
		FactionType = factionType;
		ReputationType = reputationType;
		Points = reputationPoints;
	}
}
