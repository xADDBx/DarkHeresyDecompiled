using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Interfaces;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.View.Covers;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class PerformAttackLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventAttack>
{
	public void HandleEvent(GameLogEventAttack evt)
	{
		RulePerformAttack rule = evt.Rule;
		if (rule.Ability.Blueprint.AbilityTag == AbilityTag.ThrowingGrenade)
		{
			return;
		}
		RuleDealDamage resultDamageRule = rule.ResultDamageRule;
		if (resultDamageRule != null && resultDamageRule.IsFake)
		{
			return;
		}
		if (evt.TargetDamageList != null)
		{
			foreach (GameLogRuleEvent<RuleDealDamage> targetDamage in evt.TargetDamageList)
			{
				AddMessage(CreateMessage(evt, targetDamage.Rule));
			}
			return;
		}
		AddMessage(CreateMessage(evt));
	}

	public static CombatLogMessage CreateMessage(GameLogEventAttack evt, RuleDealDamage overrideDealDamage = null)
	{
		RulePerformAttack rule = evt.Rule;
		RuleDealDamage ruleDealDamage = overrideDealDamage ?? evt.Rule.ResultDamageRule;
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteInitiator;
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteTarget;
		GameLogContext.HitD100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)rule.RollPerformAttackRule.ResultChanceRule;
		GameLogContext.BodyPartD100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)rule.RollPerformBodyPartHitRule.ResultD100;
		GameLogContext.DefenceD100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)rule.RollPerformDefenceRule.ResultD100;
		GameLogContext.HitChance = rule.RollPerformAttackRule.HitChanceRule.ResultHitChance;
		GameLogContext.DefenceChance = rule.RollPerformAttackRule.RollPerformDefenceRule.ResultDefence;
		GameLogContext.CoverHitChance = AbilityDataHelper.CalculateCoverHitChance(rule.RollPerformAttackRule.HitChanceRule.ResultLos, rule.ConcreteTarget);
		GameLogContext.DamageReduction = ruleDealDamage?.ResultDamage.DamageReduction.Value ?? 0;
		GameLogContext.ResultDamage = ruleDealDamage?.ResultValue ?? rule.ResultDamageValue;
		if (ruleDealDamage != null)
		{
			GameLogContext.DamageType = UtilityText.GetDamageTypeText(ruleDealDamage.ResultDamage.Type);
		}
		GameLogContext.AttackNumber = rule.BurstIndex + 1;
		GameLogContext.TotalHitChance = AbilityDataHelper.CalculateHitChanceWithAvoidance((int)GameLogContext.HitChance, (int)GameLogContext.DefenceChance, (int)GameLogContext.CoverHitChance, 0f);
		GameLogContext.Text = null;
		if (evt.Rule.FromOverpenetration)
		{
			GameLogContext.Description = LogThreadBase.Strings.TooltipBrickStrings.FromOverpenetration.Text;
			if (evt.RollPerformAttackRule.IsOverpenetration)
			{
				GameLogContext.Description = LogThreadBase.Strings.TooltipBrickStrings.FromOverpenetrationAndTriggersOverpenetration.Text;
			}
		}
		else if (evt.RollPerformAttackRule.IsOverpenetration)
		{
			GameLogContext.Description = LogThreadBase.Strings.TooltipBrickStrings.TriggersOverpenetration.Text;
		}
		else
		{
			GameLogContext.Description = null;
		}
		CombatLogMessage combatLogMessage = GetMessage(evt, overrideDealDamage).CreateCombatLogMessage(null, null, isPerformAttackMessage: true, rule.ConcreteTarget);
		TooltipBaseTemplate tooltipBaseTemplate = combatLogMessage?.Tooltip;
		if (tooltipBaseTemplate != null)
		{
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, CollectExtraBricks(evt, overrideDealDamage).ToArray(), arg3: false);
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, CollectExtraBricks(evt, overrideDealDamage, isInfotip: true).ToArray(), arg3: true);
		}
		int shotNumber = ((GameLogContext.AttacksCount.Value > 1) ? GameLogContext.AttackNumber.Value : 0);
		if (combatLogMessage != null)
		{
			combatLogMessage.SetShotNumber(shotNumber);
			return combatLogMessage;
		}
		return combatLogMessage;
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(GameLogEventAttack evt, RuleDealDamage overrideDealDamage = null, bool isInfotip = false)
	{
		Func<string, ITooltipBrick> textTemplate = CombatLogTooltipService.CreateBrickText;
		Func<TooltipBrickTextValueArgs, ITooltipBrick> textValueTemplate = CombatLogTooltipService.CreateBrickTextValue;
		Func<BrickChanceArgs, ITooltipBrick> chanceTemplate = CombatLogTooltipService.CreateBrickChance;
		Func<TooltipBrickTriggeredAutoArgs, ITooltipBrick> triggeredAutoTemplate = CombatLogTooltipService.CreateBrickTriggeredAuto;
		Func<BrickIconTextValueArgs, ITooltipBrick> iconTextTemplate = CombatLogTooltipService.CreateBrickIconTextValue;
		Func<BrickDamageRangeArgs, ITooltipBrick> damageRangeTemplate = CombatLogTooltipService.CreateBrickDamageRange;
		Func<TooltipBrickElementType, ITooltipBrick> separatorTemplate = CombatLogTooltipService.CreateBrickSeparator;
		if (textValueTemplate == null || chanceTemplate == null || triggeredAutoTemplate == null || iconTextTemplate == null || damageRangeTemplate == null || separatorTemplate == null)
		{
			yield break;
		}
		RulePerformAttack rule = evt.Rule;
		RuleDealDamage resultDamage = overrideDealDamage ?? evt.Rule.ResultDamageRule;
		TooltipBrickStrings s = LogThreadBase.Strings.TooltipBrickStrings;
		bool isGrenade = rule.Ability.Blueprint.AbilityTag == AbilityTag.ThrowingGrenade;
		bool isAutoHit = rule.RollPerformAttackRule.OverrideAttackHitPolicy && rule.RollPerformAttackRule.AttackHitPolicyType == AttackHitPolicyType.AutoHit;
		int num = GameLogContext.TotalHitChance.Value;
		if (isGrenade || isAutoHit)
		{
			num = 100;
		}
		yield return textValueTemplate(new TooltipBrickTextValueArgs(s.HitChance.Text, "<b>" + num + "%</b>"));
		if (GameLogContext.HitD100.Value != null)
		{
			int sufficientValue = rule.RollPerformAttackRule.ResultChanceRule.RerollSuccessChance ?? GameLogContext.HitChance.Value;
			if (isGrenade || isAutoHit)
			{
				sufficientValue = 100;
			}
			int value = ((rule.RollPerformAttackRule.ResultChanceRule.RollHistory.Count > 1) ? rule.RollPerformAttackRule.ResultChanceRule.RollHistory.LastOrDefault() : GameLogContext.HitD100.Value.Result);
			yield return chanceTemplate(new BrickChanceArgs(s.HitRoll.Text, sufficientValue, value, 2, isResultValue: false, null, CombatLogIcon.TargetHit));
			if (isInfotip)
			{
				if (rule.RollPerformAttackRule.HitChanceRule.IsAutoHit && rule.InitiatorUnit != null)
				{
					if (rule.Target is DestructibleEntity)
					{
						yield return triggeredAutoTemplate(new TooltipBrickTriggeredAutoArgs(s.AutoHit.Text, null, isSuccess: true));
						yield return textValueTemplate(new TooltipBrickTextValueArgs(s.AutoHitDestructible.Text, null, 3));
					}
					else
					{
						yield return triggeredAutoTemplate(new TooltipBrickTriggeredAutoArgs(s.AutoHit.Text, evt.GetAutoHitAssociatedBuffs(), isSuccess: true));
					}
				}
				else if (rule.RollPerformAttackRule.HitChanceRule.IsAutoMiss && rule.InitiatorUnit != null)
				{
					yield return triggeredAutoTemplate(new TooltipBrickTriggeredAutoArgs(s.AutoMiss.Text, evt.GetAutoMissAssociatedBuffs(), isSuccess: false));
				}
				else if (isGrenade)
				{
					yield return triggeredAutoTemplate(new TooltipBrickTriggeredAutoArgs(s.AutoHit.Text, null, isSuccess: true));
					yield return textValueTemplate(new TooltipBrickTextValueArgs(s.AutoHitGrenade.Text, null, 3));
				}
				else if (isAutoHit)
				{
					yield return triggeredAutoTemplate(new TooltipBrickTriggeredAutoArgs(s.AutoHit.Text, null, isSuccess: true));
					yield return textValueTemplate(new TooltipBrickTextValueArgs(s.AoeRangedAttack.Text, null, 3));
				}
				else
				{
					if (rule.RollPerformAttackRule.ResultChanceRule.RollHistory.Count > 1)
					{
						IEnumerable<ITooltipBrick> enumerable = LogThreadBase.ShowReroll(rule.RollPerformAttackRule.ResultChanceRule, GameLogContext.HitChance.Value, isTargetHitIcon: true);
						foreach (ITooltipBrick item in enumerable)
						{
							yield return item;
						}
					}
					RuleCalculateHitChances hitChanceRule = rule.RollPerformAttackRule.HitChanceRule;
					yield return textValueTemplate(new TooltipBrickTextValueArgs(LocalizedTexts.Instance.Stats.GetText(hitChanceRule.ResultAttackStatType), "+" + hitChanceRule.ResultAttackStatValue + "%", 2));
					IEnumerable<ITooltipBrick> enumerable2 = LogThreadBase.CreateBrickModifiers(rule.RollPerformAttackRule.HitChanceRule.AllModifiersList, valueIsPercent: true, null, 2);
					foreach (ITooltipBrick item2 in enumerable2)
					{
						yield return item2;
					}
					yield return LogThreadBase.MinMaxValueBorder(rule.RollPerformAttackRule.HitChanceRule.RawResult, 0, ConfigRoot.Instance.CombatRoot.HitChanceOverkillBorder, 2, percent: true);
				}
			}
		}
		if (rule.ResultLos == LosCalculations.CoverType.Cover)
		{
			if (rule.ResultIsCoverHit)
			{
				string value2 = rule.RollPerformAttackRule.TargetUnit?.Name;
				if (!string.IsNullOrEmpty(value2))
				{
					yield return textValueTemplate(new TooltipBrickTextValueArgs(s.UnitInCover.Text, value2));
				}
				if (rule.ResultForceCoverHit)
				{
					Modifier? resultForcedCoverReason = rule.ResultForcedCoverReason;
					object obj;
					if (resultForcedCoverReason.HasValue)
					{
						Modifier valueOrDefault = resultForcedCoverReason.GetValueOrDefault();
						obj = StatModifiersBreakdown.GetBonusSourceText(valueOrDefault);
					}
					else
					{
						obj = "Unknown reason";
					}
					string value3 = (string)obj;
					yield return textValueTemplate(new TooltipBrickTextValueArgs(s.CoverForceHit.Text, value3));
				}
			}
			else
			{
				string text = rule.ResultCoverEntity?.Name;
				string text2 = ((text == null || text.Length <= 0) ? s.Environment.Text : text);
				string value4 = text2;
				yield return textValueTemplate(new TooltipBrickTextValueArgs(s.Cover, value4));
			}
		}
		if (rule.ResultHitLocation != null)
		{
			yield return textValueTemplate(new TooltipBrickTextValueArgs(s.BodyPart.Text, rule.ResultHitLocation.Name));
		}
		if (GameLogContext.DefenceD100.Value != null)
		{
			yield return textValueTemplate(new TooltipBrickTextValueArgs(s.Defence.Text, rule.RollPerformDefenceRule.IsDefended ? s.Success.Text : s.Failure.Text));
			yield return chanceTemplate(new BrickChanceArgs(s.DefenceRoll.Text, GameLogContext.DefenceChance.Value, GameLogContext.DefenceD100.Value.Result, 2, isResultValue: false, null, CombatLogIcon.Protection));
			StatQueryOutput defenceOutput = rule.RollPerformAttackRule.ResultTargetDefenceOutput;
			int num2 = ((defenceOutput != null) ? rule.RollPerformAttackRule.ResultTargetDefenceBase : 0);
			yield return textValueTemplate(new TooltipBrickTextValueArgs(s.BaseModifier.Text, num2.ToString(), 2));
			List<Modifier> list = new List<Modifier>();
			if (defenceOutput != null)
			{
				list.AddRange(defenceOutput.Modifiers);
			}
			IEnumerable<ITooltipBrick> enumerable3 = LogThreadBase.CreateBrickModifiers(list, valueIsPercent: false, null, 2);
			foreach (ITooltipBrick item3 in enumerable3)
			{
				yield return item3;
			}
			if (rule.RollPerformDefenceRule.MaxDefenceCapApplied)
			{
				yield return textValueTemplate(new TooltipBrickTextValueArgs(s.MaxDefence.Text, rule.RollPerformDefenceRule.MaxDefenceCap.ToString(), 2));
			}
		}
		else if (rule.ResultIsHit)
		{
			yield return textTemplate(rule.Target.Features.DefenceDisabled ? s.DefenceDisabled.Text : s.NoDefenceRoll.Text);
		}
		if (GameLogContext.CoverHitD100.Value != null && GameLogContext.CoverHitChance.Value > 0)
		{
			yield return chanceTemplate(new BrickChanceArgs(s.CoverHit.Text, GameLogContext.CoverHitChance.Value, GameLogContext.CoverHitD100.Value.Result, 2, isResultValue: false, null, CombatLogIcon.Protection));
		}
		yield return separatorTemplate(TooltipBrickElementType.Small);
		string attackResultText = LogThreadBase.Strings.AttackResultStrings.GetAttackResultText(rule.Result);
		yield return iconTextTemplate(new BrickIconTextValueArgs(s.Result.Text, attackResultText));
		if (resultDamage == null)
		{
			yield break;
		}
		RolledDamage damage = resultDamage.ResultDamage;
		yield return damageRangeTemplate(new BrickDamageRangeArgs(s.BaseDamage.Text, damage.BaseDamageValue, damage.BaseDamageMinValue, damage.BaseDamageMaxValue, 0, isResultValue: true, $"={damage.BaseDamageValue}"));
		ITooltipBrick[] array = LogThreadBase.GetMinMaxDamageModifiers(damage.BaseDamageMin, damage.BaseDamageMax, 2).ToArray();
		if (isInfotip && array.Length > 1)
		{
			ITooltipBrick[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				yield return array2[i];
			}
		}
		int value5 = damage.DamageReduction.Value;
		yield return iconTextTemplate(new BrickIconTextValueArgs("<b>" + s.DamageReduction.Text + "</b>", $"<b>×{(1f - (float)value5 / 100f).ToString(CultureInfo.InvariantCulture)} ({value5}%)</b>", 0, isResultValue: true));
		if (isInfotip)
		{
			foreach (ITooltipBrick item4 in LogThreadBase.GetModifiersSummary(damage.DamageReduction, 2))
			{
				yield return item4;
			}
		}
		yield return iconTextTemplate(new BrickIconTextValueArgs("<b>" + s.PlainDamage.Text + "</b>", $"<b>{damage.ResultPlainDamage}</b>", 0, isResultValue: true));
		if (isInfotip && damage.PlainDamage.List.Count > 1)
		{
			foreach (ITooltipBrick item5 in LogThreadBase.GetModifiersSummary(damage.PlainDamage, 2))
			{
				yield return item5;
			}
		}
		if (damage.ResultDamageToArmorValue > 0)
		{
			yield return iconTextTemplate(new BrickIconTextValueArgs("<b>" + s.DamageToArmor.Text + "</b>", $"<b>{damage.ResultDamageToArmorValue}</b>", 0, isResultValue: true, ""));
			if (damage.ResultPlainDamageToArmor > 0)
			{
				yield return iconTextTemplate(new BrickIconTextValueArgs(s.PlainDamage.Text, $"{damage.ResultPlainDamageToArmor}", 1, isResultValue: true));
			}
			if (damage.ResultBonusDamageToArmor > 0)
			{
				yield return iconTextTemplate(new BrickIconTextValueArgs(s.BonusDamage.Text, $"{damage.ResultBonusDamageToArmor}", 1, isResultValue: true));
				if (isInfotip)
				{
					int value6 = damage.BonusArmorDamage.Value;
					if (value6 > damage.ResultTargetArmorBeforeDamageValue)
					{
						yield return iconTextTemplate(new BrickIconTextValueArgs("Target Armor", $"{s.MaxValue.Text} {damage.ResultTargetArmorBeforeDamageValue} ({value6})", 2, isResultValue: true));
					}
					IEnumerable<ITooltipBrick> modifiersSummary = LogThreadBase.GetModifiersSummary(damage.BonusArmorDamage, 2, damage.BonusArmorBaseDamage);
					foreach (ITooltipBrick item6 in modifiersSummary)
					{
						yield return item6;
					}
				}
			}
		}
		if (damage.ResultDamageToHealthValue <= 0)
		{
			yield break;
		}
		yield return iconTextTemplate(new BrickIconTextValueArgs("<b>" + s.DamageToHealth.Text + "</b>", $"<b>{damage.ResultDamageToHealthValue}</b>", 0, isResultValue: true, ""));
		if (damage.ResultPlainDamageToHealth > 0)
		{
			yield return iconTextTemplate(new BrickIconTextValueArgs(s.PlainDamage.Text, $"{damage.ResultPlainDamageToHealth}", 1, isResultValue: true));
		}
		if (damage.ResultBonusDamageToHealth <= 0)
		{
			yield break;
		}
		CombatLogIcon iconType = (damage.IsVitalDamage ? CombatLogIcon.VitalDamage : CombatLogIcon.None);
		yield return iconTextTemplate(new BrickIconTextValueArgs(s.BonusDamage.Text, $"{damage.ResultBonusDamageToHealth}", 1, isResultValue: true, null, iconType));
		if (!isInfotip)
		{
			yield break;
		}
		IEnumerable<ITooltipBrick> modifiersSummary2 = LogThreadBase.GetModifiersSummary(damage.BonusHealthDamage, 2, damage.BonusHealthBaseDamage, damage.VitalDamage);
		foreach (ITooltipBrick item7 in modifiersSummary2)
		{
			yield return item7;
		}
	}

	public static GameLogMessage GetMessage(GameLogEventAttack evt, RuleDealDamage overrideDealDamage = null)
	{
		RulePerformAttack rule = evt.Rule;
		RuleDealDamage ruleDealDamage = overrideDealDamage ?? evt.Rule.ResultDamageRule;
		if (rule.Ability.IsBurst)
		{
			if (rule.ResultIsHit)
			{
				if (ruleDealDamage == null)
				{
					return LogThreadBase.Strings.WarhammerHitNoDamage;
				}
				if (!LogThreadBase.IsPreviousMessageUseSomething)
				{
					return LogThreadBase.Strings.WarhammerSourceDealDamage;
				}
				return LogThreadBase.Strings.WarhammerDealDamage;
			}
			return LogThreadBase.Strings.WarhammerMiss;
		}
		if (rule.ResultIsHit)
		{
			if (ruleDealDamage == null)
			{
				return LogThreadBase.Strings.WarhammerHitNoDamage;
			}
			if (!LogThreadBase.IsPreviousMessageUseSomething)
			{
				return LogThreadBase.Strings.WarhammerSourceDealDamage;
			}
			return LogThreadBase.Strings.WarhammerDealDamage;
		}
		if (rule.RollPerformDefenceRule.IsDefended)
		{
			return LogThreadBase.Strings.WarhammerDefended;
		}
		return LogThreadBase.Strings.WarhammerMiss;
	}

	private static IEnumerable<ITooltipBrick> CreateDogeBrickModifiers(IEnumerable<Modifier> allModifiers, CompositeModifiersManager weaponModifiers, bool valueIsPercent = false, string additionText = null, int nestedLevel = 0, bool isResultValue = false, bool isFirstWithoutPlus = false)
	{
		List<Modifier> list = allModifiers.ToList();
		int num = list.FindLastIndex((Modifier o) => o.Descriptor == ModifierDescriptor.ArmorPenalty);
		int num2 = list.FindLastIndex((Modifier o) => o.Descriptor == ModifierDescriptor.Weapon);
		if (num != -1 && num2 != -1)
		{
			Modifier item = list[num2];
			Modifier item2 = list[num];
			list.RemoveAt(num);
			list.Insert(list.IndexOf(item), item2);
		}
		bool printWeaponModifiers = false;
		foreach (Modifier modifier in list)
		{
			ITooltipBrick tooltipBrick = LogThreadBase.CreateBrickModifier(modifier, valueIsPercent, additionText, nestedLevel, isResultValue, isFirstWithoutPlus);
			if (tooltipBrick == null)
			{
				continue;
			}
			isFirstWithoutPlus = false;
			yield return tooltipBrick;
			if (modifier.Descriptor != ModifierDescriptor.Weapon || printWeaponModifiers)
			{
				continue;
			}
			printWeaponModifiers = true;
			if (weaponModifiers == null)
			{
				continue;
			}
			isFirstWithoutPlus = true;
			foreach (Modifier valueModifiers in weaponModifiers.ValueModifiersList)
			{
				ITooltipBrick tooltipBrick2 = LogThreadBase.CreateBrickModifier(valueModifiers, valueIsPercent, additionText, nestedLevel + 1, isResultValue, isFirstWithoutPlus);
				if (tooltipBrick2 != null)
				{
					isFirstWithoutPlus = false;
					yield return tooltipBrick2;
				}
			}
		}
	}
}
