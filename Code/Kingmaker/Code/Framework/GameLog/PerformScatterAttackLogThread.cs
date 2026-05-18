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
using UnityEngine.Pool;

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
		GameLogContext.AttacksCount = evt.ScatterAttacks.Count((GameLogEventAttack a) => !a.Rule.FromOverpenetration);
		GameLogEventAttack gameLogEventAttack = evt.ScatterAttacks.FindOrDefault((GameLogEventAttack o) => o != null && o.RollRuleDamage != null);
		if (gameLogEventAttack != null)
		{
			GameLogContext.DamageType = UtilityText.GetDamageTypeText(gameLogEventAttack.RollRuleDamage.Damage.Type);
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
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
				default:
					num3++;
					break;
				}
			}
		}
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
		List<GameLogEventAttack> value;
		using (CollectionPool<List<GameLogEventAttack>, GameLogEventAttack>.Get(out value))
		{
			value.AddRange(evt.ScatterAttacks);
			value.Sort((GameLogEventAttack a1, GameLogEventAttack a2) => a1.Rule.BurstIndex.CompareTo(a2.Rule.BurstIndex));
			foreach (GameLogEventAttack item in value)
			{
				GameLogContext.ResultDamage = item.Rule.ResultDamageValue;
				GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)item.Rule.Target;
				if (item.Rule.FromOverpenetration)
				{
					GameLogContext.Description = LogThreadBase.Strings.TooltipBrickStrings.FromOverpenetration.Text;
					if (item.RollPerformAttackRule.IsOverpenetration)
					{
						GameLogContext.Description = LogThreadBase.Strings.TooltipBrickStrings.FromOverpenetrationAndTriggersOverpenetration.Text;
					}
				}
				else if (item.RollPerformAttackRule.IsOverpenetration)
				{
					GameLogContext.Description = LogThreadBase.Strings.TooltipBrickStrings.TriggersOverpenetration.Text;
				}
				else
				{
					GameLogContext.Description = null;
				}
				AttackResult result = item.Rule.Result;
				string arg = ((result == AttackResult.Hit || result == AttackResult.CoverHit) ? LogThreadBase.Strings.TooltipBrickStrings.ScatterAttackHit.Text : LogThreadBase.Strings.TooltipBrickStrings.ScatterAttackMiss.Text);
				string name = $"{item.Rule.BurstIndex + 1}: {arg} {GetAttackResultText(item.Rule.Result)}";
				yield return CombatLogTooltipService.CreateBrickIconTextValue(new BrickIconTextValueArgs(name, string.Empty));
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
