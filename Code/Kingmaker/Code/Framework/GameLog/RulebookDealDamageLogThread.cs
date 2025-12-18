using System;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.TextTools;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class RulebookDealDamageLogThread : LogThreadBase, IGameLogRuleHandler<RuleDealDamage>, IGameLogEventHandler<GameLogEventDealDamage>
{
	public void HandleEvent(RuleDealDamage rule)
	{
		if ((rule.ResultValue != 0 || !(rule.Target is DestructibleEntity)) && (!(rule.SourceAbility != null) || rule.SourceAbility.Blueprint.AbilityTag != AbilityTag.ThrowingGrenade) && (!(rule.SourceAbility != null) || rule.SourceAbility.Blueprint.AbilityTag != AbilityTag.ThrowingGrenade))
		{
			CombatLogMessage newMessage = CreateMessage(rule);
			AddMessage(newMessage);
		}
	}

	public void HandleEvent(GameLogEventDealDamage evt)
	{
		CombatLogMessage newMessage = CreateMessage(evt.Damage);
		AddMessage(newMessage);
	}

	public static CombatLogMessage CreateMessage(RuleDealDamage rule)
	{
		GameLogContext.Text = rule.Reason.Name;
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)(rule.Reason.Name.IsNullOrEmpty() ? rule.ConcreteInitiator : null);
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteTarget;
		GameLogContext.Description = null;
		GameLogContext.ResultDamage = rule.ResultValue;
		GameLogContext.DamageType = UtilityText.GetDamageTypeText(rule.ResultDamage.Type);
		CombatLogMessage combatLogMessage = GetMessage(rule).CreateCombatLogMessage(null, null, isPerformAttackMessage: false, rule.ConcreteTarget);
		TooltipBaseTemplate tooltipBaseTemplate = combatLogMessage?.Tooltip;
		if (tooltipBaseTemplate != null)
		{
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, CollectExtraBricks(rule).ToArray(), arg3: false);
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, CollectExtraBricks(rule, isInfotip: true).ToArray(), arg3: true);
		}
		return combatLogMessage;
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(RuleDealDamage rule, bool isInfotip = false)
	{
		Func<TooltipBrickTextValueArgs, ITooltipBrick> tooltipBrickTextValue = CombatLogTooltipService.CreateTooltipBrickTextValue;
		Func<TooltipBrickIconTextValueArgs, ITooltipBrick> tooltipBrickIconTextValue = CombatLogTooltipService.CreateTooltipBrickIconTextValue;
		Func<TooltipBrickDamageRangeArgs, ITooltipBrick> tooltipBrickDamageRange = CombatLogTooltipService.CreateTooltipBrickDamageRange;
		Func<int, string, ITooltipBrick> createTooltipBrickMinimalAdmissibleDamage = CombatLogTooltipService.CreateTooltipBrickMinimalAdmissibleDamage;
		if (tooltipBrickTextValue == null || tooltipBrickIconTextValue == null || tooltipBrickDamageRange == null || createTooltipBrickMinimalAdmissibleDamage == null)
		{
			yield break;
		}
		TooltipBrickStrings s = LogThreadBase.Strings.TooltipBrickStrings;
		RolledDamage damage = rule.ResultDamage;
		if (rule.Reason.SourceEntity != null)
		{
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.Reason.SourceEntity;
			string value = TextTemplateEngine.Instance.Process("{source}");
			yield return tooltipBrickIconTextValue(new TooltipBrickIconTextValueArgs(s.DamageSource.Text, value, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true));
		}
		if (!rule.Reason.Name.IsNullOrEmpty())
		{
			yield return tooltipBrickIconTextValue(new TooltipBrickIconTextValueArgs(s.DamageReason.Text, "<b>" + rule.Reason.Name + "</b>", 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true));
		}
		yield return tooltipBrickDamageRange(new TooltipBrickDamageRangeArgs(s.BaseDamage.Text, damage.BaseDamageValue, damage.BaseDamageMinValue, damage.BaseDamageMaxValue, 1, isResultValue: true, $"={damage.BaseDamageValue}", isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true));
		if (isInfotip)
		{
			int baseDamageMinValue = damage.BaseDamageMinValue;
			string text = baseDamageMinValue.ToString();
			baseDamageMinValue = damage.BaseDamageMaxValue;
			string value2 = text + " — " + baseDamageMinValue;
			yield return tooltipBrickTextValue(new TooltipBrickTextValueArgs(s.BaseModifier.Text, value2, 2, isResultValue: true));
			IEnumerable<ITooltipBrick> damageModifiers = LogThreadBase.GetDamageModifiers(damage, 2, minMax: true, common: true);
			foreach (ITooltipBrick item in damageModifiers)
			{
				yield return item;
			}
		}
		string attackResultText = LogThreadBase.Strings.AttackResultStrings.GetAttackResultText(AttackResult.Hit);
		yield return tooltipBrickIconTextValue(new TooltipBrickIconTextValueArgs(s.Result.Text, attackResultText, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true));
	}

	private static GameLogMessage GetMessage(RuleDealDamage rule)
	{
		if (GameLogContext.Text.Value != null)
		{
			if (!rule.ResultDamage.IsVitalDamage)
			{
				return LogThreadBase.Strings.WarhammerSourceDealDamage;
			}
			return LogThreadBase.Strings.WarhammerSourceDealVitalDamage;
		}
		return LogThreadBase.Strings.WarhammerDealDamage;
	}
}
