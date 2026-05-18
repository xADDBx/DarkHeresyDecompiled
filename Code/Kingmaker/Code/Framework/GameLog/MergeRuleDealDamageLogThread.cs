using System;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Disperse;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Mechanics.Facts.Interfaces;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.Framework.GameLog;

public class MergeRuleDealDamageLogThread : LogThreadBase, IGameLogEventHandler<MergeGameLogEvent<GameLogRuleEvent<RuleDealDamage>>>
{
	public void HandleEvent(MergeGameLogEvent<GameLogRuleEvent<RuleDealDamage>> evt)
	{
		IReadOnlyList<GameLogRuleEvent<RuleDealDamage>> events = evt.GetEvents();
		HandleParentAttackEvent(events[0]);
		HandleDispersedDamageEvents(events);
	}

	private void HandleParentAttackEvent(GameLogEvent evt)
	{
		GameLogEventAttack parentAttackEvent = GetParentAttackEvent(evt);
		if (parentAttackEvent != null)
		{
			CombatLogMessage newMessage = PerformAttackLogThread.CreateMessage(parentAttackEvent);
			AddMessage(newMessage);
		}
	}

	private void HandleDispersedDamageEvents(IReadOnlyList<GameLogRuleEvent<RuleDealDamage>> mergedEvents)
	{
		GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent = mergedEvents[0];
		GameLogEvent parentEvent = gameLogRuleEvent.ParentEvent;
		if (parentEvent is GameLogRuleEvent<RuleDisperseDamage> && parentEvent.ParentEvent is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent2)
		{
			RuleDealDamage rule = gameLogRuleEvent2.Rule;
			RuleReason reason = gameLogRuleEvent.Rule.Reason;
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)null;
			GameLogContext.SourceFact = (GameLogContext.Property<IMechanicEntityFact>)(IMechanicEntityFact)reason.Fact;
			GameLogContext.ResultDamage = rule.ResultValue;
			GameLogContext.DamageType = UtilityText.GetDamageTypeText(rule.ResultDamage.Type);
			GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteTarget;
			string text = LogThreadBase.Strings.DispersedDamageGroup.Message.Text;
			TooltipBaseTemplate tooltipBaseTemplate = CombatLogTooltipService.CreateTooltipTemplateCombatLogMessage(text, string.Empty, 0f);
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, GetDisperseDamageTooltip(reason.Fact, mergedEvents, info: false), arg3: false);
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, GetDisperseDamageTooltip(reason.Fact, mergedEvents, info: true), arg3: true);
			PrefixIcon icon = LogThreadBase.Strings.DispersedDamageGroup.Icon;
			Color32 color = LogThreadBase.Strings.DispersedDamageGroup.Color;
			CombatLogMessage newMessage = new CombatLogMessage(text, color, icon, tooltipBaseTemplate);
			AddMessage(newMessage);
		}
	}

	private static GameLogEventAttack GetParentAttackEvent(GameLogEvent childEvent)
	{
		GameLogEvent gameLogEvent = childEvent;
		while (gameLogEvent != null)
		{
			gameLogEvent = gameLogEvent.ParentEvent;
			if (gameLogEvent is GameLogEventAttack result)
			{
				return result;
			}
		}
		return null;
	}

	private static IEnumerable<ITooltipBrick> GetDisperseDamageTooltip(MechanicEntityFact sourceFact, IEnumerable<GameLogRuleEvent<RuleDealDamage>> mergedEvents, bool info)
	{
		Func<CombatLogMessage, bool, ITooltipBrick> createBrickNestedMessage = CombatLogTooltipService.CreateBrickNestedMessage;
		Func<string, ITooltipBrick> createBrickText = CombatLogTooltipService.CreateBrickText;
		if (createBrickNestedMessage == null || createBrickText == null)
		{
			return null;
		}
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)null;
		GameLogContext.SourceFact = (GameLogContext.Property<IMechanicEntityFact>)(IMechanicEntityFact)sourceFact;
		foreach (GameLogRuleEvent<RuleDealDamage> mergedEvent in mergedEvents)
		{
			RuleDealDamage rule = mergedEvent.Rule;
			GameLogContext.ResultDamage = rule.ResultValue;
			GameLogContext.DamageType = UtilityText.GetDamageTypeText(rule.ResultDamage.Type);
			GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteTarget;
			CombatLogMessage combatLogMessage = LogThreadBase.Strings.DispersedDamage.CreateCombatLogMessage();
			ITooltipBrick item = (info ? createBrickText(combatLogMessage?.Message) : createBrickNestedMessage(combatLogMessage, arg2: true));
			list.Add(item);
		}
		return list;
	}
}
