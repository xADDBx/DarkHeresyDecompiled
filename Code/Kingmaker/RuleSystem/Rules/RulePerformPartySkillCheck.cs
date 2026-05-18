using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformPartySkillCheck : RulebookEvent
{
	private readonly StatType m_StatType;

	private readonly int m_DifficultyClass;

	private readonly bool m_ApplyPartyChecksInCapital;

	private RuleRollD100 m_D100;

	private int m_StatValue;

	private RulePerformSkillCheck m_SourceCheck;

	public readonly ValueModifiersManager ForceResultModifiers = new ValueModifiersManager();

	public BaseUnitEntity Roller { get; private set; }

	public RulePerformSkillCheck SkillCheck { get; private set; }

	public RulePerformSkillCheck.VoicingType Voice { get; set; }

	public StatType StatType => m_StatType;

	public int Difficulty
	{
		get
		{
			int difficultyClass = m_DifficultyClass;
			BaseUnitEntity initiatorUnit = base.InitiatorUnit;
			return difficultyClass + ((initiatorUnit != null && initiatorUnit.IsPlayerFaction) ? ((int)SettingsRoot.Difficulty.SkillCheckModifier) : 0);
		}
	}

	public int ResultDC => SkillCheck?.Difficulty ?? Difficulty;

	public bool Success
	{
		get
		{
			if (RollResult > TotalSkill)
			{
				return m_StatType == StatType.Unknown;
			}
			return true;
		}
	}

	public int TotalSkill => SkillCheck.EffectiveSkill;

	public int RollResult => m_D100;

	public int StatValue => m_StatValue;

	public RuleRollD100 D100 => m_D100;

	public RulePerformPartySkillCheck(RulePerformSkillCheck skillCheck)
		: base(Game.Instance.Player.MainCharacter.Entity)
	{
		m_ApplyPartyChecksInCapital = true;
		m_SourceCheck = skillCheck;
		m_DifficultyClass = skillCheck.Difficulty;
		m_StatType = skillCheck.StatType;
		Voice = skillCheck.Voice;
		ForceResultModifiers.CopyFrom(skillCheck.ChanceRule.ForceResultModifiers);
	}

	public RulePerformPartySkillCheck(StatType statType, int difficultyClass, bool applyPartyChecksInCapital = true)
		: base(Game.Instance.Player.MainCharacter.Entity)
	{
		m_StatType = statType;
		m_DifficultyClass = difficultyClass;
		m_ApplyPartyChecksInCapital = applyPartyChecksInCapital;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Calculate(isTrigger: true);
	}

	public void Calculate(bool isTrigger = false, bool doCheck = true)
	{
		m_StatValue = int.MinValue;
		Roller = null;
		foreach (BaseUnitEntity applicableUnit in GetApplicableUnits())
		{
			if (applicableUnit.CanAct)
			{
				int num = applicableUnit.Actor.GetStat(m_StatType, null, default(StatContext), "Calculate");
				if (m_StatValue < num)
				{
					m_StatValue = num;
					Roller = applicableUnit;
				}
			}
		}
		if (Roller == null)
		{
			if (m_SourceCheck != null)
			{
				Check(isTrigger, m_SourceCheck);
			}
			else
			{
				PFLog.Default.Log("Roller is null, in the party skillcheck");
			}
			return;
		}
		RulePerformSkillCheck rulePerformSkillCheck = CreateSkillCheck();
		if (!doCheck)
		{
			SkillCheck = rulePerformSkillCheck;
		}
		else
		{
			Check(isTrigger, rulePerformSkillCheck);
		}
	}

	private RulePerformSkillCheck CreateSkillCheck()
	{
		if (m_SourceCheck != null)
		{
			return new RulePerformSkillCheck(Roller, m_SourceCheck);
		}
		RulePerformSkillCheck rulePerformSkillCheck = new RulePerformSkillCheck(Roller, m_StatType, m_DifficultyClass);
		rulePerformSkillCheck.Voice = Voice;
		rulePerformSkillCheck.ChanceRule.ForceResultModifiers.CopyFrom(ForceResultModifiers);
		return rulePerformSkillCheck;
	}

	private void Check(bool isTrigger, RulePerformSkillCheck roll)
	{
		if (isTrigger)
		{
			Rulebook.Trigger(roll);
		}
		else
		{
			roll.Roll();
		}
		SkillCheck = roll;
		m_D100 = roll.RollRule;
	}

	private IEnumerable<BaseUnitEntity> GetApplicableUnits()
	{
		Player player = Game.Instance.Player;
		IEnumerable<BaseUnitEntity> enumerable = player.Party;
		if (m_ApplyPartyChecksInCapital && player.CapitalPartyMode)
		{
			IEnumerable<BaseUnitEntity> second = player.RemoteCompanions.Where((BaseUnitEntity u) => !u.IsPet);
			enumerable = enumerable.Concat(second);
		}
		return enumerable;
	}
}
