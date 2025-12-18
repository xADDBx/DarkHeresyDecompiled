using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Code.Framework.GameLog;

public class RulebookCastSpellLogThread : BaseUseAbilityLogThread, IGameLogRuleHandler<RulePerformAbility>
{
	public void HandleEvent(RulePerformAbility rule)
	{
		HandleUseAbility(rule.Ability, rule);
	}
}
