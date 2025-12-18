using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class MoraleLogThread : LogThreadBase, IGameLogEventHandler<MergeGameLogEvent<GameLogRuleEvent<RulePerformMoraleChange>>>, IGameLogRuleHandler<RulePerformMoraleChange>
{
	private MergeGameLogEvent<GameLogRuleEvent<RulePerformMoraleChange>> m_Events;

	public void HandleEvent(MergeGameLogEvent<GameLogRuleEvent<RulePerformMoraleChange>> evt)
	{
		m_Events = evt;
		CombatLogMessage combatLogMessage = LogThreadBase.Strings.MoraleValueChangedMultipleUnits.CreateCombatLogMessage();
		TooltipBaseTemplate tooltipBaseTemplate = CombatLogTooltipService.CreateTooltipTemplateCombatLogMessage(combatLogMessage.Message, string.Empty, 0f);
		Dictionary<BaseUnitEntity, List<RulePerformMoraleChange>> dictionary = AggregateMoraleEventsByUnit(evt.GetEvents());
		GameLogContext.Text = dictionary.Count.ToString();
		ITooltipBrick[] array = CollectExtraBricks(dictionary).ToArray();
		if (array.Length != 0)
		{
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, array, arg3: false);
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, array, arg3: true);
			AddMessage(new CombatLogMessage(combatLogMessage, tooltipBaseTemplate));
		}
	}

	public void HandleEvent(RulePerformMoraleChange evt)
	{
		if (!evt.EventType.HasFlag(MoraleEventType.CombatEnd) && (evt.ValueModifier.List.Count != 0 || evt.MoraleBeforeEvent != 0 || evt.MoraleAfterEvent != 0) && evt.MoraleAfterEvent != evt.MoraleBeforeEvent)
		{
			CombatLogMessage combatLogMessage = GetCombatLogMessage(evt);
			TooltipBaseTemplate tooltipBaseTemplate = CombatLogTooltipService.CreateTooltipTemplateCombatLogMessage(combatLogMessage.Message, string.Empty, 0f);
			ITooltipBrick[] array = GetBricksForMoraleSources(new MoraleEventSummary(evt)).ToArray();
			if (array.Length != 0)
			{
				CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, array, arg3: false);
				CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, array, arg3: true);
				AddMessage(new CombatLogMessage(combatLogMessage, tooltipBaseTemplate));
			}
		}
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(Dictionary<BaseUnitEntity, List<RulePerformMoraleChange>> events)
	{
		foreach (KeyValuePair<BaseUnitEntity, List<RulePerformMoraleChange>> @event in events)
		{
			MoraleEventSummary summary = new MoraleEventSummary(@event.Value);
			List<MoraleEventSummary> aggregatedEventsByType = GetAggregatedEventsByType(@event.Value);
			yield return CombatLogTooltipService.CreateTooltipBricksGroupStart();
			GetCombatLogMessage(summary);
			yield return CombatLogTooltipService.CreateTooltipBrickText(GetCombatLogMessage(summary).Message);
			foreach (MoraleEventSummary item in aggregatedEventsByType)
			{
				foreach (ITooltipBrick bricksForMoraleSource in GetBricksForMoraleSources(item))
				{
					yield return bricksForMoraleSource;
				}
			}
			yield return CombatLogTooltipService.CreateTooltipBricksGroupEnd();
			yield return CombatLogTooltipService.CreateTooltipBrickSeparator(TooltipBrickElementType.Big);
		}
	}

	private static List<MoraleEventSummary> GetAggregatedEventsByType(List<RulePerformMoraleChange> rules)
	{
		List<MoraleEventSummary> list = new List<MoraleEventSummary>();
		foreach (RulePerformMoraleChange rule in rules)
		{
			MoraleEventSummary moraleEventSummary = list.Find((MoraleEventSummary s) => s.IsEventOfSameType(rule));
			if (moraleEventSummary != null)
			{
				moraleEventSummary.AggregateWith(rule);
			}
			else
			{
				list.Add(new MoraleEventSummary(rule));
			}
		}
		return list;
	}

	private static Dictionary<BaseUnitEntity, List<RulePerformMoraleChange>> AggregateMoraleEventsByUnit(IEnumerable<GameLogRuleEvent<RulePerformMoraleChange>> events)
	{
		Dictionary<BaseUnitEntity, List<RulePerformMoraleChange>> dictionary = new Dictionary<BaseUnitEntity, List<RulePerformMoraleChange>>();
		foreach (GameLogRuleEvent<RulePerformMoraleChange> @event in events)
		{
			if (@event.Rule.Target is BaseUnitEntity key)
			{
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, new List<RulePerformMoraleChange>());
				}
				dictionary[key].Add(@event.Rule);
			}
		}
		return dictionary;
	}

	private static IEnumerable<ITooltipBrick> GetBricksForMoraleSources(MoraleEventSummary summary)
	{
		Func<TooltipBrickTextValueArgs, ITooltipBrick> textValueTooltip = CombatLogTooltipService.CreateTooltipBrickTextValue;
		Func<TooltipBrickElementType, ITooltipBrick> separatorTooltip = CombatLogTooltipService.CreateTooltipBrickSeparator;
		if (textValueTooltip == null || separatorTooltip == null)
		{
			yield break;
		}
		yield return textValueTooltip(new TooltipBrickTextValueArgs(UIStrings.Instance.CombatLog.MoraleSourcesHeader.Text + ":", string.Empty, 0, isResultValue: false, needShowLine: true, TooltipTextType.Bold, TooltipTextAlignment.Left));
		RulePerformMoraleChange rule = summary.Rule;
		string arg = ((summary.Count > 1) ? (" ×" + summary.Count) : string.Empty);
		string text = $"{GetEventReason(rule)}{arg}";
		string value = (rule.ResultDelta * summary.Count).ToStringWithSign();
		yield return textValueTooltip(new TooltipBrickTextValueArgs(text, value, 0, isResultValue: true));
		bool resultIsCapped = rule.ResultDeltaRaw != rule.ResultDelta;
		if (rule.ValueModifier.List.Count > 1 || resultIsCapped)
		{
			foreach (Modifier item in rule.ValueModifier.List)
			{
				yield return textValueTooltip(new TooltipBrickTextValueArgs(GetModifierDescription(item), item.Value.ToStringWithSign(), 1));
			}
		}
		bool showTopLimit = resultIsCapped && rule.ResultDeltaRaw > 0;
		bool showBottomLimit = resultIsCapped && rule.ResultDeltaRaw < 0;
		if (showTopLimit || showBottomLimit)
		{
			yield return separatorTooltip(TooltipBrickElementType.Big);
		}
		if (showTopLimit)
		{
			yield return textValueTooltip(new TooltipBrickTextValueArgs(UIStrings.Instance.CombatLog.MoraleTopLimit, rule.TopLimit.ToStringWithSign(), 1));
			if (rule.TopLimitModifier.List.Count > 1)
			{
				foreach (Modifier item2 in rule.TopLimitModifier.List)
				{
					yield return textValueTooltip(new TooltipBrickTextValueArgs(GetModifierDescription(item2), item2.Value.ToStringWithSign(), 2));
				}
			}
		}
		if (!showBottomLimit)
		{
			yield break;
		}
		yield return textValueTooltip(new TooltipBrickTextValueArgs(UIStrings.Instance.CombatLog.MoraleBottomLimit, rule.BottomLimit.ToStringWithSign(), 1));
		if (rule.BottomLimitModifier.List.Count <= 1)
		{
			yield break;
		}
		foreach (Modifier item3 in rule.BottomLimitModifier.List)
		{
			yield return textValueTooltip(new TooltipBrickTextValueArgs(GetModifierDescription(item3), item3.Value.ToStringWithSign(), 2));
		}
	}

	private static TooltipBaseTemplate GetTooltipToBrick(Modifier modifier)
	{
		if (modifier.Fact != null)
		{
			EntityFact fact = modifier.Fact;
			if (!(fact is Ability ability))
			{
				if (!(fact is Buff buff))
				{
					if (fact is Feature { Hidden: false } feature)
					{
						return CombatLogTooltipService.CreateTooltipTemplateFeature(feature);
					}
				}
				else if (!buff.Hidden)
				{
					return CombatLogTooltipService.CreateTooltipTemplateBuff(new TooltipTemplateBuffArgs(buff));
				}
			}
			else if (!ability.Hidden)
			{
				return CombatLogTooltipService.CreateTooltipTemplateAbilityBlueprint(ability.Blueprint);
			}
		}
		if (modifier.Item != null)
		{
			return CombatLogTooltipService.CreateTooltipTemplateItem(modifier.Item);
		}
		return null;
	}

	private static string GetModifierDescription(Modifier modifier)
	{
		if (!string.IsNullOrEmpty(modifier.Fact?.Name))
		{
			return modifier.Fact.Name;
		}
		return LocalizedTexts.Instance.Descriptors.GetText(modifier.Descriptor);
	}

	private static CombatLogMessage GetCombatLogMessage(RulePerformMoraleChange evt)
	{
		return GetCombatLogMessage(new MoraleEventSummary(evt));
	}

	private static CombatLogMessage GetCombatLogMessage(MoraleEventSummary summary)
	{
		GameLogContext.MoraleStartValue = summary.MoraleStartValue;
		GameLogContext.MoraleResultValue = summary.MoraleResultValue;
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)summary.Target;
		GameLogContext.Text = summary.Count.ToString();
		if (!summary.MoraleResultPhase.HasValue)
		{
			return LogThreadBase.Strings.MoraleValueChangedSingleUnit.CreateCombatLogMessage();
		}
		MoralePhaseType? moraleResultPhase = summary.MoraleResultPhase;
		bool flag;
		if (moraleResultPhase.HasValue)
		{
			MoralePhaseType valueOrDefault = moraleResultPhase.GetValueOrDefault();
			if ((uint)(valueOrDefault - 1) <= 1u)
			{
				flag = true;
				goto IL_008d;
			}
		}
		flag = false;
		goto IL_008d;
		IL_008d:
		if (flag)
		{
			moraleResultPhase = summary.MoraleResultPhase;
			string text = default(string);
			switch (moraleResultPhase)
			{
			case MoralePhaseType.Broken:
				text = UIStrings.Instance.CombatLog.MoralePhaseBroken.Text;
				break;
			case MoralePhaseType.Heroic:
				text = UIStrings.Instance.CombatLog.MoralePhaseHeroic.Text;
				break;
			default:
				global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(moraleResultPhase);
				break;
			}
			GameLogContext.MoralePhase = text;
			return LogThreadBase.Strings.MoraleValueAndPhaseChangedSingleUnit.CreateCombatLogMessage();
		}
		return LogThreadBase.Strings.MoraleValueChangedSingleUnit.CreateCombatLogMessage();
	}

	private static GameLogContext.Property<string> GetEventReason(RulePerformMoraleChange rule)
	{
		return rule.EventType switch
		{
			MoraleEventType.CombatStart | MoraleEventType.TurnStart => UIStrings.Instance.CombatLog.MoraleEventCombatStart.Text, 
			MoraleEventType.TurnStart => UIStrings.Instance.CombatLog.MoraleEventTurnStart.Text, 
			MoraleEventType.AllyDeath => UIStrings.Instance.CombatLog.MoraleEventAllyDeath.Text, 
			MoraleEventType.EnemyDeath => GetEnemyDeathReason(rule), 
			MoraleEventType.AllyDeath | MoraleEventType.LeaderAllyDeath => UIStrings.Instance.CombatLog.MoraleEventLeaderAllyDeath.Text, 
			MoraleEventType.EnemyDeath | MoraleEventType.LeaderEnemyDeath => UIStrings.Instance.CombatLog.MoraleEventLeaderEnemyDeath.Text, 
			MoraleEventType.RestoreToRegular => UIStrings.Instance.CombatLog.MoraleEventRestoreToRegular.Text, 
			MoraleEventType.BecomeHeroic => UIStrings.Instance.CombatLog.MoraleEventBecomeHeroic.Text, 
			MoraleEventType.BecomeBroken => UIStrings.Instance.CombatLog.MoraleEventBecomeBroken.Text, 
			MoraleEventType.ForcedChangeMorale => GetForcedChangeReason(rule), 
			MoraleEventType.ForcedChangeMoralePhase => UIStrings.Instance.CombatLog.MoraleEventForcedChangeMoralePhase.Text, 
			_ => null, 
		};
	}

	private static string GetForcedChangeReason(RulePerformMoraleChange rule)
	{
		MechanicsContext sourceContext = rule.SourceContext;
		if (sourceContext != null)
		{
			MechanicEntityFact fact = sourceContext.Fact;
			if (fact != null)
			{
				string name = fact.Name;
				if (name != null)
				{
					return name;
				}
			}
			if (sourceContext.Blueprint is IUIDataProvider { Name: { } name2 })
			{
				return name2;
			}
		}
		return UIStrings.Instance.CombatLog.MoraleEventForcedChangeMorale.Text;
	}

	private static string GetEnemyDeathReason(RulePerformMoraleChange rule)
	{
		if (rule.Initiator != rule.Target)
		{
			return UIStrings.Instance.CombatLog.MoraleEventEnemyDeath.Text;
		}
		return UIStrings.Instance.CombatLog.MoraleEventKillingEnemy.Text;
	}
}
