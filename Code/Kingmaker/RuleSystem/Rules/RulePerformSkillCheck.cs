using System;
using JetBrains.Annotations;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.RuleSystem.Rules.Utility;

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

	public bool Silent { get; set; }

	public VoicingType Voice { get; set; }

	public bool ShowAnyway { get; set; }

	public RuleCalculateSkillCheck ChanceRule { get; }

	public RuleRollChance RollRule { get; private set; }

	public StatType StatType => ChanceRule.StatType;

	public int BaseDifficulty => ChanceRule.BaseDifficulty;

	public SkillCheckType Type => ChanceRule.Type;

	public int Difficulty => ChanceRule.Difficulty;

	public int StatValue => base.Initiator.Actor.GetStat(StatType, null, default(StatContext), "StatValue");

	public IReadonlyModifiersComposite Modifiers => ChanceRule.Modifiers;

	public int EffectiveSkill => StatValue + Difficulty;

	public int RollResult => RollRule;

	public bool ResultIsSuccess
	{
		get
		{
			if (!RollRule.Success || AutoFailure)
			{
				return AutoSuccess;
			}
			return true;
		}
	}

	public bool ResultIsCriticalFail
	{
		get
		{
			if (!ResultIsSuccess)
			{
				return (int)RollRule - EffectiveSkill > 30;
			}
			return false;
		}
	}

	public int ResultDegreeOfSuccess => GetDegreeOfSuccess();

	public bool AutoSuccess => ChanceRule.ForceResultModifiers.Value > 0;

	public bool AutoFailure => ChanceRule.ForceResultModifiers.Value < 0;

	int IRuleWithChanceRoll.Chance => ChanceRule.ResultChance;

	StatType? IRuleWithChanceRoll.Stat => ChanceRule.StatType;

	MechanicEntity IRuleWithChanceRoll.AttackInitiator => base.TargetUnit;

	ChanceRollType IRuleWithChanceRoll.RollType => ChanceRollType.SkillCheck;

	public RulePerformSkillCheck([NotNull] MechanicEntity unit, RulePerformSkillCheck sourceCheck)
		: base(unit, sourceCheck.TargetUnit ?? unit)
	{
		ChanceRule = sourceCheck.ChanceRule;
		Silent = sourceCheck.Silent;
		Voice = sourceCheck.Voice;
		ShowAnyway = sourceCheck.ShowAnyway;
	}

	public RulePerformSkillCheck([NotNull] MechanicEntity unit, SkillType skillType, int difficulty, SkillCheckType type = SkillCheckType.Default)
		: this(unit, skillType.ToStatType(), difficulty, null, type)
	{
	}

	public RulePerformSkillCheck([NotNull] MechanicEntity unit, StatType statType, int difficulty, [CanBeNull] MechanicEntity attacker = null, SkillCheckType type = SkillCheckType.Default, bool isSaveFromMaxCritStage = false)
		: this(type, unit, statType, difficulty, attacker, isSaveFromMaxCritStage)
	{
	}

	private RulePerformSkillCheck(SkillCheckType type, [NotNull] MechanicEntity unit, StatType statType, int difficulty, [CanBeNull] MechanicEntity attacker, bool isSaveFromMaxCritStage)
		: base(unit, attacker ?? unit)
	{
		ChanceRule = new RuleCalculateSkillCheck(unit, statType, difficulty, type, attacker, isSaveFromMaxCritStage);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Roll();
		Metrics.SkillCheck.Initiator(ChanceRule.Initiator.Blueprint.AssetGuid).Target(ChanceRule.Target.Blueprint.AssetGuid).Type(ChanceRule.Type)
			.Result(ResultIsSuccess)
			.Send();
	}

	public void Roll()
	{
		if (RollRule == null)
		{
			Rulebook.Trigger(ChanceRule);
			int value = ChanceRule.ForceResultModifiers.Value;
			RollRule = ((value != 0) ? RuleRollChance.FromInt(this, (value > 0) ? 1 : 100) : RuleRollChance.Roll(this));
		}
	}

	private int GetDegreeOfSuccess()
	{
		int num = EffectiveSkill - (int)RollRule;
		return num / 10 + ((num >= 0) ? 1 : (-1));
	}
}
