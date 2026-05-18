using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules.Damage;

[RuleRoles(Initiator = "damage source", Target = "damage recipient (may be null)")]
public class RuleCalculateDamage : RulebookOptionalTargetEvent, IDamageHolderRule
{
	private readonly IntermediateDamage m_DamageModifiersHolder;

	[CanBeNull]
	public readonly RulePerformAttackRoll RollPerformAttackRule;

	public CompositeModifiersManager Modifiers => m_DamageModifiersHolder.Modifiers;

	public CompositeModifiersManager VitalModifiers => m_DamageModifiersHolder.VitalModifiers;

	public CompositeModifiersManager ArmorDamageModifiers => m_DamageModifiersHolder.ArmorDamageModifiers;

	public CompositeModifiersManager HealthDamageModifiers => m_DamageModifiersHolder.HealthDamageModifiers;

	public CompositeModifiersManager CritsCountModifiers => m_DamageModifiersHolder.CritsCountModifiers;

	public CompositeModifiersManager DamageReduction => m_DamageModifiersHolder.DamageReduction;

	public ValueModifiersManager MinValueModifiers => m_DamageModifiersHolder.MinValueModifiers;

	public ValueModifiersManager MaxValueModifiers => m_DamageModifiersHolder.MaxValueModifiers;

	public FlagModifiersManager Avoidable => m_DamageModifiersHolder.Avoidable;

	[CanBeNull]
	public BlueprintBodyPart TargetBodyPart { get; }

	[NotNull]
	private RuleCalculateStatsWeapon InitiatorWeaponStatsRule { get; }

	public bool IsDOT { get; set; }

	public IntermediateDamage ResultDamage { get; private set; }

	DamageType IDamageHolderRule.DamageType => ResultDamage?.Type ?? InitiatorWeaponStatsRule.ResultDamage?.Type ?? InitiatorWeaponStatsRule.BaseDamage?.Type ?? DamageType.None;

	[CanBeNull]
	public AbilityData Ability => InitiatorWeaponStatsRule.Ability;

	public override AbilityData MaybeAbility => Ability ?? base.MaybeAbility;

	public DamageType DamageType => InitiatorWeaponStatsRule.BaseDamage.Type;

	public RuleCalculateDamage([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] AbilityData ability, [CanBeNull] RulePerformAttackRoll performAttackRoll = null, [CanBeNull] IntermediateDamage baseDamageOverride = null, [CanBeNull] BlueprintBodyPart bodyPart = null)
		: base(initiator, target)
	{
		InitiatorWeaponStatsRule = ((baseDamageOverride == null) ? WeaponStatsHelper.GetWeaponStats(ability, ability?.Weapon, base.Initiator, MaybeTarget) : new RuleCalculateStatsWeapon(initiator, target, ability, baseDamageOverride));
		TargetBodyPart = baseDamageOverride?.BodyPart ?? bodyPart ?? performAttackRoll?.ResultHitLocation ?? target?.DefaultBodyPart;
		RollPerformAttackRule = performAttackRoll;
		m_DamageModifiersHolder = new IntermediateDamage(DamageType.Direct, 0);
		if (baseDamageOverride != null)
		{
			m_DamageModifiersHolder.CopyModifiersFrom(baseDamageOverride);
		}
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(InitiatorWeaponStatsRule);
		IntermediateDamage intermediateDamage = new IntermediateDamage(InitiatorWeaponStatsRule.ResultDamage);
		intermediateDamage.CopyModifiersFrom(m_DamageModifiersHolder);
		ApplyDifficultyDamageModifiers(intermediateDamage);
		StatContext ctx = new StatContext(null, base.Initiator?.Actor, null, null, intermediateDamage.Type, TargetBodyPart, this);
		int valueOrDefault = (MaybeTarget?.Actor?.GetStat(StatType.ArmorDamageReduction, null, ctx, "OnTrigger").ModifiedValue).GetValueOrDefault();
		if (valueOrDefault > 0)
		{
			intermediateDamage.DamageReduction.Add(ModifierType.ValAdd, valueOrDefault, this, ModifierDescriptor.BaseValue);
		}
		ItemEntityWeapon weapon = InitiatorWeaponStatsRule.Weapon;
		if (weapon != null)
		{
			StatFactionModifierConfig[] fractionModifiers = weapon.GetFractionModifiers();
			foreach (StatFactionModifierConfig statFactionModifierConfig in fractionModifiers)
			{
				if (statFactionModifierConfig.Stat == StatType.ItemWeaponDamageReductionIgnore)
				{
					intermediateDamage.DamageReduction.Add(ModifierType.ValAdd, -statFactionModifierConfig.Value, this, ModifierDescriptor.Faction);
				}
			}
		}
		if (intermediateDamage.ApplyVitalStrategy != VitalDamageStrategy.Always)
		{
			BlueprintBodyPart targetBodyPart = TargetBodyPart;
			if (targetBodyPart == null || !targetBodyPart.IsVital)
			{
				goto IL_0245;
			}
		}
		if (!IsDOT)
		{
			int statBonus = base.Initiator.Actor.GetStatBonus(StatType.Perception);
			int statBonus2 = base.Initiator.Actor.GetStatBonus(StatType.Intelligence);
			int value = statBonus + statBonus2;
			intermediateDamage.VitalModifiers.Add(ModifierType.ValAdd, value, this, ModifierDescriptor.BaseValue);
			ItemEntityWeapon weapon2 = InitiatorWeaponStatsRule.Weapon;
			if (weapon2 != null)
			{
				intermediateDamage.VitalModifiers.Add(ModifierType.ValAdd, weapon2.DamageVital, this, ModifierDescriptor.Weapon);
			}
			ItemEntityWeapon weapon3 = InitiatorWeaponStatsRule.Weapon;
			if (weapon3 != null)
			{
				StatFactionModifierConfig[] fractionModifiers = weapon3.GetFractionModifiers();
				foreach (StatFactionModifierConfig statFactionModifierConfig2 in fractionModifiers)
				{
					if (statFactionModifierConfig2.Stat == StatType.ItemWeaponVitalDamage)
					{
						intermediateDamage.VitalModifiers.Add(statFactionModifierConfig2.ModifierType, statFactionModifierConfig2.Value, this, ModifierDescriptor.Faction);
					}
				}
			}
			if (intermediateDamage.ApplyVitalStrategy != VitalDamageStrategy.Always)
			{
				BlueprintBodyPart targetBodyPart = TargetBodyPart;
				if (targetBodyPart == null || !targetBodyPart.IsVital)
				{
					goto IL_0245;
				}
			}
			intermediateDamage.VitalModifiers.Add(ModifierType.PctAdd, TargetBodyPart.VitalDamageIncrease, this, ModifierDescriptor.BodyPartVital);
		}
		goto IL_0245;
		IL_0245:
		PartHealth healthOptional = Target.GetHealthOptional();
		if (intermediateDamage.ApplyStrategy == DamageStrategy.HealthOnly && healthOptional != null && healthOptional.IsForbidDirectHpDamage)
		{
			intermediateDamage.Modifiers.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Mechanism);
		}
		if (healthOptional != null && healthOptional.IsCountHpAsArmor)
		{
			intermediateDamage.HealthDamageModifiers.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Mechanism);
			intermediateDamage.VitalModifiers.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Mechanism);
			if (intermediateDamage.ApplyStrategy != DamageStrategy.HealthOnly)
			{
				intermediateDamage.ForceArmorOnlyApplyStrategy(ModifierDescriptor.Mechanism);
			}
		}
		ResultDamage = new IntermediateDamage(intermediateDamage)
		{
			BodyPart = TargetBodyPart
		};
		ResultDamage.MarkCalculated();
	}

	private void ApplyDifficultyDamageModifiers(IntermediateDamage damage)
	{
		bool num = base.Initiator?.IsPlayerFaction ?? false;
		bool flag = base.Initiator?.IsPlayerEnemy ?? false;
		if (num)
		{
			int num2 = SettingsRoot.Difficulty.PartyDamageModifier;
			if (num2 != 0)
			{
				damage.Modifiers.Add(ModifierType.PctMul_Extra, 100 + num2, this, ModifierDescriptor.Difficulty);
			}
		}
		else if (flag)
		{
			int num3 = SettingsRoot.Difficulty.EnemyDamageModifier;
			if (num3 != 0)
			{
				damage.Modifiers.Add(ModifierType.PctMul_Extra, 100 + num3, this, ModifierDescriptor.Difficulty);
			}
			int cr = (base.Initiator as BaseUnitEntity)?.CR ?? 0;
			float damageFactor = DifficultyUtils.GetDamageFactor(SettingsRoot.Difficulty.EnemyDamage, cr);
			if (!Mathf.Approximately(damageFactor, 1f))
			{
				int value = Mathf.RoundToInt(damageFactor * 100f);
				damage.Modifiers.Add(ModifierType.PctMul_Extra, value, this, ModifierDescriptor.EnemyCombatVeterancy);
			}
		}
	}

	public void PushApplyStrategy(DamageStrategy strategy, [CanBeNull] EntityFactComponent source = null, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		m_DamageModifiersHolder.PushApplyStrategy(strategy, source, descriptor);
	}

	public void PushApplyStrategy(VitalDamageStrategy strategy, [CanBeNull] EntityFactComponent source = null, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		m_DamageModifiersHolder.PushApplyVitalStrategy(strategy, source, descriptor);
	}
}
