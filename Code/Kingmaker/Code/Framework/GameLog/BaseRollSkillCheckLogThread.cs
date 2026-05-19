using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Mechanics.Facts.Interfaces;
using Owlcat.Plugins.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
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
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)null;
		GameLogContext.SourceFact = (GameLogContext.Property<IMechanicEntityFact>)(IMechanicEntityFact)null;
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)null;
		if (rule.Reason.Name.IsNullOrEmpty())
		{
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.Target;
		}
		else
		{
			GameLogContext.SourceFact = (GameLogContext.Property<IMechanicEntityFact>)(IMechanicEntityFact)rule.Reason.Fact;
		}
		if (!rule.ConcreteInitiator.Name.IsNullOrEmpty())
		{
			GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteInitiator;
		}
		GameLogContext.Text = LocalizedTexts.Instance.Stats.GetText(rule.ChanceRule.EffectiveStatType);
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
		Func<TooltipBrickTextValueArgs, ITooltipBrick> tooltipBrickTextValue = CombatLogTooltipService.CreateBrickTextValue;
		Func<BrickIconTextValueArgs, ITooltipBrick> tooltipBrickIconTextValue = CombatLogTooltipService.CreateBrickIconTextValue;
		Func<BrickChanceArgs, ITooltipBrick> createBrickChance = CombatLogTooltipService.CreateBrickChance;
		if (tooltipBrickTextValue == null || tooltipBrickIconTextValue == null || createBrickChance == null)
		{
			yield break;
		}
		TooltipBrickStrings s = LogThreadBase.Strings.TooltipBrickStrings;
		RuleRollChance rollRule = rule.RollRule;
		int rollChance = Math.Clamp(rollRule.OriginalSuccessChance, 0, 100);
		int sufficientValue = rollRule.RerollSuccessChance ?? rollChance;
		int value = ((rollRule.RollHistory.Count > 1) ? rollRule.RollHistory.LastOrDefault() : rollRule.Result);
		yield return createBrickChance(new BrickChanceArgs(s.CheckRoll.Text, sufficientValue, value, 1));
		int nestedLevel = 1;
		if (rollRule.RollHistory.Count > 1)
		{
			IEnumerable<ITooltipBrick> enumerable = LogThreadBase.ShowReroll(rollRule, rollChance);
			foreach (ITooltipBrick item in enumerable)
			{
				yield return item;
			}
			nestedLevel++;
		}
		List<Modifier> list = rule.ChanceRule.Modifiers.ToTempList();
		list.Sort(SortModifiers);
		if (list.Count > 1)
		{
			foreach (Modifier item2 in list)
			{
				string text = ((item2.Fact == null || item2.Fact.Name.IsNullOrEmpty()) ? s.BaseModifier.Text : item2.Fact.Name);
				if (item2.Value != 0)
				{
					yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(text, $"{item2.Value}%", nestedLevel));
				}
			}
		}
		else
		{
			string text2;
			if (list.Count <= 0)
			{
				text2 = rule.StatValue + "%";
			}
			else
			{
				int value2 = list[0].Value;
				text2 = value2.ToString();
			}
			string value3 = text2;
			yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(GameLogContext.Text, value3, nestedLevel));
		}
		if (rule.BaseDifficulty != 0)
		{
			string value4 = UtilityText.AddSign(rule.BaseDifficulty) + "%";
			string text3 = (rule.Reason.Name.IsNullOrEmpty() ? s.BaseModifier.Text : rule.Reason.Name);
			yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(text3, value4, nestedLevel));
		}
		foreach (Modifier item3 in rule.ChanceRule.DifficultyModifiers.List)
		{
			yield return LogThreadBase.CreateBrickModifier(item3, valueIsPercent: true, null, nestedLevel);
		}
		yield return tooltipBrickIconTextValue(new BrickIconTextValueArgs(s.Result, rule.ResultIsSuccess ? s.Success : s.Failure));
	}

	private static int SortModifiers(Modifier modifier0, Modifier modifier1)
	{
		if ((modifier0.IsBaseValue || modifier0.Descriptor == ModifierDescriptor.BaseStatBonus) && (!modifier1.IsBaseValue || modifier1.Descriptor != ModifierDescriptor.BaseStatBonus))
		{
			return -1;
		}
		if ((modifier1.IsBaseValue || modifier1.Descriptor == ModifierDescriptor.BaseStatBonus) && (!modifier0.IsBaseValue || modifier0.Descriptor != ModifierDescriptor.BaseStatBonus))
		{
			return 1;
		}
		return 0;
	}
}
