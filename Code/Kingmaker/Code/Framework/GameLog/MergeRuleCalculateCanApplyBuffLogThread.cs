using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class MergeRuleCalculateCanApplyBuffLogThread : LogThreadBase, IGameLogEventHandler<MergeGameLogEvent<GameLogRuleEvent<RuleCalculateCanApplyBuff>>>
{
	public void HandleEvent(MergeGameLogEvent<GameLogRuleEvent<RuleCalculateCanApplyBuff>> evt)
	{
		IReadOnlyList<GameLogRuleEvent<RuleCalculateCanApplyBuff>> events = evt.GetEvents();
		RuleCalculateCanApplyBuff rule = events[0].Rule;
		if (events.Count == 1)
		{
			CombatLogMessage combatLogMessage = RulebookCanApplyBuffLogThread.GetCombatLogMessage(rule);
			AddMessage(combatLogMessage);
			return;
		}
		MechanicEntity caster = rule.Reason.Caster;
		GameLogContext.Text = rule.Context.Name;
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)caster;
		CombatLogMessage combatLogMessage2 = LogThreadBase.Strings.GroupStatusEffect.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, caster);
		TooltipBaseTemplate tooltipBaseTemplate = combatLogMessage2?.Tooltip;
		if (tooltipBaseTemplate != null)
		{
			ITooltipBrick[] array = CollectExtraBricks(events).ToArray();
			if (array.Length == 0)
			{
				return;
			}
			if (array.Length == 1)
			{
				CombatLogMessage combatLogMessage3 = RulebookCanApplyBuffLogThread.GetCombatLogMessage(rule);
				AddMessage(combatLogMessage3);
				return;
			}
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, array, arg3: false);
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, array, arg3: true);
		}
		AddMessage(combatLogMessage2);
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(IEnumerable<GameLogRuleEvent<RuleCalculateCanApplyBuff>> events)
	{
		Func<CombatLogMessage, bool, ITooltipBrick> nestedMessageTemplate = CombatLogTooltipService.CreateBrickNestedMessage;
		if (nestedMessageTemplate == null)
		{
			yield break;
		}
		foreach (GameLogRuleEvent<RuleCalculateCanApplyBuff> @event in events)
		{
			CombatLogMessage combatLogMessage = RulebookCanApplyBuffLogThread.GetCombatLogMessage(@event.Rule);
			if (combatLogMessage != null)
			{
				yield return nestedMessageTemplate(combatLogMessage, arg2: true);
			}
		}
	}
}
