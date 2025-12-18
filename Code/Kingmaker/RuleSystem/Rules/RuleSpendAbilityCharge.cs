using System;
using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules;

public class RuleSpendAbilityCharge : RulebookEvent
{
	public readonly ValueModifiersManager NotSpendChanceModifiers = new ValueModifiersManager();

	public readonly FlagModifiersManager NotSpendModifiers = new FlagModifiersManager();

	[NotNull]
	public readonly AbilityData Ability;

	public RuleRollChance NotSpendChanceRoll { get; private set; }

	public bool ResultIsSpent { get; private set; }

	public override AbilityData MaybeAbility => Ability;

	public int NotSpendChance => Math.Clamp(NotSpendChanceModifiers.Value, 0, 100);

	public bool ShouldSpend => !NotSpendModifiers.Value;

	public RuleSpendAbilityCharge([NotNull] IMechanicEntity initiator, [NotNull] AbilityData ability)
		: base(initiator)
	{
		Ability = ability;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		bool shouldSpend = ShouldSpend;
		if (shouldSpend)
		{
			NotSpendChanceRoll = RuleRollChance.Roll(ChanceRollType.NotSpendConsumable, base.Initiator, NotSpendChance);
		}
		ResultIsSpent = shouldSpend && !(NotSpendChanceRoll?.Success ?? false);
		if (ResultIsSpent)
		{
			Ability.SourceItem?.SpendCharges(Ability.Caster);
			Ability.ResourceLogic?.Spend(Ability);
		}
	}
}
