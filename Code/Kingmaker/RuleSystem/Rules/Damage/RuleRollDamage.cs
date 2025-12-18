using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules.Damage;

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
	private (int damage, int minDamageValue, int minDamagePercent) ApplyDifficultyModifiers(int damage, int damageBeforeReductions)
	{
		int num = ((damage > 0) ? Math.Max(1, damage) : 0);
		int num2 = num;
		int num3 = 0;
		if (base.Initiator.IsPlayerFaction && !Target.IsPlayerFaction)
		{
			num2 = SettingsRoot.Difficulty.MinPartyDamage;
			num3 = SettingsRoot.Difficulty.MinPartyDamageFraction;
			num = Math.Max(num2, num);
			num = Math.Max(Mathf.CeilToInt((float)(damageBeforeReductions * num3) / 100f), num);
		}
		if (base.Initiator.IsPlayerEnemy && Target.IsPlayerFaction && Damage.Avoidable.Value)
		{
			int num4 = SettingsRoot.Difficulty.AvoidableDamagePercentModifier;
			float num5 = 1f + (float)num4 / 100f;
			num = (int)((float)num * num5);
		}
		return (damage: num, minDamageValue: num2, minDamagePercent: num3);
	}

	[Obsolete]
	public void NullifyDamage(EntityFact source)
	{
	}
}
