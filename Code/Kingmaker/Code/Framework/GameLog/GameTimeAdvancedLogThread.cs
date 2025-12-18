using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Code.Framework.GameLog;

public class GameTimeAdvancedLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventTimeAdvanced>
{
	public void HandleEvent(GameLogEventTimeAdvanced evt)
	{
		CombatLogMessage combatLogMessage = ConfigRoot.Instance.LocalizedTexts.GameLog.TimePassed.CreateCombatLogMessage();
		string compactPeriodString = ConfigRoot.Instance.Calendar.GetCompactPeriodString(evt.DeltaTime);
		if (!string.IsNullOrEmpty(compactPeriodString))
		{
			string timePassedMessage = ((combatLogMessage != null) ? (combatLogMessage.Message + ": " + compactPeriodString) : compactPeriodString);
			combatLogMessage?.ReplaceMessage(timePassedMessage);
			AddMessage(combatLogMessage);
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(timePassedMessage, addToLog: false);
			});
		}
	}
}
