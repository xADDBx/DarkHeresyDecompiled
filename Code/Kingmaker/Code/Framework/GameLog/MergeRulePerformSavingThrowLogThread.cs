using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class MergeRulePerformSavingThrowLogThread : LogThreadBase, IGameLogEventHandler<MergeGameLogEvent<GameLogRuleEvent<RulePerformSavingThrow>>>
{
	public void HandleEvent(MergeGameLogEvent<GameLogRuleEvent<RulePerformSavingThrow>> evt)
	{
		IReadOnlyList<GameLogRuleEvent<RulePerformSavingThrow>> events = evt.GetEvents();
		RulePerformSavingThrow rule = events[0].Rule;
		MechanicEntity caster = rule.Reason.Caster;
		GameLogContext.Text = LocalizedTexts.Instance.Stats.GetText(rule.StatType);
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)caster;
		CombatLogMessage combatLogMessage = LogThreadBase.Strings.SavingThrowGroup.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, caster);
		TooltipBaseTemplate tooltipBaseTemplate = combatLogMessage?.Tooltip;
		if (tooltipBaseTemplate != null)
		{
			ITooltipBrick[] array = CollectExtraBricks(events).ToArray();
			if (array.Length == 0)
			{
				return;
			}
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, array, arg3: false);
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, array, arg3: true);
		}
		AddMessage(combatLogMessage);
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(IEnumerable<GameLogRuleEvent<RulePerformSavingThrow>> events)
	{
		Func<CombatLogMessage, bool, ITooltipBrick> nestedMessageTemplate = CombatLogTooltipService.CreateTooltipBrickNestedMessage;
		if (nestedMessageTemplate == null)
		{
			yield break;
		}
		foreach (GameLogRuleEvent<RulePerformSavingThrow> @event in events)
		{
			CombatLogMessage combatLogMessage = RulebookSavingThrowLogThread.GetCombatLogMessage(@event.Rule, ignoreInitiatorDeath: true);
			if (combatLogMessage != null)
			{
				yield return nestedMessageTemplate(combatLogMessage, arg2: true);
			}
		}
	}
}
