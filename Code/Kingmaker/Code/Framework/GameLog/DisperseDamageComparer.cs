using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Disperse;

namespace Kingmaker.Code.Framework.GameLog;

public class DisperseDamageComparer : IMergeEventComparer
{
	public bool Compare(GameLogEvent lhs, GameLogEvent rhs)
	{
		if (!(lhs is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent) || !(rhs is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent2))
		{
			return false;
		}
		if (gameLogRuleEvent.ParentEvent == gameLogRuleEvent2.ParentEvent)
		{
			return gameLogRuleEvent.ParentEvent is GameLogRuleEvent<RuleDisperseDamage>;
		}
		return false;
	}
}
