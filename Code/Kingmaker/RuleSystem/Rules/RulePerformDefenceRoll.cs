using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Utility;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformDefenceRoll : RulebookTargetEvent<MechanicEntity, MechanicEntity>, IRuleWithChanceRoll, IRulebookEvent
{
	public bool? OverridenResult;

	public RuleRollChance ResultD100 { get; private set; }

	public RuleCalculateDefence DefenceRule { get; }

	public bool IsDefended => OverridenResult ?? ResultD100?.Success ?? false;

	int IRuleWithChanceRoll.Chance => DefenceRule.ResultDefence;

	StatType? IRuleWithChanceRoll.Stat => StatType.Defence;

	MechanicEntity IRuleWithChanceRoll.AttackInitiator => base.Initiator;

	ChanceRollType IRuleWithChanceRoll.RollType => ChanceRollType.Defence;

	public RulePerformDefenceRoll([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [CanBeNull] bool? overrideResult = null)
		: base(initiator, target)
	{
		DefenceRule = new RuleCalculateDefence(initiator, target);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!OverridenResult.HasValue)
		{
			Rulebook.Trigger(DefenceRule);
			if (DefenceRule.ResultDefence > 0)
			{
				ResultD100 = RuleRollChance.Roll(this);
			}
		}
	}
}
