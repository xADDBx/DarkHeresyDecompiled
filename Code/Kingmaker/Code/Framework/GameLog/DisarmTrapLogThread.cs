using System;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.Code.Framework.GameLog;

public class DisarmTrapLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventDisarmTrap>
{
	public void HandleEvent(GameLogEventDisarmTrap evt)
	{
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Actor;
		switch (evt.Result)
		{
		case GameLogEventDisarmTrap.ResultType.Success:
			AddMessage(LogThreadBase.Strings.DisarmTrapSuccess.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, evt.Actor));
			break;
		case GameLogEventDisarmTrap.ResultType.Fail:
			AddMessage(LogThreadBase.Strings.DisarmTrapFail.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, evt.Actor));
			break;
		case GameLogEventDisarmTrap.ResultType.CriticalFail:
			AddMessage(LogThreadBase.Strings.DisarmTrapCriticalFail.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, evt.Actor));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
