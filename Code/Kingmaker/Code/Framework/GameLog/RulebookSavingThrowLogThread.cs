using System;
using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

[Obsolete]
public class RulebookSavingThrowLogThread : LogThreadBase, IGameLogRuleHandler<RulePerformSavingThrow>
{
	public void HandleEvent(RulePerformSavingThrow rule)
	{
		CombatLogMessage combatLogMessage = GetCombatLogMessage(rule, ignoreInitiatorDeath: true);
		if (combatLogMessage != null)
		{
			AddMessage(combatLogMessage);
		}
	}

	public static CombatLogMessage GetCombatLogMessage(RulePerformSavingThrow rule, bool ignoreInitiatorDeath = false)
	{
		return null;
	}

	public static IEnumerable<ITooltipBrick> CollectExtraBricks(RulePerformSavingThrow rule)
	{
		yield break;
	}
}
