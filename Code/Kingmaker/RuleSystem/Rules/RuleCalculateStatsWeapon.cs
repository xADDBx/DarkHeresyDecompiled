using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem.Enum;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateStatsWeapon : RulebookOptionalTargetEvent
{
	public readonly IntermediateDamage BaseDamage;

	public readonly CompositeModifiersManager DamageBonusAttributeModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager SingleAdditionalHitChanceModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager BurstAdditionalHitChanceModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager AoeAdditionalHitChanceModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager MaxDistanceModifiers = new CompositeModifiersManager(1);

	public readonly CompositeModifiersManager RateOfFireModifiers = new CompositeModifiersManager(1);

	public readonly CompositeModifiersManager RecoilModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager OverpenetrationChanceModifiers = new CompositeModifiersManager();

	public readonly List<StatType> MeleeDamageStats = new List<StatType> { StatType.Strength };

	[CanBeNull]
	public AbilityData Ability { get; }

	[CanBeNull]
	public ItemEntityWeapon Weapon { get; }

	public override AbilityData MaybeAbility => Ability ?? base.MaybeAbility;

	public IntermediateDamage ResultDamage { get; private set; }

	public StatType? DamageBonusAttribute { get; private set; }

	public int ResultAdditionalSingleHitChance => SingleAdditionalHitChanceModifiers.Value;

	public int ResultAdditionalBurstHitChance => BurstAdditionalHitChanceModifiers.Value;

	public int ResultAdditionalAoeHitChance => AoeAdditionalHitChanceModifiers.Value;

	public int ResultMaxDistance => MaxDistanceModifiers.Value;

	public int ResultOptimalDistance
	{
		get
		{
			if (Weapon == null || !Weapon.Blueprint.IsRanged)
			{
				return ResultMaxDistance;
			}
			return ResultMaxDistance / 2;
		}
	}

	public int ResultRateOfFire => RateOfFireModifiers.Value;

	public int ResultOverpenetrationChance()
	{
		int num = OverpenetrationChanceModifiers.Value;
		if (Ability != null && Ability.Weapon != null)
		{
			num += Ability.Weapon.Blueprint._OverpenetrationChance;
		}
		return num;
	}

	public RuleCalculateStatsWeapon([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] AbilityData ability, [CanBeNull] IntermediateDamage baseDamageOverride = null)
		: this(initiator, target, ability?.Weapon, ability, baseDamageOverride)
	{
	}

	public RuleCalculateStatsWeapon([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] ItemEntityWeapon weapon, [CanBeNull] AbilityData ability)
		: this(initiator, target, weapon, ability, null)
	{
	}

	private RuleCalculateStatsWeapon([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] ItemEntityWeapon weapon, [CanBeNull] AbilityData ability, [CanBeNull] IntermediateDamage baseDamageOverride)
		: base(initiator, target)
	{
		Ability = ability;
		Weapon = weapon ?? ability?.Weapon;
		BaseDamage = baseDamageOverride ?? new IntermediateDamage(weapon?.Blueprint.DamageType.Type ?? Ability?.Blueprint.ElementsArray.OfType<ContextActionDealDamage>().FirstOrDefault()?.DamageType.Type ?? DamageType.Direct, weapon?.Blueprint.DamageMin ?? 0, weapon?.Blueprint.DamageMax ?? 0);
		if (Weapon != null)
		{
			MaxDistanceModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.WarhammerMaxDistance, this, ModifierDescriptor.BaseValue);
			RateOfFireModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.RateOfFire, this, ModifierDescriptor.BaseValue);
			RecoilModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.Recoil, this, ModifierDescriptor.BaseValue);
			BaseDamage.PushApplyStrategy(Weapon.Blueprint.DamageApplyStrategy, null, ModifierDescriptor.Weapon);
			BaseDamage.CritsCountModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.AdditionalCrits, this, ModifierDescriptor.Weapon);
			BaseDamage.CritsThroughArmorCount = Weapon.Blueprint.CritsThroughArmor;
			if (Weapon.Blueprint.BrutalDamage > 0)
			{
				BaseDamage.HealthDamageModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.BrutalDamage, this, ModifierDescriptor.Weapon);
			}
			if (Weapon.Blueprint.DestructiveDamage > 0)
			{
				BaseDamage.ArmorDamageModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.DestructiveDamage, this, ModifierDescriptor.Weapon);
			}
		}
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		TryApplyItemFactsManually();
		ItemEntityWeapon weapon = Weapon;
		if (weapon != null && weapon.Blueprint.AttackType == AttackType.Melee)
		{
			AbilityData ability = Ability;
			if ((object)ability != null && ability.Blueprint.IsWeaponAbility && !(base.Reason.Fact is Buff))
			{
				DamageBonusAttribute = MeleeDamageStats.MaxBy((StatType p) => (base.InitiatorUnit?.Stats.GetStat(p)?.ModifiedValue).GetValueOrDefault());
			}
		}
		StatType valueOrDefault = DamageBonusAttribute.GetValueOrDefault();
		if (valueOrDefault != 0)
		{
			float? num = Weapon?.Blueprint.DamageStatBonusFactor.GetValue();
			if (num.HasValue)
			{
				float valueOrDefault2 = num.GetValueOrDefault();
				if (valueOrDefault2 >= 0f)
				{
					DamageBonusAttributeModifiers.Add(ModifierType.PctMul, (int)(valueOrDefault2 * 100f), this, ModifierDescriptor.Weapon);
				}
			}
			int value = base.ConcreteInitiator.GetAttributeOptional(valueOrDefault)?.WarhammerBonus ?? ((int)base.ConcreteInitiator.GetStatOptional(valueOrDefault) / 10);
			int num2 = DamageBonusAttributeModifiers.Apply(value);
			BaseDamage.Modifiers.Add(ModifierType.PctAdd, num2 * 5, this, valueOrDefault);
		}
		if (Weapon != null)
		{
			if (Weapon.Blueprint.AdditionalHitChanceSingle != 0)
			{
				SingleAdditionalHitChanceModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.AdditionalHitChanceSingle, this, ModifierDescriptor.Weapon);
			}
			if (Weapon.Blueprint.AdditionalHitChanceBurst != 0)
			{
				BurstAdditionalHitChanceModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.AdditionalHitChanceBurst, this, ModifierDescriptor.Weapon);
			}
			if (Weapon.Blueprint.AdditionalHitChanceAoe != 0)
			{
				AoeAdditionalHitChanceModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.AdditionalHitChanceAoe, this, ModifierDescriptor.Weapon);
			}
			int num3 = 100 - RecoilModifiers.Value;
			if (num3 < 100)
			{
				BurstAdditionalHitChanceModifiers.Add(ModifierType.PctMul, num3, this, ModifierDescriptor.Recoil);
			}
		}
		ResultDamage = BaseDamage.Copy();
	}

	public void ReplaceDamageBonusAttribute(StatType attribute, EntityFact source)
	{
		if (!attribute.IsAttribute())
		{
			PFLog.Default.ErrorWithReport($"Invalid attribute for bonus weapon damage: {attribute}");
		}
		else
		{
			DamageBonusAttribute = attribute;
		}
	}

	private void TryApplyItemFactsManually()
	{
		if (base.ConcreteInitiator.Buffs.IsSubscribedOnEventBus)
		{
			return;
		}
		foreach (Buff rawFact in base.ConcreteInitiator.Buffs.RawFacts)
		{
			rawFact.CallComponents(delegate(IInitiatorRulebookHandler<RuleCalculateStatsWeapon> c)
			{
				try
				{
					c.OnEventAboutToTrigger(this);
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			});
		}
	}
}
