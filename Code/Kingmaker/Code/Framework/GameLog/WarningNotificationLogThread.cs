using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;

namespace Kingmaker.Code.Framework.GameLog;

public class WarningNotificationLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventWarningNotification>
{
	public void HandleEvent(GameLogEventWarningNotification evt)
	{
		if (evt.Type == WarningNotificationType.None)
		{
			AddMessage(new CombatLogMessage(evt.Text, LogThreadBase.Colors.WarningLogColor, PrefixIcon.None));
			return;
		}
		string text = LocalizedTexts.Instance.WarningNotification.GetText(evt.Type);
		AddMessage(new CombatLogMessage(text, LogThreadBase.Colors.WarningLogColor, PrefixIcon.None));
	}
}
