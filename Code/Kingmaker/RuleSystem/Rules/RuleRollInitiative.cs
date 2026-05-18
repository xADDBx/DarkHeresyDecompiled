using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.ContextContract;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.RuleSystem.Rules;

[RuleRoles(Initiator = "rolling unit", Target = "rolling unit (self)")]
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
		int statBonus = base.ConcreteInitiator.Actor.GetStatBonus(StatType.Agility);
		if (statBonus > 0)
		{
			Modifiers.Add(statBonus, this, StatType.Agility);
		}
		int statBonus2 = base.ConcreteInitiator.Actor.GetStatBonus(StatType.Perception);
		if (statBonus2 > 0)
		{
			Modifiers.Add(statBonus2 / 2, this, StatType.Perception);
		}
		int num = (ResultD100.Result + 9) / 10;
		Result = ((float?)m_OverrideResult) ?? Math.Max((float)(num + Modifiers.Value) + (float)(statBonus + statBonus2 / 2) / 100f, 1f);
	}
}
