using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.TextTools.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class PerformScatterAttackLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventAbility>
{
	public void HandleEvent(GameLogEventAbility evt)
	{
		if (!evt.IsScatter)
		{
			return;
		}
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Context.Caster;
		GameLogContext.Text = evt.Ability.Weapon?.Name ?? evt.Ability.Name;
		GameLogContext.AttacksCount = evt.ScatterAttacks.Count;
		GameLogEventAttack gameLogEventAttack = evt.ScatterAttacks.FindOrDefault((GameLogEventAttack o) => o != null && o.RollRuleDamage != null);
		if (gameLogEventAttack != null)
		{
			GameLogContext.DamageType = UtilityText.GetDamageTypeText(gameLogEventAttack.RollRuleDamage.Damage.Type);
		}
		int num = 0;
		int num2 = 0;
		foreach (GameLogEventAttack scatterAttack in evt.ScatterAttacks)
		{
			if (!scatterAttack.Rule.FromOverpenetration)
			{
				switch (scatterAttack.RollPerformAttackRule.Result)
				{
				case AttackResult.Hit:
					num++;
					break;
				case AttackResult.CoverHit:
					num2++;
					break;
				}
			}
		}
		int num3 = evt.ScatterAttacks.Count - num - num2;
		StringBuilder stringBuilder = GameLogUtility.StringBuilder;
		if (num > 0)
		{
			stringBuilder.Append($"<b>{num}</b> {UIStrings.Instance.CombatLog.ScatterShotHits.Text}");
		}
		if (num2 > 0)
		{
			if (num > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append($"<b>{num2}</b> {UIStrings.Instance.CombatLog.ScatterShotCoverHits.Text}");
		}
		if (num3 > 0)
		{
			if (num > 0 || num2 > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append($"<b>{num3}</b> {UIStrings.Instance.CombatLog.ScatterShotMiss.Text}");
		}
		GameLogContext.Description = stringBuilder.ToString();
		string text = "{source} " + LogThreadBase.Strings.WarhammerScatterHitFull.Message.Text;
		string tooltipHeader = TextTemplateEngineProxy.Instance.Process(text);
		CombatLogMessage combatLogMessage = LogThreadBase.Strings.WarhammerScatterHitFull.CreateCombatLogMessage(null, tooltipHeader, isPerformAttackMessage: false, evt.Context.Caster);
		TooltipBaseTemplate tooltipBaseTemplate = combatLogMessage?.Tooltip;
		if (tooltipBaseTemplate != null)
		{
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, CollectExtraBricks(evt).ToArray(), arg3: false);
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, CollectExtraBricks(evt).ToArray(), arg3: true);
		}
		AddMessage(combatLogMessage);
		AddMessage(new CombatLogMessage(null, isSeparator: true, GameLogEventAddSeparator.States.Break));
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(GameLogEventAbility evt)
	{
		int countAttack = 0;
		foreach (GameLogEventAttack scatterAttack in evt.ScatterAttacks)
		{
			int index = scatterAttack.Rule.BurstIndex + 1;
			bool isAttack = false;
			for (; countAttack < evt.ScatterAttacks.Count; countAttack++)
			{
				GameLogEventAttack gameLogEventAttack = evt.ScatterAttacks[countAttack];
				if (gameLogEventAttack.RollPerformAttackRule.BurstIndex + 1 != index)
				{
					break;
				}
				isAttack = true;
				GameLogContext.ResultDamage = gameLogEventAttack.Rule.ResultDamageValue;
				GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)gameLogEventAttack.Rule.Target;
				GameLogContext.Description = (gameLogEventAttack.Rule.FromOverpenetration ? LogThreadBase.Strings.TooltipBrickStrings.TriggersOverpenetration.Text : null);
				AttackResult result = gameLogEventAttack.Rule.Result;
				string name = ((result == AttackResult.Hit || result == AttackResult.CoverHit) ? LogThreadBase.Strings.TooltipBrickStrings.ScatterAttackHit.Text : LogThreadBase.Strings.TooltipBrickStrings.ScatterAttackMiss.Text) + " " + GetAttackResultText(gameLogEventAttack.Rule.Result);
				yield return CombatLogTooltipService.CreateTooltipBrickIconTextValue(new TooltipBrickIconTextValueArgs(name, string.Empty));
			}
			if (!isAttack)
			{
				yield return CombatLogTooltipService.CreateTooltipBrickIconText(LogThreadBase.Strings.TooltipBrickStrings.ScatterAttackMiss.Text + " " + LogThreadBase.Strings.TooltipBrickStrings.ScatterAttackNoTarget.Text, arg2: false);
			}
		}
	}

	private static string GetAttackResultText(AttackResult result)
	{
		return result switch
		{
			AttackResult.Unknown => GameLogStrings.Instance.AttackResultStrings.AttackResultUnknown.Text, 
			AttackResult.Hit => GameLogStrings.Instance.WarhammerDealDamage.Message.Text, 
			AttackResult.CoverHit => GameLogStrings.Instance.WarhammerCoverHit.Message.Text, 
			AttackResult.Miss => GameLogStrings.Instance.WarhammerMiss.Message.Text, 
			_ => GameLogStrings.Instance.AttackResultStrings.AttackResultUnknown.Text, 
		};
	}
}
