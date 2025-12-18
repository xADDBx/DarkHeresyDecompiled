using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Utility;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollChance : RuleRollD100
{
	private readonly MechanicEntity _initiator;

	private readonly ChanceRollType _rollType;

	private readonly MechanicEntity? _attackInitiator;

	private readonly StatType? _stat;

	private readonly int _successChance;

	private int? _rerollSuccessChance;

	private int? _rerollFailChance;

	private int _rerollCount;

	private BlueprintMechanicEntityFact? _rerollSuccessSource;

	private BlueprintMechanicEntityFact? _rerollFailSource;

	private bool _originalSuccess;

	public bool Success { get; private set; }

	public int OriginalSuccessChance => _successChance;

	public MechanicEntity AttackInitiator => _attackInitiator ?? _initiator;

	public ChanceRollType Type => _rollType;

	public SkillType? SkillType
	{
		get
		{
			ref readonly StatType? stat = ref _stat;
			if (!stat.HasValue || !stat.GetValueOrDefault().IsSkill())
			{
				return null;
			}
			return _stat.Value.ToSkillType();
		}
	}

	public AttributeType? AttributeType
	{
		get
		{
			ref readonly StatType? stat = ref _stat;
			if (!stat.HasValue || !stat.GetValueOrDefault().IsAttribute())
			{
				return null;
			}
			return _stat.Value.ToAttributeType();
		}
	}

	public bool IsResultOverriden => base.ResultOverride.HasValue;

	public int? RerollSuccessChance
	{
		get
		{
			if (!_originalSuccess)
			{
				return _rerollFailChance;
			}
			return _rerollSuccessChance;
		}
	}

	public string RerollSourceFactName => ((!_originalSuccess) ? _rerollFailSource?.Name : _rerollSuccessSource?.Name) ?? string.Empty;

	private RuleRollChance(MechanicEntity initiator, MechanicEntity? attackInitiator, int successChance, ChanceRollType rollType, StatType? statType)
		: base(initiator)
	{
		_initiator = initiator;
		_attackInitiator = attackInitiator;
		_successChance = successChance;
		_rollType = rollType;
		_stat = statType;
	}

	public static RuleRollChance Roll(IRuleWithChanceRoll rule)
	{
		return Roll(rule.RollType, (MechanicEntity)rule.Initiator, rule.Chance, rule.Stat, rule.AttackInitiator);
	}

	public static RuleRollChance Roll(ChanceRollType rollType, MechanicEntity initiator, int chance, StatType? statType = null, MechanicEntity? attackInitiator = null)
	{
		return Rulebook.Trigger(new RuleRollChance(initiator, attackInitiator, chance, rollType, statType));
	}

	public static RuleRollChance FromInt(IRuleWithChanceRoll rule, int roll)
	{
		return FromInt(rule.RollType, (MechanicEntity)rule.Initiator, rule.Chance, roll, rule.Stat, rule.AttackInitiator);
	}

	public static RuleRollChance FromInt(ChanceRollType rollType, MechanicEntity initiator, int chance, int roll, StatType? statType = null, MechanicEntity? attackInitiator = null)
	{
		RuleRollChance ruleRollChance = Rulebook.Trigger(new RuleRollChance(initiator, attackInitiator, chance, rollType, statType));
		ruleRollChance.Override(roll, null);
		return ruleRollChance;
	}

	public override void Override(int roll, MechanicEntityFact? source)
	{
		base.Override(roll, source);
		Success = Result <= OriginalSuccessChance;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		base.OnTrigger(context);
		bool originalSuccess = (Success = Result <= OriginalSuccessChance);
		_originalSuccess = originalSuccess;
		if (base.ResultOverride.HasValue)
		{
			return;
		}
		int? rerollSuccessChance = RerollSuccessChance;
		if (rerollSuccessChance.HasValue)
		{
			int valueOrDefault = rerollSuccessChance.GetValueOrDefault();
			int result = Result;
			int num = 0;
			while (_originalSuccess == Success && _rerollCount > num)
			{
				Reroll(base.Initiator?.MainFact);
				Success = Result <= valueOrDefault;
				num++;
			}
			PFLog.Default.Log("Reroll {0} success={1}->{2}, chance={3}->{4}, result={5}->{6}, initiator : {7} {8}, rerollCount={9}/{10}", Type, _originalSuccess, Success, OriginalSuccessChance, valueOrDefault, result, Result, base.Initiator?.UniqueId, (base.Initiator as BaseUnitEntity)?.CharacterName, num, _rerollCount);
		}
	}

	public void AddRerollSuccess(int chanceValue, int rerollCount, MechanicEntityFact sourceFact)
	{
		_rerollSuccessSource = sourceFact.Blueprint;
		_rerollSuccessChance = (_rerollSuccessChance.HasValue ? Math.Max(chanceValue, _rerollSuccessChance.Value) : chanceValue);
		_rerollCount = rerollCount;
	}

	public void AddRerollFail(int chanceValue, int maxRerollCount, MechanicEntityFact sourceFact)
	{
		_rerollFailSource = sourceFact.Blueprint;
		_rerollFailChance = (_rerollFailChance.HasValue ? Math.Max(chanceValue, _rerollFailChance.Value) : chanceValue);
		_rerollCount = maxRerollCount;
	}
}
