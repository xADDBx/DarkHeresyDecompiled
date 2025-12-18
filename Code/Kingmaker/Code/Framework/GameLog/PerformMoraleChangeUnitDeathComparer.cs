using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Code.Framework.GameLog;

public class PerformMoraleChangeUnitDeathComparer : IMergeEventComparer
{
	public static IMergeEventComparer Create()
	{
		return new PerformMoraleChangeUnitDeathComparer();
	}

	private PerformMoraleChangeUnitDeathComparer()
	{
	}

	public bool Compare(GameLogEvent evn1, GameLogEvent evn2)
	{
		if (evn1 is GameLogRuleEvent<RulePerformMoraleChange> gameLogRuleEvent && evn2 is GameLogRuleEvent<RulePerformMoraleChange> gameLogRuleEvent2 && IsSuitableEvent(gameLogRuleEvent.Rule.EventType))
		{
			return IsSuitableEvent(gameLogRuleEvent2.Rule.EventType);
		}
		return false;
	}

	private bool IsSuitableEvent(MoraleEventType eventType)
	{
		if (!eventType.HasFlag(MoraleEventType.AllyDeath) && !eventType.HasFlag(MoraleEventType.LeaderAllyDeath) && !eventType.HasFlag(MoraleEventType.EnemyDeath))
		{
			return eventType.HasFlag(MoraleEventType.LeaderAllyDeath);
		}
		return true;
	}
}
