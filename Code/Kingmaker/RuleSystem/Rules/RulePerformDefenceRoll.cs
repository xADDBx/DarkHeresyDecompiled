using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Utility;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformDefenceRoll : RulebookTargetEvent<MechanicEntity, MechanicEntity>, IRuleWithChanceRoll, IRulebookEvent
{
	[CanBeNull]
	private readonly AbilityData m_Ability;

	public bool? OverridenResult;

	public RuleRollChance ResultD100 { get; private set; }

	public int ResultDefence { get; private set; }

	public int MaxDefenceCap { get; private set; }

	public bool MaxDefenceCapApplied { get; private set; }

	public bool IsDefended => OverridenResult ?? ResultD100?.Success ?? false;

	public override AbilityData MaybeAbility => m_Ability ?? base.MaybeAbility;

	int IRuleWithChanceRoll.Chance => ResultDefence;

	StatType? IRuleWithChanceRoll.Stat => StatType.Defence;

	MechanicEntity IRuleWithChanceRoll.AttackInitiator => base.Initiator;

	ChanceRollType IRuleWithChanceRoll.RollType => ChanceRollType.Defence;

	public RulePerformDefenceRoll([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [CanBeNull] AbilityData ability = null, [CanBeNull] bool? overrideResult = null)
		: base(initiator, target)
	{
		m_Ability = ability;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!OverridenResult.HasValue)
		{
			StatContext ctx = new StatContext(null, base.Initiator.Actor, m_Ability, null, null, null, this);
			int modifiedValue = base.Target.Actor.GetStat(StatType.Defence, null, ctx, "OnTrigger").ModifiedValue;
			MaxDefenceCap = base.Target.GetMaxDefenceCap();
			ResultDefence = base.Target.ApplyMaxDefenceCap(modifiedValue);
			MaxDefenceCapApplied = ResultDefence < modifiedValue;
			if (ResultDefence > 0)
			{
				ResultD100 = RuleRollChance.Roll(this);
			}
		}
	}
}
