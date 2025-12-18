using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("1ab746cecbdf4360a26a0f60fcf45409")]
public class WarhammerHitChanceBonusAgainstLongRange : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber
{
	public int HitChanceModifier;

	public int MinimalRange;

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		if (evt.ConcreteInitiator.DistanceToInCells(evt.ConcreteTarget) >= MinimalRange)
		{
			evt.Modifiers.Add(ModifierType.ValAdd, -HitChanceModifier, base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}
}
