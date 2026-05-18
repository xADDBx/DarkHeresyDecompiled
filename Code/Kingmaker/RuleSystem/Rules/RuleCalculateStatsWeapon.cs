using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
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

	public readonly CompositeModifiersManager AdditionalHitChanceModifiers = new CompositeModifiersManager();

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

	public int ResultAdditionalHitChance => AdditionalHitChanceModifiers.Value;

	[Obsolete("Per-mode HitChance unified — use ResultAdditionalHitChance. Recoil is now applied in RuleCalculateHitChances. WH2-48749.")]
	public int ResultAdditionalSingleHitChance => ResultAdditionalHitChance;

	[Obsolete("Per-mode HitChance unified — use ResultAdditionalHitChance. Recoil is now applied in RuleCalculateHitChances. WH2-48749.")]
	public int ResultAdditionalBurstHitChance => ResultAdditionalHitChance;

	[Obsolete("Per-mode HitChance unified — use ResultAdditionalHitChance. Recoil is now applied in RuleCalculateHitChances. WH2-48749.")]
	public int ResultAdditionalAoeHitChance => ResultAdditionalHitChance;

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
			num += Ability.Weapon.OverpenetrationChance;
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
		BaseDamage = baseDamageOverride ?? new IntermediateDamage(weapon?.Blueprint.DamageType.Type ?? Ability?.Blueprint.ElementsArray.OfType<ContextActionDealDamage>().FirstOrDefault()?.DamageType.Type ?? DamageType.Direct, weapon?.DamageMin ?? 0, weapon?.DamageMax ?? 0);
		if (Weapon != null)
		{
			MaxDistanceModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.WarhammerMaxDistance, this, ModifierDescriptor.BaseValue);
			RateOfFireModifiers.Add(ModifierType.ValAdd, Weapon.RateOfFire, this, ModifierDescriptor.BaseValue);
			RecoilModifiers.Add(ModifierType.ValAdd, Weapon.Recoil, this, ModifierDescriptor.BaseValue);
			BaseDamage.PushApplyStrategy(Weapon.Blueprint.DamageApplyStrategy, null, ModifierDescriptor.Weapon);
			BaseDamage.CritsCountModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.AdditionalCrits, this, ModifierDescriptor.Weapon);
			BaseDamage.CritsThroughArmorCount = Weapon.Blueprint.CritsThroughArmor;
			if (Weapon.BrutalDamage > 0)
			{
				BaseDamage.HealthDamageModifiers.Add(ModifierType.ValAdd, Weapon.BrutalDamage, this, ModifierDescriptor.Brutal);
			}
			if (Weapon.DestructiveDamage > 0)
			{
				BaseDamage.ArmorDamageModifiers.Add(ModifierType.ValAdd, Weapon.DestructiveDamage, this, ModifierDescriptor.Weapon);
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
				DamageBonusAttribute = MeleeDamageStats.MaxBy((StatType p) => ((int?)base.InitiatorUnit?.Actor.GetStat(p, null, default(StatContext), "OnTrigger")) ?? 0);
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
			int statBonus = base.ConcreteInitiator.Actor.GetStatBonus(valueOrDefault);
			int num2 = DamageBonusAttributeModifiers.Apply(statBonus);
			BaseDamage.Modifiers.Add(ModifierType.PctAdd, num2 * 5, this, valueOrDefault);
		}
		if (Weapon != null)
		{
			StatFactionModifierConfig[] fractionModifiers = Weapon.GetFractionModifiers();
			foreach (StatFactionModifierConfig statFactionModifierConfig in fractionModifiers)
			{
				switch (statFactionModifierConfig.Stat)
				{
				case StatType.ItemWeaponDamage:
					BaseDamage.Modifiers.Add(statFactionModifierConfig.ModifierType, statFactionModifierConfig.Value, this, ModifierDescriptor.Faction);
					break;
				case StatType.ItemWeaponRange:
					MaxDistanceModifiers.Add(statFactionModifierConfig.ModifierType, statFactionModifierConfig.Value, this, ModifierDescriptor.Faction);
					break;
				case StatType.ItemWeaponAccuracy:
					AdditionalHitChanceModifiers.Add(statFactionModifierConfig.ModifierType, statFactionModifierConfig.Value, this, ModifierDescriptor.Faction);
					break;
				}
			}
		}
		if (Weapon != null)
		{
			int additionalHitChance = Weapon.AdditionalHitChance;
			if (additionalHitChance != 0)
			{
				AdditionalHitChanceModifiers.Add(ModifierType.ValAdd, additionalHitChance, this, ModifierDescriptor.Weapon);
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
