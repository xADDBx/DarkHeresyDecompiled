using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Code.Framework.GameLog;

public class CalculateCanApplyBuffComparer : IMergeEventComparer
{
	public static IMergeEventComparer Create()
	{
		return new CalculateCanApplyBuffComparer();
	}

	private CalculateCanApplyBuffComparer()
	{
	}

	public bool Compare(GameLogEvent evn1, GameLogEvent evn2)
	{
		if (evn1.ParentEvent == evn2.ParentEvent && evn1 is GameLogRuleEvent<RuleCalculateCanApplyBuff> gameLogRuleEvent && evn2 is GameLogRuleEvent<RuleCalculateCanApplyBuff> gameLogRuleEvent2 && gameLogRuleEvent.Rule.AppliedBuff.Blueprint == gameLogRuleEvent2.Rule.AppliedBuff.Blueprint)
		{
			return gameLogRuleEvent.Rule.AppliedBuff.MaybeParentContext?.MaybeCaster == gameLogRuleEvent2.Rule.AppliedBuff.MaybeParentContext?.MaybeCaster;
		}
		return false;
	}
}
