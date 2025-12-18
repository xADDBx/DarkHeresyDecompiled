using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateAbilityRange : RulebookEvent
{
	public int Result { get; private set; }

	public int DefaultRange { get; }

	public int Bonus { get; set; }

	public AbilityData Ability { get; }

	public int? OverrideRange { get; set; }

	public override AbilityData MaybeAbility => Ability ?? base.MaybeAbility;

	public RuleCalculateAbilityRange([NotNull] MechanicEntity initiator, AbilityData ability)
		: base(initiator)
	{
		Ability = ability;
		Bonus = 0;
		int blueprintRange = Ability.Blueprint.GetBlueprintRange();
		if (blueprintRange >= 0)
		{
			DefaultRange = blueprintRange;
		}
		else
		{
			DefaultRange = Ability.GetWeaponStats(initiator).ResultMaxDistance;
		}
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		int val = ((DefaultRange > 0) ? Math.Max(1, Ability.MinRangeCells) : 0);
		Result = Math.Max(val, (OverrideRange ?? DefaultRange) + Bonus);
	}
}
