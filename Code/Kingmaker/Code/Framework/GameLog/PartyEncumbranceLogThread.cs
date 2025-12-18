using System;
using Kingmaker.UnitLogic;

namespace Kingmaker.Code.Framework.GameLog;

public class PartyEncumbranceLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventPartyEncumbranceChanged>
{
	public void HandleEvent(GameLogEventPartyEncumbranceChanged evt)
	{
		switch (evt.CurrentEncumbrance)
		{
		case Encumbrance.Light:
			AddMessage(LogThreadBase.Strings.PartyEncumbranceLight.CreateCombatLogMessage());
			break;
		case Encumbrance.Medium:
			AddMessage(LogThreadBase.Strings.PartyEncumbranceMedium.CreateCombatLogMessage());
			break;
		case Encumbrance.Heavy:
			AddMessage(LogThreadBase.Strings.PartyEncumbranceHeavy.CreateCombatLogMessage());
			break;
		case Encumbrance.Overload:
			AddMessage(LogThreadBase.Strings.PartyEncumbranceOverload.CreateCombatLogMessage());
			break;
		default:
			throw new ArgumentOutOfRangeException("CurrentEncumbrance", evt.CurrentEncumbrance, "CurrentEncumbrance is out of range");
		}
	}
}
