using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.RuleSystem.Rules.RuleBurst;

public class RuleCalculateWeightBurstTarget : RulebookTargetEvent
{
	public int Result;

	public ValueModifiersManager ValueModifier = new ValueModifiersManager();

	private BurstWeightSettings m_BurstWeightSettings;

	public RuleCalculateWeightBurstTarget([NotNull] MechanicEntity initiator, MechanicEntity target, BurstWeightSettings burstWeightSettings)
		: base(initiator, target)
	{
		m_BurstWeightSettings = burstWeightSettings;
		ValueModifier.Add(m_BurstWeightSettings.GetEntityWeight(target), this, ModifierDescriptor.BaseValue);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Result = Math.Max(0, ValueModifier.Value);
	}
}
