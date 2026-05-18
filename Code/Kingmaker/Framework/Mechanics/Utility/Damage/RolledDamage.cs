using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;

namespace Kingmaker.Framework.Mechanics.Utility.Damage;

public sealed class RolledDamage : IDamageTypeHolder
{
	private readonly IntermediateDamage _intermediateDamage;

	private readonly ValueModifiersManager _minBaseDamage = new ValueModifiersManager(0);

	private readonly ValueModifiersManager _maxBaseDamage = new ValueModifiersManager(0);

	private readonly CompositeModifiersManager _bonusArmorBaseDamage = new CompositeModifiersManager(0);

	private readonly CompositeModifiersManager _bonusHealthBaseDamage = new CompositeModifiersManager(0);

	private readonly CompositeModifiersManager _damage = new CompositeModifiersManager(0);

	private readonly CompositeModifiersManager _bonusArmorDamage = new CompositeModifiersManager(0);

	private readonly CompositeModifiersManager _bonusHealthDamage = new CompositeModifiersManager(0);

	private readonly CompositeModifiersManager _critsCount = new CompositeModifiersManager(0);

	public readonly DamageType Type;

	public readonly float Roll;

	public readonly int? ForcedResult;

	public readonly BlueprintBodyPart? BodyPart;

	public readonly bool VitalDamageAllowed;

	public readonly bool CanApplyCrits;

	public readonly int CritsCountThroughArmorValue;

	public readonly int CritsCountDefaultValue;

	public readonly int BaseDamageMinValue;

	public readonly int BaseDamageMaxValue;

	public readonly int BaseDamageValue;

	public readonly int ResultDamageValue;

	public readonly int ResultDamageToArmorValue;

	public readonly int ResultDamageToHealthValue;

	public readonly int ResultVitalDamageValue;

	public readonly int ResultTargetArmorBeforeDamageValue;

	public readonly int ResultCritsCountValue;

	DamageType IDamageTypeHolder.Type => Type;

	public bool CausedByCheckFail => _intermediateDamage.CausedByCheckFail;

	public bool AvoidableValue => _intermediateDamage.Avoidable.Value;

	public DamageStrategy ApplyStrategyValue => _intermediateDamage.ApplyStrategy;

	public VitalDamageStrategy ApplyVitalStrategyValue => _intermediateDamage.ApplyVitalStrategy;

	public bool IsVitalDamage
	{
		get
		{
			if (VitalDamageAllowed && VitalDamage.Value > 0)
			{
				return ResultDamageToHealthValue > 0;
			}
			return false;
		}
	}

	public int ResultPlainDamage => _damage.Value;

	public int ResultBonusDamageToArmor => Math.Min(ResultTargetArmorBeforeDamageValue, BonusArmorDamage.Value);

	public int ResultPlainDamageToArmor
	{
		get
		{
			if (ResultBonusDamageToArmor >= ResultTargetArmorBeforeDamageValue)
			{
				return 0;
			}
			return Math.Min(ResultTargetArmorBeforeDamageValue - ResultBonusDamageToArmor, ResultPlainDamage);
		}
	}

	public int ResultBonusDamageToHealth => BonusHealthDamage.Value;

	public int ResultPlainDamageToHealth => ResultPlainDamage - ResultPlainDamageToArmor;

	public bool ResultIsArmorCrack
	{
		get
		{
			if (ResultDamageToArmorValue > 0)
			{
				return ResultDamageToArmorValue >= ResultTargetArmorBeforeDamageValue;
			}
			return false;
		}
	}

	public IReadonlyModifiersValue BaseDamageMin => _minBaseDamage;

	public IReadonlyModifiersValue BaseDamageMax => _maxBaseDamage;

	public IReadonlyModifiersComposite BonusArmorBaseDamage => _bonusArmorBaseDamage;

	public IReadonlyModifiersComposite BonusHealthBaseDamage => _bonusHealthBaseDamage;

	public IReadonlyModifiersComposite PlainDamage => _damage;

	public IReadonlyModifiersComposite BonusArmorDamage => _bonusArmorDamage;

	public IReadonlyModifiersComposite BonusHealthDamage => _bonusHealthDamage;

	public IReadonlyModifiersComposite CritsCountDefault => _critsCount;

	public IReadonlyModifiersComposite Modifiers => _intermediateDamage.Modifiers;

	public IReadonlyModifiersComposite DamageReduction => _intermediateDamage.DamageReduction;

	public IReadonlyModifiersComposite VitalDamage => _intermediateDamage.VitalModifiers;

	public IReadonlyModifiersFlag Avoidable => _intermediateDamage.Avoidable;

	public IReadonlyModifiersComposite ApplyStrategy => _intermediateDamage.ApplyDamageStrategyModifiers;

	public IReadonlyModifiersComposite ApplyVitalStrategy => _intermediateDamage.ApplyVitalDamageStrategyModifiers;

	public RolledDamage(MechanicEntity source, MechanicEntity target, IntermediateDamage damage, RuleRollD100 roll)
		: this(source, target, damage, roll.Result)
	{
	}

	public RolledDamage(MechanicEntity source, MechanicEntity target, IntermediateDamage damage, int roll)
	{
		_intermediateDamage = damage.Copy();
		Type = damage.Type;
		Roll = Math.Clamp((float)roll / 100f, 0f, 1f);
		ForcedResult = damage.CalculatedValue;
		BodyPart = damage.BodyPart;
		CanApplyCrits = damage.CanApplyCriticalEffects;
		CritsCountThroughArmorValue = damage.CritsThroughArmorCount;
		_critsCount.Add(ModifierType.ValAdd, ConfigRoot.Instance.SystemMechanics.CriticalEffectsCountOnHit, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.BaseValue);
		_critsCount.CopyFrom(damage.CritsCountModifiers);
		CritsCountDefaultValue = _critsCount.Value;
		Setup(source, target, damage, out BaseDamageValue, out BaseDamageMinValue, out BaseDamageMaxValue, out VitalDamageAllowed);
		Calculate(target, out ResultDamageValue, out ResultDamageToArmorValue, out ResultDamageToHealthValue, out ResultVitalDamageValue, out ResultTargetArmorBeforeDamageValue, out ResultCritsCountValue);
	}

	private void Setup(MechanicEntity source, MechanicEntity target, IntermediateDamage damage, out int baseDamageValue, out int baseDamageMinValue, out int baseDamageMaxValue, out bool vitalDamageAllowed)
	{
		_minBaseDamage.Add(damage.MinValueBase, ModifierDescriptor.BaseValue);
		_minBaseDamage.CopyFrom(damage.MinValueModifiers);
		_maxBaseDamage.Add(damage.MaxValueBase, ModifierDescriptor.BaseValue);
		_maxBaseDamage.CopyFrom(damage.MaxValueModifiers);
		baseDamageMinValue = _minBaseDamage.Value;
		baseDamageMaxValue = _maxBaseDamage.Value;
		baseDamageValue = baseDamageMinValue + Mathf.RoundToInt((float)(baseDamageMaxValue - baseDamageMinValue) * Roll);
		_damage.Add(ModifierType.ValAdd, baseDamageValue, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.BaseValue);
		_damage.CopyFrom(Modifiers);
		if (ApplyStrategyValue != DamageStrategy.HealthOnly)
		{
			if (damage.ArmorDamageModifiers.Contains((Modifier i) => i.Type == ModifierType.PctAdd && i.Value != 0))
			{
				_bonusArmorBaseDamage.Add(ModifierType.ValAdd, baseDamageValue, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.BaseValue);
				CopyBonusDamageBaseModifiers(_bonusArmorBaseDamage, Modifiers, damage.ArmorDamageModifiers);
			}
			int value = _bonusArmorBaseDamage.Value;
			_bonusArmorDamage.Add(ModifierType.ValAdd_Extra, value, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.BaseValue);
			_bonusArmorDamage.CopyFrom(Modifiers, (Modifier m) => m.IsPercent);
			_bonusArmorDamage.CopyFrom(damage.ArmorDamageModifiers);
		}
		if (ApplyStrategyValue != DamageStrategy.ArmorOnly)
		{
			if (damage.HealthDamageModifiers.Contains((Modifier i) => i.Type == ModifierType.PctAdd && i.Value != 0))
			{
				_bonusHealthBaseDamage.Add(ModifierType.ValAdd, baseDamageValue, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.BaseValue);
				CopyBonusDamageBaseModifiers(_bonusHealthBaseDamage, Modifiers, damage.HealthDamageModifiers);
			}
			int value2 = _bonusHealthBaseDamage.Value;
			_bonusHealthDamage.Add(ModifierType.ValAdd_Extra, value2, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.BaseValue);
			_bonusHealthDamage.CopyFrom(Modifiers, (Modifier m) => m.IsPercent);
			_bonusHealthDamage.CopyFrom(damage.HealthDamageModifiers);
			bool flag = ApplyStrategyValue != DamageStrategy.ArmorOnly;
			if (flag)
			{
				flag = ApplyVitalStrategyValue switch
				{
					VitalDamageStrategy.Default => BodyPart?.IsVital ?? false, 
					VitalDamageStrategy.Always => true, 
					VitalDamageStrategy.Never => false, 
					_ => throw new ArgumentOutOfRangeException("ApplyVitalStrategyValue", ApplyVitalStrategyValue, null), 
				};
			}
			vitalDamageAllowed = flag;
			if (vitalDamageAllowed)
			{
				_bonusHealthDamage.Add(ModifierType.ValAdd, VitalDamage.Value, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.Vital);
			}
		}
		else
		{
			vitalDamageAllowed = false;
		}
		int num = 100 - DamageReduction.Value;
		if (num != 100)
		{
			_damage.Add(ModifierType.PctMul_Extra, num, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.DamageReduction);
			_bonusArmorDamage.Add(ModifierType.PctMul_Extra, num, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.DamageReduction);
			_bonusHealthDamage.Add(ModifierType.PctMul_Extra, num, null, null, BonusType.None, StatType.Unknown, ModifierDescriptor.DamageReduction);
		}
	}

	private static void CopyBonusDamageBaseModifiers(CompositeModifiersManager target, IReadonlyModifiersComposite modifiers, IReadonlyModifiersComposite bonusModifiers)
	{
		foreach (Modifier item in modifiers.List)
		{
			ModifierType type = item.Type;
			if (type == ModifierType.PctMul || type == ModifierType.ValAdd)
			{
				target.Add(item);
			}
		}
		foreach (Modifier item2 in bonusModifiers.List)
		{
			if (item2.Type == ModifierType.PctAdd)
			{
				target.Add(item2.WithType(ModifierType.PctMul));
			}
			else if (item2.Type == ModifierType.PctMul)
			{
				target.Add(item2);
			}
		}
	}

	private void Calculate(MechanicEntity target, out int damage, out int armorDamage, out int healthDamage, out int vitalDamage, out int targetArmor, out int crits)
	{
		targetArmor = target.GetOptional<PartArmor>()?.DurabilityLeft ?? 0;
		vitalDamage = (VitalDamageAllowed ? VitalDamage.Value : 0);
		int num = (ForcedResult.HasValue ? Mathf.RoundToInt((float)Modifiers.ApplyPctMulExtra(ForcedResult.Value) * (1f - (float)DamageReduction.Value / 100f)) : _damage.Value);
		int val = ((!ForcedResult.HasValue) ? _bonusArmorDamage.Value : 0);
		int num2 = ((!ForcedResult.HasValue) ? _bonusHealthDamage.Value : 0);
		int num3 = 0;
		if (ApplyStrategyValue != DamageStrategy.HealthOnly)
		{
			num3 = Math.Min(targetArmor, val);
			int num4 = targetArmor - num3;
			if (num4 > 0)
			{
				int num5 = Math.Min(num4, num);
				num3 += num5;
				num -= num5;
			}
		}
		int num6 = ApplyStrategyValue switch
		{
			DamageStrategy.Default => (num3 >= targetArmor) ? (num + num2) : 0, 
			DamageStrategy.ArmorOnly => 0, 
			DamageStrategy.HealthOnly => num + num2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		damage = num3 + num6;
		healthDamage = num6;
		armorDamage = num3;
		bool ignoringCrits = target.HasMechanicFeature(MechanicsFeatureType.CannotBeCriticallyHit);
		CalculateCrits(healthDamage, armorDamage, ignoringCrits, out crits);
	}

	private void CalculateCrits(int healthDamage, int armorDamage, bool ignoringCrits, out int crits)
	{
		crits = 0;
		if (ignoringCrits)
		{
			return;
		}
		DamageStrategy applyStrategyValue = ApplyStrategyValue;
		if ((applyStrategyValue == DamageStrategy.Default || applyStrategyValue == DamageStrategy.HealthOnly) && CanApplyCrits)
		{
			if (healthDamage > 0)
			{
				crits = CritsCountDefaultValue;
			}
			else if (armorDamage > 0)
			{
				crits = CritsCountThroughArmorValue;
			}
		}
		BlueprintBodyPart bodyPart = BodyPart;
		if (bodyPart != null && bodyPart.AlwaysCriticalHit)
		{
			crits = Math.Max(crits, 1);
		}
	}

	public IntermediateDamage CopyAsIntermediateDamage()
	{
		return new IntermediateDamage(_intermediateDamage);
	}

	public int GetMaxValueWithoutPenalties()
	{
		return _intermediateDamage.GetMaxValueWithoutPenalties();
	}
}
