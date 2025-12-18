using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventPartyCombatState : GameLogEvent<GameLogEventPartyCombatState>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IPartyCombatHandler, ISubscriber
	{
		public void HandlePartyCombatStateChanged(bool inCombat)
		{
			AddEvent(new GameLogEventPartyCombatState(inCombat));
		}
	}

	public readonly bool InCombat;

	public GameLogEventPartyCombatState(bool inCombat)
	{
		InCombat = inCombat;
	}
}
