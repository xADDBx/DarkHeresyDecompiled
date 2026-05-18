using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.QA;
using Kingmaker.RuleSystem.Rules.Modifiers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Damage;

public sealed class IntermediateDamage : IDamageTypeHolder
{
	public readonly CompositeModifiersManager Modifiers = new CompositeModifiersManager(0);

	public readonly ValueModifiersManager MinValueModifiers = new ValueModifiersManager();

	public readonly ValueModifiersManager MaxValueModifiers = new ValueModifiersManager();

	public readonly CompositeModifiersManager DamageReduction = new CompositeModifiersManager(0, 100);

	public readonly CompositeModifiersManager VitalModifiers = new CompositeModifiersManager(0);

	public readonly CompositeModifiersManager ArmorDamageModifiers = new CompositeModifiersManager(0);

	public readonly CompositeModifiersManager HealthDamageModifiers = new CompositeModifiersManager(0);

	public readonly CompositeModifiersManager CritsCountModifiers = new CompositeModifiersManager(0);

	public readonly FlagModifiersManager Avoidable = new FlagModifiersManager();

	private readonly CompositeModifiersManager m_ApplyDamageStrategyModifiers = new CompositeModifiersManager(-1, 1);

	private readonly CompositeModifiersManager m_ApplyVitalDamageStrategyModifiers = new CompositeModifiersManager(-1, 1);

	public DamageType Type { get; }

	public int MinValueBase { get; }

	public int MaxValueBase { get; }

	public bool IsCalculated { get; private set; }

	public int? CalculatedValue { get; set; }

	public bool CausedByCheckFail { get; set; }

	public BlueprintBodyPart? BodyPart { get; set; }

	public int TargetDurabilityLeft { get; set; }

	public bool CanApplyCriticalEffects { get; set; }

	public int CritsThroughArmorCount { get; set; }

	public IReadonlyModifiersComposite ApplyDamageStrategyModifiers => m_ApplyDamageStrategyModifiers;

	public IReadonlyModifiersComposite ApplyVitalDamageStrategyModifiers => m_ApplyVitalDamageStrategyModifiers;

	public DamageStrategy ApplyStrategy
	{
		get
		{
			int value = m_ApplyDamageStrategyModifiers.Value;
			if (value <= 0)
			{
				if (value < 0)
				{
					return DamageStrategy.HealthOnly;
				}
				return DamageStrategy.Default;
			}
			return DamageStrategy.ArmorOnly;
		}
	}

	public VitalDamageStrategy ApplyVitalStrategy
	{
		get
		{
			int value = m_ApplyVitalDamageStrategyModifiers.Value;
			if (value <= 0)
			{
				if (value < 0)
				{
					return VitalDamageStrategy.Never;
				}
				return VitalDamageStrategy.Default;
			}
			return VitalDamageStrategy.Always;
		}
	}

	public int VitalDamage => VitalModifiers.Value;

	public int MinValueBaseWithMinModifiers => MinValueBase + MinValueModifiers.Value;

	public int MaxValueBaseWithMaxModifiers => MaxValueBase + MaxValueModifiers.Value;

	public int MinInitialValue => Mathf.Max(0, Modifiers.Apply(MinValueBaseWithMinModifiers));

	public int MaxInitialValue => Mathf.Max(0, Modifiers.Apply(MaxValueBaseWithMaxModifiers));

	public int MinValue => ApplyDamageReduction(Modifiers.Apply(MinValueBaseWithMinModifiers));

	public int MaxValue => ApplyDamageReduction(Modifiers.Apply(MaxValueBaseWithMaxModifiers));

	public int AverageValue => Mathf.RoundToInt((float)(MinValue + MaxValue) / 2f);

	public int CritsCountBase => ConfigRoot.Instance.SystemMechanics.CriticalEffectsCountOnHit;

	public int CritsCount => CritsCountModifiers.Apply(CritsCountBase);

	public int BonusDamageToHealth => HealthDamageModifiers.Apply(0);

	public int BonusDamageToArmour => ArmorDamageModifiers.Apply(0);

	public IntermediateDamage(DamageType type, int min, int max)
	{
		Type = type;
		MinValueBase = Math.Max(min, 0);
		MaxValueBase = Math.Max(max, MinValueBase);
		if (min > max)
		{
			PFLog.Default.ErrorWithReport($"Invalid damage range: min > max, [{min}..{max}]");
		}
		if (min < 0 || max < 0)
		{
			PFLog.Default.ErrorWithReport($"Invalid damage range: min < 0 || max < 0, [{min}..{max}]");
		}
	}

	public IntermediateDamage(DamageType type, int value)
		: this(type, value, value)
	{
	}

	public IntermediateDamage(IntermediateDamage source, bool withModifiers = true, DamageType? overrideDamageType = null)
		: this(overrideDamageType ?? source.Type, source.MinValueBase, source.MaxValueBase)
	{
		CalculatedValue = source.CalculatedValue;
		CausedByCheckFail = source.CausedByCheckFail;
		BodyPart = source.BodyPart;
		TargetDurabilityLeft = source.TargetDurabilityLeft;
		CanApplyCriticalEffects = source.CanApplyCriticalEffects;
		CritsThroughArmorCount = source.CritsThroughArmorCount;
		if (withModifiers)
		{
			IsCalculated = source.IsCalculated;
			CopyModifiersFrom(source);
		}
	}

	public IntermediateDamage Copy(DamageType overrideDamageType)
	{
		return Copy(withModifiers: true, overrideDamageType);
	}

	public IntermediateDamage Copy()
	{
		return Copy(withModifiers: true);
	}

	public IntermediateDamage CopyWithoutModifiers()
	{
		return Copy(withModifiers: false);
	}

	private IntermediateDamage Copy(bool withModifiers, DamageType? overrideDamageType = null)
	{
		return new IntermediateDamage(this, withModifiers, overrideDamageType);
	}

	public void CopyModifiersFrom(IntermediateDamage source)
	{
		Modifiers.CopyFrom(source.Modifiers);
		MinValueModifiers.CopyFrom(source.MinValueModifiers);
		MaxValueModifiers.CopyFrom(source.MaxValueModifiers);
		DamageReduction.CopyFrom(source.DamageReduction);
		VitalModifiers.CopyFrom(source.VitalModifiers);
		ArmorDamageModifiers.CopyFrom(source.ArmorDamageModifiers);
		HealthDamageModifiers.CopyFrom(source.HealthDamageModifiers);
		CritsCountModifiers.CopyFrom(source.CritsCountModifiers);
		Avoidable.CopyFrom(source.Avoidable);
		m_ApplyDamageStrategyModifiers.CopyFrom(source.ApplyDamageStrategyModifiers);
		m_ApplyVitalDamageStrategyModifiers.CopyFrom(source.ApplyVitalDamageStrategyModifiers);
	}

	public int ApplyDamageReduction(int damage)
	{
		return ApplyDamageReduction(damage, DamageReduction.Value);
	}

	public int ApplyDamageReduction(int damage, int damageReduction)
	{
		return (int)((float)(damage * (100 - damageReduction)) / 100f);
	}

	public int GetMaxValueWithoutPenalties()
	{
		return Mathf.Max(0, Modifiers.Apply(MinValueBase + MinValueModifiers.GetValue(Modifier.IsPositive), Modifier.IsPositive));
	}

	public void MarkCalculated()
	{
		IsCalculated = true;
	}

	public void PushApplyStrategy(DamageStrategy strategy, EntityFactComponent? source = null, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		int num = strategy switch
		{
			DamageStrategy.ArmorOnly => 1, 
			DamageStrategy.HealthOnly => -1, 
			_ => 0, 
		};
		if (num != 0)
		{
			m_ApplyDamageStrategyModifiers.Add(ModifierType.ValAdd, num, source, descriptor);
		}
	}

	public void ForceArmorOnlyApplyStrategy(ModifierDescriptor descriptor)
	{
		m_ApplyDamageStrategyModifiers.Add(ModifierType.PctMul, 0, null, null, BonusType.None, StatType.Unknown, descriptor);
		m_ApplyDamageStrategyModifiers.Add(ModifierType.ValAdd_Extra, 1, null, null, BonusType.None, StatType.Unknown, descriptor);
	}

	public void PushApplyVitalStrategy(VitalDamageStrategy strategy, EntityFactComponent? source = null, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		int num = strategy switch
		{
			VitalDamageStrategy.Always => 1, 
			VitalDamageStrategy.Never => -1, 
			_ => 0, 
		};
		if (num != 0)
		{
			m_ApplyVitalDamageStrategyModifiers.Add(ModifierType.ValAdd, num, source, descriptor);
		}
	}
}
