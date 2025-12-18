using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Kingmaker.RuleSystem.Rules;

[Obsolete("Use RulePerformSkillCheck instead")]
public class RulePerformSavingThrow : RulebookEvent
{
	public readonly ValueModifiersManager ValueModifiers = new ValueModifiersManager();

	public readonly int DifficultyClassBase;

	public readonly StatType StatType;

	public readonly SavingThrowType Type;

	public RuleRollD100 D100 { get; }

	public int StatValue { get; private set; }

	private int DifficultyClassMod { get; set; }

	public bool AutoPass { get; set; }

	public int SuccessBonus { get; set; }

	[CanBeNull]
	public EntityFact SuccessBonusSource { get; set; }

	public bool? IsAlternativePassed { get; set; }

	[CanBeNull]
	public BlueprintBuff Buff { get; set; }

	public bool PersistentSpell { get; set; }

	public int DifficultyClass => DifficultyClassBase + DifficultyClassMod;

	public int RollResult
	{
		get
		{
			if (!RequiresSuccessBonus)
			{
				return BaseRollResult;
			}
			return BaseRollResult - SuccessBonus;
		}
	}

	public bool RequiresSuccessBonus
	{
		get
		{
			if (BaseRollResult > StatValue && SuccessBonus != 0)
			{
				return BaseRollResult - SuccessBonus > StatValue;
			}
			return false;
		}
	}

	public bool IsPassed
	{
		get
		{
			if (!AutoPass && !IsSuccessRoll(D100, RequiresSuccessBonus ? SuccessBonus : 0))
			{
				return IsAlternativePassed == true;
			}
			return true;
		}
	}

	public int BaseRollResult => (int)D100 + DifficultyClass;

	public bool IsSuccessRoll(int d100, int successBonus = 0)
	{
		if (d100 != 1)
		{
			if (d100 > 1)
			{
				return d100 <= StatValue + successBonus + DifficultyClass;
			}
			return false;
		}
		return true;
	}

	private static StatType GetSave(SavingThrowType type)
	{
		return type switch
		{
			SavingThrowType.Athletics => StatType.SkillAthletics, 
			SavingThrowType.Tenacity => StatType.SkillAthletics, 
			SavingThrowType.Mobility => StatType.SkillMobility, 
			SavingThrowType.Resistance => StatType.SkillResistance, 
			SavingThrowType.Demolition => StatType.SkillDemolition, 
			SavingThrowType.Reflexes => StatType.SkillReflexes, 
			SavingThrowType.SleightOfHand => StatType.SkillSleightOfHand, 
			SavingThrowType.LoreHeresy => StatType.SkillLoreHeresy, 
			SavingThrowType.LoreXenos => StatType.SkillLoreXenos, 
			SavingThrowType.LoreWarp => StatType.SkillLoreWarp, 
			SavingThrowType.TechUse => StatType.SkillTechUse, 
			SavingThrowType.Interrogation => StatType.SkillInterrogation, 
			SavingThrowType.Mettle => StatType.SkillMettle, 
			SavingThrowType.Awareness => StatType.SkillAwareness, 
			SavingThrowType.Wits => StatType.SkillWits, 
			SavingThrowType.Intimidation => StatType.SkillIntimidation, 
			SavingThrowType.Diplomacy => StatType.SkillDiplomacy, 
			SavingThrowType.Medicae => StatType.SkillMedicae, 
			_ => StatType.SkillWits, 
		};
	}

	public RulePerformSavingThrow(MechanicEntity entity, SavingThrowType saveType, int difficultyClass)
		: base(entity)
	{
		Type = saveType;
		StatType = GetSave(saveType);
		DifficultyClassBase = difficultyClass;
		D100 = new RuleRollD100(entity);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
	}

	public void AddBonusDC(int dc)
	{
		DifficultyClassMod += dc;
	}
}
