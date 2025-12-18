using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollInitiative : RulebookEvent
{
	public readonly ValueModifiersManager Modifiers = new ValueModifiersManager();

	private readonly int? m_OverrideResult;

	public float Result { get; private set; }

	public RuleRollD100 ResultD100 { get; private set; }

	public int Modifier => Modifiers.Value;

	public bool IsOverriden => m_OverrideResult.HasValue;

	public RuleRollInitiative(MechanicEntity initiator, int? overrideResult = null)
		: base(initiator)
	{
		m_OverrideResult = overrideResult;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		ResultD100 = RulebookEvent.RollD100();
		int num = base.ConcreteInitiator.GetAttributeOptional(StatType.Agility)?.WarhammerBonus ?? 0;
		if (num > 0)
		{
			Modifiers.Add(num, this, StatType.Agility);
		}
		int num2 = base.ConcreteInitiator.GetAttributeOptional(StatType.Perception)?.WarhammerBonus ?? 0;
		if (num2 > 0)
		{
			Modifiers.Add(num2 / 2, this, StatType.Perception);
		}
		int num3 = (ResultD100.Result + 9) / 10;
		Result = ((float?)m_OverrideResult) ?? Math.Max((float)(num3 + Modifiers.Value) + (float)(num + num2 / 2) / 100f, 1f);
	}
}
