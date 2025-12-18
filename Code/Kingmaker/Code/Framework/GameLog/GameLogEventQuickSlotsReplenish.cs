using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventQuickSlotsReplenish : GameLogEvent<GameLogEventQuickSlotsReplenish>
{
	[UsedImplicitly]
	private class EventsHandler : GameLogController.GameEventsHandler, IPartyQuickSlotsReplenishedHandler, ISubscriber
	{
		public void HandleQuickSlotsReplenished(QuickSlotsReplenishResult result)
		{
			AddEvent(new GameLogEventQuickSlotsReplenish(result));
		}
	}

	public readonly QuickSlotsReplenishResult Result;

	private GameLogEventQuickSlotsReplenish(QuickSlotsReplenishResult result)
	{
		Result = result;
	}
}
