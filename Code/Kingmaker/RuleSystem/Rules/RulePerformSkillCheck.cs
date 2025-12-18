using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.RuleSystem.Rules.Utility;
using Kingmaker.Settings;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformSkillCheck : RulebookTargetEvent<MechanicEntity, MechanicEntity>, IRuleWithChanceRoll, IRulebookEvent
{
	[Flags]
	public enum VoicingType
	{
		None = 0,
		Success = 1,
		Failure = 2,
		All = 3
	}

	public readonly CompositeModifiersManager DifficultyModifiers = new CompositeModifiersManager();

	public readonly ValueModifiersManager ForceResultModifiers = new ValueModifiersManager();

	public SkillCheckType Type { get; }

	public int BaseDifficulty { get; }

	public StatType StatType { get; }

	public bool IsSaveFromMaxCritStage { get; }

	public bool Silent { get; set; }

	public VoicingType Voice { get; set; }

	public bool ShowAnyway { get; set; }

	public RuleRollChance ResultChanceRule { get; private set; }

	public int Difficulty => BaseDifficulty + DifficultyModifiers.Value;

	public int StatValue => base.Initiator.GetStatOptional(StatType);

	public int EffectiveSkill => StatValue + Difficulty;

	public int RollResult => ResultChanceRule;

	public bool ResultIsSuccess => ResultChanceRule.Success;

	public bool ResultIsCriticalFail
	{
		get
		{
			if (!ResultIsSuccess)
			{
				return (int)ResultChanceRule - EffectiveSkill > 30;
			}
			return false;
		}
	}

	public int ResultDegreeOfSuccess => GetDegreeOfSuccess();

	public bool AutoSuccess => ForceResultModifiers.Value > 0;

	public bool AutoFailure => ForceResultModifiers.Value < 0;

	int IRuleWithChanceRoll.Chance => GetSuccessChance();

	StatType? IRuleWithChanceRoll.Stat => StatType;

	MechanicEntity IRuleWithChanceRoll.AttackInitiator => base.TargetUnit;

	ChanceRollType IRuleWithChanceRoll.RollType => ChanceRollType.SkillCheck;

	public RulePerformSkillCheck([NotNull] MechanicEntity unit, RulePerformSkillCheck sourceCheck)
		: base(unit, sourceCheck.TargetUnit ?? unit)
	{
		Type = sourceCheck.Type;
		StatType = sourceCheck.StatType;
		BaseDifficulty = sourceCheck.BaseDifficulty;
		IsSaveFromMaxCritStage = sourceCheck.IsSaveFromMaxCritStage;
		Silent = sourceCheck.Silent;
		Voice = sourceCheck.Voice;
		ShowAnyway = sourceCheck.ShowAnyway;
		DifficultyModifiers.CopyFrom(sourceCheck.DifficultyModifiers);
		ForceResultModifiers.CopyFrom(sourceCheck.ForceResultModifiers);
	}

	public RulePerformSkillCheck([NotNull] MechanicEntity unit, StatType statType, int difficulty, [CanBeNull] MechanicEntity attacker = null, SkillCheckType type = SkillCheckType.Default, bool isSaveFromMaxCritStage = false)
		: this(type, unit, statType, difficulty, attacker, isSaveFromMaxCritStage)
	{
	}

	public RulePerformSkillCheck([NotNull] MechanicEntity unit, SkillType skillType, int difficulty, SkillCheckType type = SkillCheckType.Default)
		: this(unit, skillType.ToStatType(), difficulty, null, type)
	{
	}

	private RulePerformSkillCheck(SkillCheckType type, [NotNull] MechanicEntity unit, StatType statType, int difficulty, [CanBeNull] MechanicEntity attacker, bool isSaveFromMaxCritStage)
		: base(unit, attacker ?? unit)
	{
		Type = type;
		StatType = statType;
		BaseDifficulty = difficulty;
		IsSaveFromMaxCritStage = isSaveFromMaxCritStage;
		if (Type != SkillCheckType.Inspect && base.Initiator.IsPlayerFaction)
		{
			DifficultyModifiers.Add(ModifierType.ValAdd, SettingsRoot.Difficulty.SkillCheckModifier, this, ModifierDescriptor.Difficulty);
		}
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (base.Initiator.GetStatOptional(StatType) == null)
		{
			PFLog.Default.Error($"Invalid stat {StatType}");
		}
		else
		{
			Roll();
		}
	}

	public void Roll()
	{
		if (ResultChanceRule == null)
		{
			int value = ForceResultModifiers.Value;
			ResultChanceRule = ((value != 0) ? RuleRollChance.FromInt(this, (value > 0) ? 1 : 100) : RuleRollChance.Roll(this));
		}
	}

	private int GetDegreeOfSuccess()
	{
		int num = EffectiveSkill - (int)ResultChanceRule;
		return num / 10 + ((num >= 0) ? 1 : (-1));
	}

	private int GetSuccessChance(int successBonus = 0)
	{
		if (StatType != 0)
		{
			return EffectiveSkill + successBonus;
		}
		return 100;
	}
}
