using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Code.Framework.GameLog;

public class PatternAddEventInsertSeparator : PatternAddEvent
{
	public static PatternAddEvent Create()
	{
		return new PatternAddEventInsertSeparator();
	}

	private PatternAddEventInsertSeparator()
	{
	}

	protected override bool TryApplyImpl(List<GameLogEvent> queue, GameLogEvent @in, out GameLogEvent @out)
	{
		if (@in is GameLogEventAddSeparator { State: GameLogEventAddSeparator.States.Start })
		{
			int num = queue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RulePerformAbility>);
			if (num != -1)
			{
				queue.Insert(num, @in);
				@out = @in;
				return true;
			}
		}
		@out = null;
		return false;
	}
}
