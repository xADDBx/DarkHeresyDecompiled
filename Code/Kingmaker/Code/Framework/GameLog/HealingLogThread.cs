using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class HealingLogThread : LogThreadBase, IGameLogRuleHandler<RuleHealDamage>
{
	public void HandleEvent(RuleHealDamage rule)
	{
		if (rule.ConcreteTarget.IsInGame)
		{
			CombatLogMessage data = LogThreadBase.Strings.HealDamage.GetData(rule.CalculateHealRule);
			TooltipBaseTemplate tooltipBaseTemplate = data?.Tooltip;
			if (tooltipBaseTemplate != null)
			{
				CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, CollectExtraBricks(rule).ToArray(), arg3: false);
				CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, CollectExtraBricks(rule).ToArray(), arg3: true);
			}
			AddMessage(data);
		}
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(RuleHealDamage rule)
	{
		Func<BrickIconTextValueArgs, ITooltipBrick> iconTextTemplate = CombatLogTooltipService.CreateBrickIconTextValue;
		if (iconTextTemplate != null)
		{
			yield return iconTextTemplate(new BrickIconTextValueArgs(LogThreadBase.Strings.TooltipBrickStrings.Initiator.Text, rule.ConcreteInitiator.Name, 1));
			LocalizedString localizedString = ((rule.CalculateHealRule.Strategy == DamageStrategy.ArmorOnly) ? LogThreadBase.Strings.TooltipBrickStrings.RestoresArmor : LogThreadBase.Strings.TooltipBrickStrings.HealsWounds);
			yield return iconTextTemplate(new BrickIconTextValueArgs(localizedString.Text, rule.Value.ToString(), 1));
		}
	}
}
