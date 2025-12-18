using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public abstract class BaseRollSkillCheckLogThread : LogThreadBase, IGameLogRuleHandler<RulePerformPartySkillCheck>, IGameLogRuleHandler<RulePerformSkillCheck>
{
	public virtual void HandleEvent(RulePerformPartySkillCheck check)
	{
	}

	public virtual void HandleEvent(RulePerformSkillCheck evt)
	{
		if (!evt.Silent)
		{
			LogRuleSkillCheck(evt);
		}
	}

	private void LogRuleSkillCheck(RulePerformSkillCheck rule)
	{
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteInitiator;
		GameLogContext.Text = LocalizedTexts.Instance.Stats.GetText(rule.StatType);
		CombatLogMessage combatLogMessage = (rule.ResultIsSuccess ? LogThreadBase.Strings.SkillCheckSuccess.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, rule.ConcreteInitiator) : LogThreadBase.Strings.SkillCheckFail.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, rule.ConcreteInitiator));
		TooltipBaseTemplate tooltipBaseTemplate = combatLogMessage?.Tooltip;
		if (tooltipBaseTemplate != null)
		{
			ITooltipBrick[] arg = CollectExtraBricks(rule).ToArray();
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, arg, arg3: false);
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, arg, arg3: true);
		}
		AddMessage(combatLogMessage);
	}

	public static IEnumerable<ITooltipBrick> CollectExtraBricks(RulePerformSkillCheck rule)
	{
		Func<TooltipBrickTextValueArgs, ITooltipBrick> tooltipBrickTextValue = CombatLogTooltipService.CreateTooltipBrickTextValue;
		Func<TooltipBrickIconTextValueArgs, ITooltipBrick> tooltipBrickIconTextValue = CombatLogTooltipService.CreateTooltipBrickIconTextValue;
		Func<TooltipBrickChanceArgs, ITooltipBrick> createTooltipBrickChance = CombatLogTooltipService.CreateTooltipBrickChance;
		if (tooltipBrickTextValue == null || tooltipBrickIconTextValue == null || createTooltipBrickChance == null)
		{
			yield break;
		}
		TooltipBrickStrings s = LogThreadBase.Strings.TooltipBrickStrings;
		int roleChance = Math.Clamp(rule.ResultChanceRule.OriginalSuccessChance, 0, 100);
		int sufficientValue = rule.ResultChanceRule.RerollSuccessChance ?? roleChance;
		int value = ((rule.ResultChanceRule.RollHistory.Count > 1) ? rule.ResultChanceRule.RollHistory.LastOrDefault() : rule.ResultChanceRule.Result);
		yield return createTooltipBrickChance(new TooltipBrickChanceArgs(s.CheckRoll.Text, sufficientValue, value, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true));
		int nestedLevel = 1;
		if (rule.ResultChanceRule.RollHistory.Count > 1)
		{
			IEnumerable<ITooltipBrick> enumerable = LogThreadBase.ShowReroll(rule.ResultChanceRule, roleChance);
			foreach (ITooltipBrick item in enumerable)
			{
				yield return item;
			}
			nestedLevel++;
		}
		if (rule.BaseDifficulty != 0)
		{
			string value2 = rule.BaseDifficulty + "%";
			yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(s.BaseModifier.Text, value2, nestedLevel));
		}
		string value3 = ((rule.BaseDifficulty != 0) ? UtilityText.AddSign(rule.StatValue) : rule.StatValue.ToString()) + "%";
		yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(GameLogContext.Text, value3, nestedLevel));
		foreach (Modifier item2 in rule.DifficultyModifiers.List)
		{
			yield return LogThreadBase.CreateBrickModifier(item2, valueIsPercent: true, null, nestedLevel);
		}
		yield return tooltipBrickIconTextValue(new TooltipBrickIconTextValueArgs(s.Result, rule.ResultIsSuccess ? s.Success : s.Failure));
	}
}
