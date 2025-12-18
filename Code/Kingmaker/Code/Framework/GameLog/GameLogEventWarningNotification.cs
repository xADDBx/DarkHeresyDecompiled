using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventWarningNotification : GameLogEvent<GameLogEventWarningNotification>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IWarningNotificationUIHandler, ISubscriber
	{
		public void HandleWarning(WarningNotificationType type, bool addToLog = true, WarningNotificationFormat? format = null, bool withSound = true)
		{
			if (addToLog)
			{
				AddEvent(new GameLogEventWarningNotification(type, null));
			}
		}

		public void HandleWarning(string text, bool addToLog = true, WarningNotificationFormat format = WarningNotificationFormat.Common, bool withSound = true)
		{
			if (addToLog)
			{
				AddEvent(new GameLogEventWarningNotification(WarningNotificationType.None, text));
			}
		}
	}

	public readonly WarningNotificationType Type;

	public readonly string Text;

	public GameLogEventWarningNotification(WarningNotificationType type, string text)
	{
		Type = type;
		Text = text;
	}
}
