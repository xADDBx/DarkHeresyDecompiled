using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.Code.Framework.GameLog;

public class PatternPostAddEventSwitchApplyBuffAndDependAbility : PatternPostAddEvent
{
	public static PatternPostAddEvent Create()
	{
		return new PatternPostAddEventSwitchApplyBuffAndDependAbility();
	}

	private PatternPostAddEventSwitchApplyBuffAndDependAbility()
	{
	}

	protected override void ApplyImpl(List<GameLogEvent> queue, GameLogEvent @event)
	{
		if (!(@event is GameLogRuleEvent<RulePerformAbility> gameLogRuleEvent))
		{
			return;
		}
		MechanicsContext executionActionContext = gameLogRuleEvent.Rule.ExecutionActionContext;
		if (executionActionContext == null)
		{
			return;
		}
		BlueprintScriptableObject blueprint = executionActionContext.Blueprint;
		BlueprintBuff blueprintBuff = blueprint as BlueprintBuff;
		if (blueprintBuff != null)
		{
			int num = queue.FindLastIndex((GameLogEvent evn) => evn is GameLogRuleEvent<RuleCalculateCanApplyBuff> gameLogRuleEvent2 && gameLogRuleEvent2.Rule.Blueprint == blueprintBuff);
			if (num != -1)
			{
				queue.Insert(num, @event);
			}
		}
	}
}
