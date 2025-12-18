using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Code.Framework.GameLog;

public class MoraleEventSummary
{
	public readonly MoraleEventType EventType;

	public readonly BaseUnitEntity Initiator;

	public readonly BaseUnitEntity Target;

	public int MoraleStartValue;

	public int MoraleResultValue;

	public MoralePhaseType? MoraleResultPhase;

	private List<RulePerformMoraleChange> m_Rules = new List<RulePerformMoraleChange>();

	public RulePerformMoraleChange Rule => m_Rules[0];

	public int Count => m_Rules.Count;

	public MoraleEventSummary(RulePerformMoraleChange rule)
	{
		Initiator = rule.ConcreteInitiator as BaseUnitEntity;
		Target = rule.TargetUnit;
		EventType = rule.EventType;
		MoraleStartValue = rule.MoraleBeforeEvent;
		MoraleResultValue = rule.MoraleAfterEvent;
		MoraleResultPhase = rule.ResultMoralePhase;
		m_Rules.Add(rule);
	}

	public void AggregateWith(RulePerformMoraleChange rule)
	{
		MoraleResultValue = rule.MoraleAfterEvent;
		MoraleResultPhase = rule.ResultMoralePhase ?? MoraleResultPhase;
		m_Rules.Add(rule);
	}

	public MoraleEventSummary(IReadOnlyList<RulePerformMoraleChange> rules)
	{
		RulePerformMoraleChange rulePerformMoraleChange = rules.First();
		Initiator = rulePerformMoraleChange.ConcreteInitiator as BaseUnitEntity;
		Target = rulePerformMoraleChange.TargetUnit;
		EventType = rulePerformMoraleChange.EventType;
		MoraleStartValue = rulePerformMoraleChange.MoraleBeforeEvent;
		MoraleResultValue = rulePerformMoraleChange.MoraleAfterEvent;
		MoraleResultPhase = rulePerformMoraleChange.ResultMoralePhase;
		m_Rules.Add(rulePerformMoraleChange);
		for (int i = 1; i < rules.Count; i++)
		{
			AggregateWith(rules[i]);
		}
	}

	public bool IsEventOfSameType(RulePerformMoraleChange otherRule)
	{
		if (m_Rules[0].EventType == otherRule.EventType && m_Rules[0].ValueModifier.Value == otherRule.ValueModifier.Value)
		{
			return m_Rules[0].ResultDelta == otherRule.ResultDelta;
		}
		return false;
	}
}
