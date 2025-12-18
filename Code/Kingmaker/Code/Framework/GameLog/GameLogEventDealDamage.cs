using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventDealDamage : GameLogEvent<GameLogEventDealDamage>
{
	public RuleDealDamage Damage { get; private set; }

	public static GameLogEventDealDamage Create(RuleDealDamage damage)
	{
		return new GameLogEventDealDamage(damage);
	}

	private GameLogEventDealDamage(RuleDealDamage damage)
	{
		Damage = damage;
	}
}
