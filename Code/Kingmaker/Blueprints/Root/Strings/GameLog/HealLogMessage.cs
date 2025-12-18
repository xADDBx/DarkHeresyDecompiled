using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings.GameLog;

[Serializable]
public class HealLogMessage
{
	public Color32 Color;

	public LocalizedString HealMessage;

	public LocalizedString ArmorRestoreMessage;

	public LocalizedString HealSelfMessage;

	public LocalizedString ArmorRestoreSelfMessage;

	[CanBeNull]
	public CombatLogMessage GetData(RuleCalculateHeal rule)
	{
		using (GameLogContext.Scope)
		{
			if (rule.ResultValue <= 0)
			{
				return null;
			}
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.Initiator;
			GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteTarget;
			GameLogContext.Count = rule.ResultValue;
			string text = ((rule.Target != rule.Initiator) ? ((string)((rule.Strategy == DamageStrategy.ArmorOnly) ? ArmorRestoreMessage : HealMessage)) : ((string)((rule.Strategy == DamageStrategy.ArmorOnly) ? ArmorRestoreSelfMessage : HealSelfMessage)));
			TooltipBaseTemplate template = CombatLogTooltipService.CreateTooltipTemplateCombatLogMessage(text, null, 0f);
			PrefixIcon icon = GameLogContext.GetIcon();
			return new CombatLogMessage(text, GetColor(), icon, template, hasTooltip: true, 0, isSeparator: false, GameLogEventAddSeparator.States.Finish, isPerformAttackMessage: false, rule.ConcreteTarget);
		}
	}

	private static void AppendDiceFormula(StringBuilder sb, DiceFormula dice)
	{
		sb.Append(dice.Rolls);
		if (dice.Dice > DiceType.One)
		{
			sb.Append('d');
			sb.Append((int)dice.Dice);
		}
	}

	protected Color32 GetColor()
	{
		return Multiply((Color.r > 0 || Color.g > 0 || Color.b > 0 || Color.a > 0) ? Color : GameLogStrings.Instance.DefaultColor, GameLogStrings.Instance.ColorMultiplier);
	}

	public static Color32 Multiply(Color32 a, Color32 b)
	{
		a.r = (byte)(a.r * b.r >> 8);
		a.g = (byte)(a.g * b.g >> 8);
		a.b = (byte)(a.b * b.b >> 8);
		a.a = (byte)(a.a * b.a >> 8);
		return a;
	}
}
