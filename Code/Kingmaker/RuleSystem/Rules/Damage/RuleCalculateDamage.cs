using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Mechanics.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.RuleSystem.Rules.Damage;

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
		MechanicEntity maybeTarget = MaybeTarget;
		if (maybeTarget != null && maybeTarget.IsInPlayerParty)
		{
			float num = SettingsHelper.CalculateCRModifier(SettingsRoot.Difficulty.PartyDamageDealtAfterArmorReductionPercentModifier);
			int num2 = (int)((float)(int)SettingsRoot.Difficulty.PartyDamageDealtAfterArmorReductionPercentModifier * num);
			if (num2 != 0)
			{
				intermediateDamage.Modifiers.Add(ModifierType.PctMul_Extra, 100 + num2, this, ModifierDescriptor.Difficulty);
			}
		}
		ModifiableValue modifiableValue = MaybeTarget?.GetStatOptional(StatType.ArmorDamageReduction);
		int num3 = ((modifiableValue != null) ? ((int)modifiableValue) : 0);
		if (num3 > 0)
		{
			intermediateDamage.DamageReduction.Add(ModifierType.ValAdd, num3, this, ModifierDescriptor.BaseValue);
		}
		if (intermediateDamage.Type.GetInfo().IgnoreArmor)
		{
			intermediateDamage.DamageReduction.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Weapon);
		}
		BlueprintBodyPart targetBodyPart = TargetBodyPart;
		if (targetBodyPart != null && targetBodyPart.IgnoreArmorDamageReduction)
		{
			intermediateDamage.DamageReduction.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Weakpoint);
		}
		if (intermediateDamage.ApplyVitalStrategy != VitalDamageStrategy.Always)
		{
			targetBodyPart = TargetBodyPart;
			if (targetBodyPart == null || !targetBodyPart.IsVital)
			{
				goto IL_01d3;
			}
		}
		if (!IsDOT)
		{
			int num4 = base.Initiator.GetAttributeOptional(StatType.Perception)?.Bonus ?? 0;
			int num5 = base.Initiator.GetAttributeOptional(StatType.Intelligence)?.Bonus ?? 0;
			int value = num4 + num5;
			intermediateDamage.VitalModifiers.Add(ModifierType.ValAdd, value, this, ModifierDescriptor.BaseValue);
			ItemEntityWeapon weapon = InitiatorWeaponStatsRule.Weapon;
			if (weapon != null)
			{
				intermediateDamage.VitalModifiers.Add(ModifierType.ValAdd, weapon.Blueprint.DamageVital, this, ModifierDescriptor.Weapon);
			}
			if (intermediateDamage.ApplyVitalStrategy != VitalDamageStrategy.Always)
			{
				targetBodyPart = TargetBodyPart;
				if (targetBodyPart == null || !targetBodyPart.IsVital)
				{
					goto IL_01d3;
				}
			}
			intermediateDamage.VitalModifiers.Add(ModifierType.PctAdd, TargetBodyPart.VitalDamageIncrease, this, ModifierDescriptor.BodyPartVital);
		}
		goto IL_01d3;
		IL_01d3:
		maybeTarget = Target;
		if (maybeTarget != null && maybeTarget.IsMechanism)
		{
			intermediateDamage.HealthDamageModifiers.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Mechanism);
			intermediateDamage.VitalModifiers.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Mechanism);
			if (intermediateDamage.ApplyStrategy == DamageStrategy.HealthOnly)
			{
				intermediateDamage.Modifiers.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Mechanism);
			}
			else
			{
				intermediateDamage.ForceArmorOnlyApplyStrategy(ModifierDescriptor.Mechanism);
			}
		}
		intermediateDamage.CopyModifiersFrom(m_DamageModifiersHolder);
		ResultDamage = new IntermediateDamage(intermediateDamage)
		{
			BodyPart = TargetBodyPart
		};
		ResultDamage.MarkCalculated();
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
