using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.RuleSystem.Rules.Damage;

[RuleRoles(Initiator = "damage source", Target = "damage recipient")]
public class RuleRollDamage : RulebookTargetEvent, IDamageHolderRule
{
	public IntermediateDamage Damage { get; private set; }

	public RolledDamage Result { get; private set; }

	DamageType IDamageHolderRule.DamageType => Damage.Type;

	public bool ArmorIgnore { get; set; }

	public RuleRollDamage([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] IntermediateDamage damage)
		: this((MechanicEntity)initiator, (MechanicEntity)target, damage)
	{
	}

	public RuleRollDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] IntermediateDamage damage)
		: base(initiator, target)
	{
		Damage = damage;
		ArmorIgnore = false;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!Damage.IsCalculated)
		{
			RuleCalculateDamage evt = new RuleCalculateDamage(base.Initiator, Target, base.Reason.Ability, (base.Reason.Rule as RulePerformAttack)?.RollPerformAttackRule, Damage);
			Damage = Rulebook.Trigger(evt).ResultDamage;
		}
		Result = new RolledDamage(base.Initiator, Target, Damage, RulebookEvent.RollD100());
	}

	[Obsolete]
	public void NullifyDamage(EntityFact source)
	{
	}
}
